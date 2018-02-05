#!/bin/bash

# Zipfile building script, currently requires cygwin/*nix due to the need to
# have a 'zip' command on path.

pkgname_prefix="bridge-cli."

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
 echo "Unable to infer bridge CLI version."
 exit 1
fi

mkdir "${pkgname_prefix}${ver}"
cp -pr LICENSE Bridge/bin/Release/{bridge.exe,templates,tools} \
 "${pkgname_prefix}${ver}"/.
cp installer/windows/zipfile-readme.txt "${pkgname_prefix}${ver}"/readme.txt
zip -r "${pkgname_prefix}${ver}.zip" "${pkgname_prefix}${ver}"

rm -rf "${pkgname_prefix}${ver}"

echo "Package created at: ${pkgname_prefix}${ver}.zip"
