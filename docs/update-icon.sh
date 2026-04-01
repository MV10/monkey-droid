#!/bin/bash

# Ensures docs/icon.png is valid PNG, copies to all project locations,
# and generates the Windows .ico.

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_DIR="$(dirname "$SCRIPT_DIR")"

SOURCE="$SCRIPT_DIR/icon.png"

if [ ! -f "$SOURCE" ]; then
    echo "Error: $SOURCE not found"
    exit 1
fi

# Ensure the source is actually PNG (re-encode if JPEG or other format)
FILE_TYPE=$(file -b "$SOURCE")
if [[ "$FILE_TYPE" != PNG* ]]; then
    echo "Source is $FILE_TYPE — converting to PNG"
    magick "$SOURCE" PNG:"$SOURCE"
fi

cp "$SOURCE" "$REPO_DIR/monkeydroid/Assets/icon.png"
echo "Copied to monkeydroid/Assets/icon.png"

cp "$SOURCE" "$REPO_DIR/monkeydroid.Android/Icon.png"
echo "Copied to monkeydroid.Android/Icon.png"

magick "$SOURCE" \
    \( -clone 0 -resize 16x16 \) \
    \( -clone 0 -resize 32x32 \) \
    \( -clone 0 -resize 48x48 \) \
    \( -clone 0 -resize 64x64 \) \
    \( -clone 0 -resize 128x128 \) \
    \( -clone 0 -resize 256x256 \) \
    -delete 0 \
    "$REPO_DIR/monkeydroid.Desktop/icon.ico"
echo "Created monkeydroid.Desktop/icon.ico"
