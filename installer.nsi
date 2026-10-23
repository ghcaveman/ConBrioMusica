!define APPNAME "ConBrioMusica"
!define VERSION "1.0.0"

Name "${APPNAME}"
OutFile "ConBrioMusica-Setup.exe"

; Target the native 64-bit Program Files directory
InstallDir "$PROGRAMFILES64\${APPNAME}"
RequestExecutionLevel admin

Page directory
Page instfiles

Section "Install"
  SetOutPath "$INSTDIR"
  
  ; Pulls the compiled binary output from the MSBuild step
  File "bin\Release\ConBrioMusica.exe"
  File "bin\Release\ConBrioMusica.exe.config"
  
  WriteUninstaller "$INSTDIR\uninstall.exe"
  CreateShortcut "$SMPROGRAMS\${APPNAME}.lnk" "$INSTDIR\ConBrioMusica.exe"
SectionEnd

Section "Uninstall"
  Delete "$INSTDIR\ConBrioMusica.exe"
  Delete "$INSTDIR\ConBrioMusica.exe.config"
  Delete "$INSTDIR\uninstall.exe"
  Delete "$SMPROGRAMS\${APPNAME}.lnk"
  RMDir "$INSTDIR"
SectionEnd
