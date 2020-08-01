#!/usr/bin/env bash

EXECUTION_DIR=$(dirname $0)
TEST_NAME=$1
JS_ENGINE=$2
JS_ENGINE_ARGS=$3
XHARNESS_OUT="$EXECUTION_DIR/xharness-output"

if [ -z ${2+x} ]; then
	JS_ENGINE=V8
fi

if [ "$JS_ENGINE" == "V8" ]; then
	JS_ENGINE_ARGS=${JS_ENGINE_ARGS} --engine-arg=--stack-trace-limit=1000
fi

if [ ! -z "$XHARNESS_CLI_PATH" ]; then
	# When running in CI, we only have the .NET runtime available
	# We need to call the XHarness CLI DLL directly via dotnet exec
	HARNESS_RUNNER="dotnet exec $XHARNESS_CLI_PATH"
else
	HARNESS_RUNNER="dotnet xharness"
fi

$HARNESS_RUNNER wasm test --engine=${JSEngine} $JSEngineArgs --js-file=runtime.js -v --output-directory=$XHARNESS_OUT -- --run WasmTestRunner.dll ${TEST_NAME}.dll

_exitCode=$?

echo "XHarness artifacts: $XHARNESS_OUT"

exit $_exitCode
