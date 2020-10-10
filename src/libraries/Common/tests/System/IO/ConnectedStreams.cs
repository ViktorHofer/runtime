// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    /// <summary>Provides support for in-memory producer/consumer streams.</summary>
    internal sealed class ConnectedStreams
    {
        /// <summary>Creates a pair of streams that are connected for bidirectional communication.</summary>
        /// <remarks>Writing to one stream produces data readable by the either, and vice versa.</remarks>
        public static (Stream Stream1, Stream Stream2) CreateBidirectional() =>
            CreateBidirectional(StreamBuffer.DefaultInitialBufferSize, StreamBuffer.DefaultMaxBufferSize);

        /// <summary>Creates a pair of streams that are connected for bidirectional communication.</summary>
        /// <param name="initialBufferSize">The initial buffer size to use when storing data in the connection.</param>
        /// <remarks>Writing to one stream produces data readable by the either, and vice versa.</remarks>
        public static (Stream Stream1, Stream Stream2) CreateBidirectional(int initialBufferSize) =>
            CreateBidirectional(initialBufferSize, StreamBuffer.DefaultMaxBufferSize);

        /// <summary>Creates a pair of streams that are connected for bidirectional communication.</summary>
        /// <param name="initialBufferSize">The initial buffer size to use when storing data in the connection.</param>
        /// <param name="maxBufferSize">
        /// The maximum buffer size to use when storing data in the connection.  When this limit is reached,
        /// writes will block until additional space becomes available.
        /// </param>
        /// <remarks>Writing to one stream produces data readable by the either, and vice versa.</remarks>
        public static (Stream Stream1, Stream Stream2) CreateBidirectional(int initialBufferSize, int maxBufferSize)
        {
            // Each direction needs a buffer; one stream will use b1 for reading and b2 for writing,
            // and the other stream will do the inverse.
            var b1 = new StreamBuffer(initialBufferSize, maxBufferSize);
            var b2 = new StreamBuffer(initialBufferSize, maxBufferSize);

            // Both StreamBuffers are shared between the streams: we don't want to dispose of the underlying storage
            // in the stream buffer until both streams are done with them, so we ref count and only dispose
            // when both streams have been disposed.  To share the same integer, it's put onto the heap.
            var refCount = new StrongBox<int>(2);

            return (new StreamBufferStream(b1, b2, refCount), new StreamBufferStream(b2, b1, refCount));
        }

        private sealed class StreamBufferStream : Stream
        {
            private readonly StreamBuffer _readBuffer;
            private readonly StreamBuffer _writeBuffer;
            private readonly StrongBox<int> _refCount;
            private bool _disposed;

            internal StreamBufferStream(StreamBuffer readBuffer, StreamBuffer writeBuffer, StrongBox<int> refCount)
            {
                _readBuffer = readBuffer;
                _writeBuffer = writeBuffer;
                _refCount = refCount;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && !_disposed)
                {
                    _disposed = true;

                    _readBuffer.AbortRead();
                    _writeBuffer.EndWrite();

                    if (Interlocked.Decrement(ref _refCount.Value) == 0)
                    {
                        _readBuffer.Dispose();
                        _writeBuffer.Dispose();
                    }
                }

                base.Dispose(disposing);
            }

            public override bool CanRead => true;
            public override bool CanWrite => true;
            public override bool CanSeek => false;

            public override void Flush() { }
            public override Task FlushAsync(CancellationToken cancellationToken) =>
                cancellationToken.IsCancellationRequested ? Task.FromCanceled(cancellationToken) :
                Task.CompletedTask;

            public override int Read(byte[] buffer, int offset, int count)
            {
                ValidateArgs(buffer, offset, count);
                return _readBuffer.Read(new Span<byte>(buffer, offset, count));
            }

            public override int ReadByte()
            {
                byte b = 0;
                int n = _readBuffer.Read(MemoryMarshal.CreateSpan(ref b, 1));
                return n != 0 ? b : -1;
            }

            public override int Read(Span<byte> buffer) => _readBuffer.Read(buffer);

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
                TaskToApm.Begin(ReadAsync(buffer, offset, count), callback, state);

            public override int EndRead(IAsyncResult asyncResult) => TaskToApm.End<int>(asyncResult);

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                ValidateArgs(buffer, offset, count);
                return _readBuffer.ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
                _readBuffer.ReadAsync(buffer, cancellationToken);


            public override void Write(byte[] buffer, int offset, int count)
            {
                ValidateArgs(buffer, offset, count);
                _writeBuffer.Write(new ReadOnlySpan<byte>(buffer, offset, count));
            }

            public override void WriteByte(byte value) => _writeBuffer.Write(MemoryMarshal.CreateReadOnlySpan(ref value, 1));

            public override void Write(ReadOnlySpan<byte> buffer) => _writeBuffer.Write(buffer);

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                ValidateArgs(buffer, offset, count);
                return _writeBuffer.WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _writeBuffer.WriteAsync(buffer, cancellationToken);

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
                TaskToApm.Begin(WriteAsync(buffer, offset, count), callback, state);

            public override void EndWrite(IAsyncResult asyncResult) => TaskToApm.End(asyncResult);

            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();

            private void ValidateArgs(byte[] buffer, int offset, int count)
            {
                if (buffer is null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if ((uint)offset > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                if ((uint)count > buffer.Length || offset > buffer.Length - count)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }
            }
        }
    }
}
