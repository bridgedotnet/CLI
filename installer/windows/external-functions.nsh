;--------------------------------
;Add/Remove path to %PATH% environment variable
;Extracted from smartmontools installer at:
;https://github.com/jcsp/smartmontools/blob/master/os_win32/installer.nsi

; Registry Entry for environment (NT4,2000,XP)
; All users:
;!define Environ 'HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"'
; Current user only:
;!define Environ 'HKCU "Environment"'

!ifndef Environ
  !define RegistryRoot "HKCU"
  !define RegistryPath "Environment"
  !define Environ '${RegistryRoot} "${RegistryPath}"'
!endif

; AddToPath - Appends dir to PATH
;   (does not work on Win9x/ME)
;
; Usage:
;   Push "dir"
;   Call AddToPath

Function AddToPath
  Exch $0
  Push $1
  Push $2
  Push $3
  Push $4

  ReadRegStr $1 ${Environ} "Path"
  ReadRegStr $4 ${Environ} "Bridge_backup_path"
  IfErrors 0 NoOverwrite
    DetailPrint "Backing up original PATH value in key '${RegistryRoot}\${RegistryPath}\Bridge_backup_path'"
    WriteRegExpandStr ${Environ} "Bridge_backup_path" $1
    Goto AfterBkCheck
  NoOverwrite:
    DetailPrint "Key '${RegistryRoot}\${RegistryPath}\Bridge_backup_path' already exists. Not overwriting."
  AfterBkCheck:
  Push "$1;"
  Push "$0;"
  Call StrStr
  Pop $2
  StrCmp $2 "" 0 done
  Push "$1;"
  Push "$0\;"
  Call StrStr
  Pop $2
  StrCmp $2 "" 0 done
  DetailPrint "Adding to PATH: $0"
  StrCpy $2 $1 1 -1
  StrCmp $2 ";" 0 +2
    StrCpy $1 $1 -1 ; remove trailing ';'
  StrCmp $1 "" +2   ; no leading ';'
    StrCpy $0 "$1;$0"
  WriteRegExpandStr ${Environ} "Path" $0
  SendMessage ${HWND_BROADCAST} ${WM_WININICHANGE} 0 "STR:Environment" /TIMEOUT=5000

done:
  Pop $4
  Pop $3
  Pop $2
  Pop $1
  Pop $0
FunctionEnd

; RemoveFromPath - Removes dir from PATH
;
; Usage:
;   Push "dir"
;   Call RemoveFromPath

Function un.RemoveFromPath
  Exch $0
  Push $1
  Push $2
  Push $3
  Push $4
  Push $5
  Push $6

  ReadRegStr $1 ${Environ} "Path"
  StrCpy $5 $1 1 -1
  StrCmp $5 ";" +2
    StrCpy $1 "$1;" ; ensure trailing ';'
  Push $1
  Push "$0;"
  Call un.StrStr
  Pop $2 ; pos of our dir
  StrCmp $2 "" done

  DetailPrint "Remove from PATH: $0"
  StrLen $3 "$0;"
  StrLen $4 $2
  StrCpy $5 $1 -$4 ; $5 is now the part before the path to remove
  StrCpy $6 $2 "" $3 ; $6 is now the part after the path to remove
  StrCpy $3 "$5$6"
  StrCpy $5 $3 1 -1
  StrCmp $5 ";" 0 +2
    StrCpy $3 $3 -1 ; remove trailing ';'
  WriteRegExpandStr ${Environ} "Path" $3
  SendMessage ${HWND_BROADCAST} ${WM_WININICHANGE} 0 "STR:Environment" /TIMEOUT=5000

done:
  Pop $6
  Pop $5
  Pop $4
  Pop $3
  Pop $2
  Pop $1
  Pop $0
FunctionEnd

; StrStr - find substring in a string
;
; Usage:
;   Push "this is some string"
;   Push "some"
;   Call StrStr
;   Pop $0 ; "some string"

!macro StrStr un
Function ${un}StrStr
  Exch $R1 ; $R1=substring, stack=[old$R1,string,...]
  Exch     ;                stack=[string,old$R1,...]
  Exch $R2 ; $R2=string,    stack=[old$R2,old$R1,...]
  Push $R3
  Push $R4
  Push $R5
  StrLen $R3 $R1
  StrCpy $R4 0
  ; $R1=substring, $R2=string, $R3=strlen(substring)
  ; $R4=count, $R5=tmp
  loop:
    StrCpy $R5 $R2 $R3 $R4
    StrCmp $R5 $R1 done
    StrCmp $R5 "" done
    IntOp $R4 $R4 + 1
    Goto loop
done:
  StrCpy $R1 $R2 "" $R4
  Pop $R5
  Pop $R4
  Pop $R3
  Pop $R2
  Exch $R1 ; $R1=old$R1, stack=[result,...]
FunctionEnd
!macroend
!insertmacro StrStr ""
!insertmacro StrStr "un."
;end smartmontools extracted block
;--------------------------------

;--------------------------------
;Delete dir if it is empty, extracted from:
;http://nsis.sourceforge.net/Delete_dir_only_if_empty
; Usage:
;   StrCpy $0 "folder"
;   Call un.DeleteDirIfEmpty
Function un.DeleteDirIfEmpty
  FindFirst $R0 $R1 "$0\*.*"
  strcmp $R1 "." 0 NoDelete
  FindNext $R0 $R1
  strcmp $R1 ".." 0 NoDelete
  ClearErrors
  FindNext $R0 $R1
  IfErrors 0 NoDelete
    FindClose $R0
    Sleep 1000
    DetailPrint "Removed empty directory: $0"
    RMDir "$0"
  NoDelete:
  FindClose $R0
FunctionEnd

Function un.AskRemovePathBackup
  ReadRegStr $R0 ${Environ} "Bridge_backup_path"
  IfErrors NoBkKey
    DetailPrint "Found backup registry key for PATH environment variable."
    MessageBox MB_YESNO|MB_ICONQUESTION "Found backup Path environment variable registry key under:$\n$\n${RegistryRoot}\${RegistryPath}\Bridge_backup_path$\n$\nRemove it?" IDNO DontRemoveBkKey
      DeleteRegValue ${Environ} "Bridge_backup_path"
      SendMessage ${HWND_BROADCAST} ${WM_WININICHANGE} 0 "STR:Environment" /TIMEOUT=5000
      DetailPrint "Removed backup PATH key from the registry."
      Goto done
  NoBkKey:
    DetailPrint "No PATH backup key from Bridge CLI in the registry."
    Goto done
  DontRemoveBkKey:
    DetailPrint "Keeping backup PATH key from Bridge CLI in the registry."
  done:
FunctionEnd