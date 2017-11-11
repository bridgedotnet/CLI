@echo off

:: This batch script will build windows installer if Bridge CLI was built in
:: Release mode.
:: Usage: build-installer.bat $(Configuration)

setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION

:: Cleanup environment variables
set nsisdir=
set Configuration=

set Configuration=%~1
if "%Configuration%"=="Release" (
 for /f "tokens=2*" %%A in ('reg query "HKLM\SOFTWARE\NSIS" /v "" /reg:32 2^>nul ^| find "REG_SZ"') do (
  set nsisdir=%%B
 )
 echo fui
 if defined nsisdir (
  echo NSIS installed at !nsisdir!
  if exist "!nsisdir!\makensis.exe" (

   :: assuming the script will be called from post-build directory, that is
   :: CLI\Bridge\bin\Release
   "!nsisdir!\makensis.exe" "..\..\..\installer\windows\bridge-installer.nsi"
   if !ERRORLEVEL! neq 0 (
    echo *** Error: makensis.exe returned error status !ERRORLEVEL!.
   ) else (
    echo Moving Bridge CLI installer to %CD%
    move ..\..\..\installer\windows\bridge-*.exe .
   )
  ) else (
   echo NSIS is installed but makensis.exe is not available.
   exit /b 1
  )
 ) else (
  echo NSIS is not installed. Skipping Bridge CLI installer build.
 )
) else (
 echo Will only build Bridge Installer when built in Release mode.
)