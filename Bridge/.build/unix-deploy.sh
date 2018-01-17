#!/bin/bash
# This script performs the post-build file deploying tasks on unix os
# environments.

osx=false
test "$(uname)" == "Darwin" && osx=true

${osx} && cpargs=pR || cpargs=dpvr

projdir="${1}"
tgtdir="${2}"
slndir="${3}"

function trigger_error() {
 if [ ${#@} -gt 0 ]; then
  >&2 echo "*** Error: ${@}"
 else
  >&2 echo "*** Error. Aborting script execution."
 fi
 exit 1
}

function copy_over() {
 # Dangerous block. Disable until we make something safe
 #if [ -d "${2}" ]; then
 # rm -rf "${2}"
 #fi
 cp -${cpargs} "${1}" "${2}" || trigger_error
}

copy_over "${projdir}.build/templates" "${tgtdir}."
copy_over "${slndir}packages/Bridge.Min.16.7.0/tools" "${tgtdir}."
copy_over "${slndir}packages/Bridge.Core.16.7.0/lib/net40" "${tgtdir}lib"
copy_over "${tgtdir}bridge.pdb" "${tgtdir}tools/."

if ${osx}; then
 echo "Should make OS X .pkg."
else
 echo "Should make some sort of linux installer."
fi
