@echo off

echo Starting build-installer.bat script.
:: This batch script will build windows installer if Bridge CLI was built in
:: Release mode.
:: Usage: build-installer.bat $(Configuration)

setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION

:: Cleanup environment variables
set nsisdir=

set exit_status=0
set Configuration=%~1
if "%Configuration%"=="Release" (
 for /f "tokens=2*" %%A in ('reg query "HKLM\SOFTWARE\NSIS" /v "" /reg:32 2^>nul ^| find "REG_SZ"') do (
  set nsisdir=%%B
 )
 if defined nsisdir (
  echo NSIS installed at !nsisdir!
  if exist "!nsisdir!\makensis.exe" (

   rem assuming the script will be called from post-build directory, that is
   rem CLI\Bridge\bin\Release
   "!nsisdir!\makensis.exe" "..\..\..\installer\windows\bridge-installer.nsi"
   if !ERRORLEVEL! neq 0 (
    set exit_status=!ERRORLEVEL!
    echo *** Error: Unable to build Windows Installer package. makensis.exe returned exit status !exit_status!.
   ) else (
    echo Moving Bridge CLI installer to %CD%
    move ..\..\..\installer\windows\bridge-cli.exe .
    if !ERRORLEVEL! neq 0 (
     set exit_status=!ERRORLEVEL!
     echo *** Error: Unable to move bridge installer package from '..\..\..\installer\windows' to '%CD%'.
    )
   )
  ) else (
   echo *** Error: NSIS is installed but makensis.exe is not available.
   set exit_status=1
  )
 ) else (
  echo *** Warning: NSIS is not installed. Skipping Bridge CLI installer build. Download latest NSIS version from http://nsis.sf.net/.
 )
) else (
 echo *** Warning: Will only build Bridge CLI Installer when built in Release mode.
)

echo Finished build-installer.bat script.
exit /b !exit_status!