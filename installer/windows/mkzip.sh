#!/bin/bash

# Zipfile building script, currently requires cygwin/*nix due to the need to
# have a 'zip' command on path.

if [ ! -d Bridge/bin/Release ]; then
 echo "This should be run from the repo's root."
 exit 1
fi

if [ ! -e Bridge/bin/Release/bridge.exe ]; then
 echo "Can't find 'bridge.exe'. Bridge CLI is probably not built."
 exit 1
fi

ver="$(egrep "^\[assembly: *AssemblyInformationalVersion\(" \
 Bridge/Properties/AssemblyInfo.cs | cut -f2 -d\")"

if [ -z "${ver}" ]; then
 echo "Unable to infer bridge-cli version."
 exit 1
fi

mkdir bridge-cli-${ver}
cp -pr Bridge/bin/Release/{bridge.exe,templates,tools} bridge-cli-${ver}/.
cp installer/windows/zipfile-readme.txt bridge-cli-${ver}/readme.txt
zip -r bridge-cli-${ver}.zip bridge-cli-${ver}

rm -rf bridge-cli-${ver}

echo "Package created at: bridge-cli-${ver}.zip"
