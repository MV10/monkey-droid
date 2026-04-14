#!/bin/bash

echo "Check for versioning notes about Android keystore files:"
echo "/data/Source/_signing_keys/monkeydroid.keystore.readme.txt"
echo ""

KEYSTORE="/data/Source/_signing_keys/monkeydroid.keystore"
KEYSTORE_ALIAS="monkeydroid"

# Generate a release keystore if one doesn't exist
if [ ! -f "$KEYSTORE" ]; then
  echo "No release keystore found. Creating one..."
  echo "(Remember the password -- you'll need it for every release build)"
  echo ""
  keytool -genkeypair -v \
    -keystore "$KEYSTORE" \
    -alias "$KEYSTORE_ALIAS" \
    -keyalg RSA -keysize 2048 -validity 10000
  echo ""
fi

if [ -z "$1" ]; then
  echo "ERROR: Keystore password required as first argument."
  echo "Usage: $0 <keystore-password>"
  exit 1
fi
KSPASS="$1"

# Validate the password before building
if ! keytool -list -keystore "$KEYSTORE" -storepass "$KSPASS" > /dev/null 2>&1; then
  echo "ERROR: Incorrect keystore password."
  exit 1
fi

dotnet publish ../monkeydroid.Android/monkeydroid.Android.csproj \
  -c Release \
  -p:AndroidKeyStore=true \
  -p:AndroidSigningKeyStore="$KEYSTORE" \
  -p:AndroidSigningKeyAlias="$KEYSTORE_ALIAS" \
  -p:AndroidSigningKeyPass="$KSPASS" \
  -p:AndroidSigningStorePass="$KSPASS"

PUBLISH_DIR="../monkeydroid.Android/bin/Release/net10.0-android/publish"

# Remove unused files
rm -f "$PUBLISH_DIR"/_Microsoft.Android.Resource.Designer.dll

# Remove unsigned intermediate APK, rename signed APK
rm -f "$PUBLISH_DIR"/com.mindmagma.monkeydroid.apk
mv -f "$PUBLISH_DIR"/com.mindmagma.monkeydroid-Signed.apk "$PUBLISH_DIR"/com.mindmagma.monkeydroid.apk 2>/dev/null

echo ""
ls -la "$PUBLISH_DIR"
