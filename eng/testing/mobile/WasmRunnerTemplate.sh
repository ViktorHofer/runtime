#!/usr/bin/env bash

EXECUTION_DIR=$(dirname $0)
TEST_NAME=$1
JS_ENGINE=$2
JS_ENGINE_ARGS=$3
TEST_FILTER=$4
XHARNESS_OUT="$EXECUTION_DIR/xharness-output"

if [ -z ${2+x} ]; then
	JS_ENGINE=V8
fi

if [ -z ${4+x} ]; then
    TEST_FILTER="-notrait category=IgnoreForCI -notrait category=OuterLoop -notrait category=failing"
fi

# Find a better way to express this
if [ "$TEST_NAME" == "System.IO.FileSystem.Tests" ]; then
    RUN_TESTS_JS_ARGUMENTS="--working-dir=/test-dir"
fi

if [ "$JS_ENGINE" == "V8" ]; then
	JS_ENGINE_ARGS="${JS_ENGINE_ARGS} --engine-arg=--stack-trace-limit=1000"
fi

if [ ! -z "$XHARNESS_CLI_PATH" ]; then
	# When running in CI, we only have the .NET runtime available
	# We need to call the XHarness CLI DLL directly via dotnet exec
	HARNESS_RUNNER="dotnet exec $XHARNESS_CLI_PATH"
else
	HARNESS_RUNNER="dotnet xharness"
fi

if [ -z "$XHARNESS_COMMAND" ]; then
	XHARNESS_COMMAND="test"
fi

$HARNESS_RUNNER wasm ${XHARNESS_COMMAND} --app=. --engine=${JS_ENGINE} ${JS_ENGINE_ARGS} --js-file=runtime.js --output-directory=$XHARNESS_OUT -- ${RUN_TESTS_JS_ARGUMENTS} --run WasmTestRunner.dll ${TEST_NAME}.dll ${TEST_FILTER}

_exitCode=$?

echo "XHarness artifacts: $XHARNESS_OUT"

exit $_exitCode
