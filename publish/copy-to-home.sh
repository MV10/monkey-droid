#!/bin/bash

echo ""
echo "======================================================="
echo "Copying monkey-droid executables to home directory"
echo "======================================================="

BASEPATH="/data/Source/monkey-droid/monkeydroid"
DESKTOP=".Desktop/bin/Release/net10.0"
ANDROID=".Android/bin/Release/net10.0"

cp "$BASEPATH$DESKTOP"/win-x64/publish/* ~
cp "$BASEPATH$DESKTOP"/linux-x64/publish/* ~
cp "$BASEPATH$ANDROID"-android/publish/* ~
