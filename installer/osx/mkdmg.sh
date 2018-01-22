#/bin/bash

# This script will create the DMG installer out of the currently built Bridge
# CLI.
# Similarly to Mono Framework DMG installer, this will install Bridge to
# /Library/Frameworks/Bridge and also install bridge to the path with the
# /etc/path.d/bridge-commands file.

# Variables
install_title="Bridge CLI Installer"
mountpoint="/Volumes/Bridge_CLI_Installer"
tempdmg="bridgecli_installer.dmg"
targetdmg="bridge-cli.dmg"

# "installer" window settings
win_top=50
win_left=10
win_height=500
win_width=600

# Basic functions
function trigger_error() {
 if [ "${1}" == "-n" ]; then
  shift
  echo "failed."
 fi
 if [ ${#@} -gt 0 ]; then
  >&2 echo "*** Error: ${@}"
 else
  >&2 echo "*** Error. Aborting script execution."
 fi
 exit 1
}

# Check for requirements/dependencies
if ! hdiutilbin="$(which hdiutil)"; then
 trigger_error "Unable to locate hdiutil."
fi

if ! osascp="$(which osascript)"; then
 trigger_error "Unable to locate apple script."
fi

clipath="Bridge/bin/Release"
paths_prefix=""

if [ ! -d "${clipath}" -a -d "../../${clipath}" ]; then
 # Running from within the installer/osx directory
 paths_prefix="../../"
 clipath="${paths_prefix}${clipath}"
fi

if [ ! -e "${clipath}/bridge.exe" ]; then
 trigger_error "Bridge executable not found. Is project built in \"Release\" mode?"
fi

if [ ! -d "${clipath}/tools" -o ! -d "${clipath}/templates" ]; then
 trigger_error "Bridge tools and/or templates folders not found on release directory.
This may happen due to a failure in the post-build event during project
compilation.
Searched bridge path: ${clipath}"
fi

if [ -d "${mountpoint}" ]; then
 trigger_error "Mount point is busy: ${mountpoint}"
fi

if [ -e "${tempdmg}" ]; then
 trigger_error "Found temporary DMG file '${tempdmg}'
This may happen on a non-clean script shutdown or a concurrently running script."
fi

scppath="$(dirname "${BASH_SOURCE[0]}")"
if [ ! -e "${scppath}/install-bg.png" ]; then
 trigger_error "Unable to locate installer's background image file."
fi

if [ -e "${targetdmg}" ]; then
 trigger_error "Target package file already exists: ${targetdmg}"
fi

echo "Bridge release path: ${clipath}"

# Create a blank DMG image and attach it
dmg_size=$(( 10#$(du -scm "${clipath}/"{bridge.exe,templates,tools} | tail -n1 | cut -f1) + 5 ))

echo -n "Creating empty DMG image: "
mkdir empty_folder > /dev/null 2>&1 || \
 trigger_error -n "Unable to create temp/dummy directory."

result="$("${hdiutilbin}" create -srcdir empty_folder \
 -volname "${install_title}" \
 -fs HFS+ -fsargs "-c c=64,a=16,e=16" -format UDRW \
 -size "${dmg_size}m" "${tempdmg}"
)" || trigger_error -n "Error creating DMG image."

rmdir empty_folder > /dev/null 2>&1 || \
 trigger_error -n "Unable to remove temp/dummy directory."
echo "done."

echo -n "Mounting image: "
result="$(
 "${hdiutilbin}" attach -readwrite -noverify -noautoopen \
  -mountpoint "${mountpoint}" "${tempdmg}" 2>&1
)" || trigger_error -n "Error trying to mount volume:
- Image file: ${tempdmg}
- Mount point: ${mountpoint}
Command output:
${result}"
echo "done."

# Create the directory structure
echo -n "Creating directory structure: "
for destdir in \
 "${mountpoint}/.background" \
 "${mountpoint}/Bridge.framework" \
 "${mountpoint}/Bridge.framework/commands"
 do
 # just to be a little paranoid, we don't 'mkdir -p'.
 result="$(mkdir "${destdir}" 2>&1)" || \
  trigger_error -n "Error trying to create image directory: ${destdir}
Error message:
${result}"
done
echo "done."

# Copy over the files to the expected directories
echo -n "Deploying Bridge files to the image: "
result="$(cp -pr "${clipath}/"{bridge.exe,templates,tools} \
 "${mountpoint}/Bridge.framework/." 2>&1)" || \
  trigger_error -n "Unable to deploy files to image.
Error message:
${result}"
echo "done."

# Create bridge wrapper (to call bridge without the need of the 'mono' prefix)
echo -n "Setting up installer (wrapper, path): "
echo "/Library/Frameworks/Bridge.framework/commands" > "${mountpoint}/bridge-commands" || \
 trigger_error -n "Unable to create bridge-commands file on image."
cat << EOS > "${mountpoint}/Bridge.framework/commands/bridge" || trigger_error -n "Unabe to create bridge wrapper on image."
#!/bin/bash

scppath="\$(dirname "\${BASH_SOURCE[0]}")"

# In OSX we can only get relative path to the link.
physpath="\$(dirname "\$(readlink -n "\${BASH_SOURCE[0]}")")"
bridgepath="\${scppath}/\${physpath}/../bridge.exe"

mono "\${bridgepath}" "\${@}"

exit "\${?}"
EOS
chmod a+x "${mountpoint}/Bridge.framework/commands/bridge" || \
 trigger_error -n
cp "${clipath}/../../bridgedotnet-32x32.ico" "${mountpoint}/.VolumeIcon.icns" || \
 trigger_error -n "Unable to copy package icon over."
SetFile -c icnC "${mountpoint}/.VolumeIcon.icns" || \
 trigger_error -n "Unable to set up package icon."
ln -s /Library/Frameworks "${mountpoint}/Frameworks" || \
 trigger_error -n
ln -s /etc/paths.d "${mountpoint}/paths.d" || \
 trigger_error -n
cp "${scppath}/install-bg.png" "${mountpoint}/.background/." || \
 tirgger_error -n
echo "done."

# Set up installer look&feel (applescript)

window_bounds="${win_left}, ${win_top}, $(( 10#${win_left} + 10#${win_width} )), $(( 10#${win_top} + 10#${win_height}))"
window_outside_position="$(( 10#${win_left} + 10#${win_width} + 100 )), $(( 10#${win_top} + 10#${win_height} + 100 ))"
window_inner_bounds="${win_left}, ${win_top}, ${win_width}, ${win_height}"
volname="$(basename "${mountpoint}")"
osascript="$(cat << EOS
on run
 tell application "Finder"
  tell disk "${volname}"
   open
   tell container window
    set current view to icon view
    set toolbar visible to false
    set statusbar visible to false
    set the bounds to {${window_bounds}}
    set position of every item to {${window_outside_position}}
   end tell

   set opts to the icon view options of container window
   tell opts
    set icon size to 64
    set text size to 16
    set arrangement to not arranged
   end tell

   set background picture of opts to file ".background:install-bg.png"

   -- set position of item ".VolumeIcon.icns" to {50, 50}
   set position of item "Bridge.framework" to {135, 80}
   set position of item "bridge-commands" to {135, 235}
   set position of item "Frameworks" to {455, 80}
   set position of item "paths.d" to {455, 235}

   close
   open

   update without registering applications

   -- force saving of the size
   delay 1

   tell container window
    set statusbar visible to false
    set the bounds to {${window_inner_bounds}}
   end tell
   update without registering applications
  end tell

  delay 1

  tell disk "${volname}"
   tell container window
    set statusbar visible to false
    set the bounds to {${window_bounds}}
   end tell
   update without registering applications
  end tell

  -- give finder some time to write the .DS_Store file
  delay 3

  set waitTime to 0
  set ejectMe to false
  repeat while ejectMe is false
   delay 1
   set waitTime to waitTime + 1
   if (do shell script "test -f '${mountpoint}/.DS_Store'; echo \${?}") = "0" then set ejectMe to true
  end repeat

  log "waited " & waitTime & " seconds for .DS_Store to be created."
 end tell
end run
EOS
)"

# Usaful for debugging, dump the script to a file so the positioning can be
# experimented with
# echo "${osascript}" > osa.scp

echo -n "Setting up .DS_Store: "
result="$("${osascp}" -e "${osascript}" "${install_title}")" || \
 trigger_error -n "Apple Script interpreter returned with error.
Script output:
${result}"
echo "done."

# Close/unmount and remove temporary stuff.
echo -n "Unmounting image: "
result="$("${hdiutilbin}" detach "${mountpoint}" 2>&1)" || \
 trigger_error -n "Unable to unmount volume: ${mountpoint}
Error output:
${result}"
echo "done."

# Pack the image.
echo -n "Packing the image file: ${targetdmg}"
result="$("${hdiutilbin}" convert "${tempdmg}" -format UDZO -imagekey zlib-level=9 \
 -o "${targetdmg}")" || {
 echo -n ", " # do a "(...).dmg, failed."
 trigger_error -n "Error converting image. Command output:
${result}"
 }
echo ", done."

echo -n "Cleaning up from temp files: "
rm "${tempdmg}" || trigger_error -n "Unable to remove temporary raw image file."
echo "done."