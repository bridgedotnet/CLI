@echo off

set TargetDir=%~1

rem disable the cleaning script (if it breaks due to changes in post build)
rem exit /b 0

cd "%TargetDir%"
if %ERRORLEVEL% neq 0 (
 echo *** Error: unable to change to directory: %TargetDir%
 exit /b 1
)

echo Removing %CD%\templates directory . . .
if exist templates\. (
 rd /s /q templates
 if %ERRORLEVEL% neq 0 (
  echo *** Error: unable to recursively delete %CD%\templates directory.
  exit /b 1
 )

 if exist templates\. (
  echo *** Error: unable to remove %CD%\templates directory.
  exit /b 1
 )
) else (
 echo *** Warning: %CD%\templates directory not found for removal.
)

echo Removing %CD%\lib directory . . .
if exist lib\. (
 if exist lib\pdb\. (
  if exist lib\pdb\Bridge.pdb (
   del lib\pdb\Bridge.pdb
   if %ERRORLEVEL% neq 0 (
    echo *** Error: unable to delete %CD%\lib\pdb\Bridge.pdb.
	exit /b 1
   )
  ) else (
   echo *** Warning: %CD%\lib\pdb\Bridge.pdb not found for removal.
  )

  rem directory should be empty, so will try just a simple 'rd'
  rem 'rd' does not return false when you try to remove a directory that's
  rem not empty, so just check if the directory still exists after erasing
  rd lib\pdb
  if %ERRORLEVEL% neq 0 (
   echo *** Error: unable to remove %CD%\lib\pdb directory.
   exit /b 1
  )

  if exist lib\pdb\. (
   echo *** Error: unable to remove %CD%\lib\pdb directory.
   exit /b 1
  )

 ) else (
  echo *** Warning: %CD%\lib\pdb directory not found for removal.
 )

 del /q lib\Bridge.*
 if %ERRORLEVEL% neq 0 (
  echo *** Error: unable to delete %CD%\lib\Bridge.*
  exit /b 1
 )

 rd lib
 if %ERRORLEVEL% neq 0 (
  echo *** Error: unable to remove %CD%\lib directory.
  exit /b 1
 )

 if exist lib\. (
  echo *** Error: unable to remove %CD%\lib directory.
  exit /b 1
 )
) else (
 echo *** Warning: %CD%\lib directory not found for removal.
)

echo Removing %CD%\tools directory . . .
if exist tools\. (
 del /q tools\*.*
 if %ERRORLEVEL% neq 0 (
  echo *** Error: unable to delete %CD%\tools\*.*
  exit /b 1
 )

 rd tools
 if %ERRORLEVEL% neq 0 (
  echo *** Error: unable to remove %CD%\tools directory.
  exit /b 1
 )

 if exist tools\. (
  echo *** Error: unable to remove %CD%\tools directory.
  exit /b 1
 )
) else (
 echo *** Warning: %CD%\tools directory not found for removal.
)

echo Removing installers if present . . .
if exist bridge-cli.exe (
 del bridge-cli.exe
 if %ERRORLEVEL% neq 0 (
  echo *** Error: unable to delete %CD%\bridge-cli.exe
  exit /b 1
 )
) else (
 echo *** Warning: %CD%\bridge-cli.exe not found for removal.
)

if exist bridge-cli-local.exe (
 del bridge-cli-local.exe
 if %ERRORLEVEL% neq 0 (
  echo *** Error: unable to delete %CD%\bridge-cli-local.exe
  exit /b 1
 )
) else (
 echo *** Warning: %CD%\bridge-cli-local.exe not found for removal.
)
