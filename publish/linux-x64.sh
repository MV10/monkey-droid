#!/bin/bash

dotnet publish ../monkeydroid.Desktop/monkeydroid.Desktop.csproj -r linux-x64 \
  -c Release --self-contained -p:PublishReadyToRun=true -p:PublishTrimmed=true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:IncludeAllContentForSelfExtract=true -p:DeleteExistingFiles=true

PUBLISH_DIR="../monkeydroid.Desktop/bin/Release/net10.0/linux-x64/publish"

# Remove unused files
rm -f "$PUBLISH_DIR"/*.pdb

# Drop the .Desktop suffix
mv -f "$PUBLISH_DIR"/monkeydroid.Desktop "$PUBLISH_DIR"/monkeydroid

ls -la "$PUBLISH_DIR"/
