@echo off

rem This script is responsible to deploy needed files by bridge to the build
rem target's directory

set ProjectDir=%~1
set TargetDir=%~2
set SolutionDir=%~3
set Configuration=%~4

set bridgeminver=
set bridgecorever=
for /f "tokens=3,5 delims== " %%a in ('type %ProjectDir%..\Bridge.Compiler\packages.config') do (
 if [%%a] == ["Bridge.Min"] (
  if not [%%b] == [] (
   set bridgeminver="%%~b"
  )
 )
 if [%%a] == ["Bridge.Core"] (
  if not [%%b] == [] (
   set bridgecorever="%%~b"
  )
 )
)

if [%bridgeminver%] == [] (
 echo Unable to query Bridge.Min package version.
 exit /b 1
)
if [%bridgecorever%] == [] (
 echo Unable to query Bridge.Core package version.
 exit /b 1
)

XCOPY "%ProjectDir%.build\templates" "%TargetDir%templates\" /E /Y
XCOPY "%SolutionDir%packages\Bridge.Min.%bridgeminver%\tools" "%TargetDir%tools\" /E /Y
XCOPY "%SolutionDir%packages\Bridge.Core.%bridgecorever%\lib\net40" "%TargetDir%lib\" /E /Y
move /-y "%TargetDir%bridge.pdb" "%TargetDir%tools\"

"%SolutionDir%installer\windows\build-installer.bat" "%Configuration%"
