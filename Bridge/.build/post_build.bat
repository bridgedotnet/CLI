@echo off

rem This script is responsible to deploy needed files by bridge to the build
rem target's directory

set ProjectDir=%~1
set TargetDir=%~2
set SolutionDir=%~3
set Configuration=%~4

XCOPY "%ProjectDir%.build\templates" "%TargetDir%templates\" /E /Y
XCOPY "%SolutionDir%packages\Bridge.Min.16.5.0\tools" "%TargetDir%tools\" /E /Y
XCOPY "%SolutionDir%packages\Bridge.Core.16.5.0\lib\net40" "%TargetDir%lib\" /E /Y
move /-y "%TargetDir%bridge.pdb" "%TargetDir%tools\"

"%SolutionDir%installer\windows\build-installer.bat" "%Configuration%"