!define APPNAME "ConBrioMusica"
!define VERSION "1.0.0"

Name "${APPNAME}"
OutFile "ConBrioMusica-Setup.exe"
InstallDir "$PROGRAMFILES\${APPNAME}"
RequestExecutionLevel admin

Page directory
Page instfiles

Section "Install"
  SetOutPath "$INSTDIR"
  
  ; Pulls the compiled binary output from the MSBuild step
  File "bin\Release\ConBrioMusica.exe"
  File "bin\Release\ConBrioMusica.exe.config"
  
  ; Include any extra dependency DLLs built into your release directory
  ; File "bin\Release\*.dll"

  ; Optional: Bundle required items if they are present in your workspace
  ; SetOutPath "$INSTDIR\dependencies"
  ; File "bin\Release\chromedriver.exe"

  WriteUninstaller "$INSTDIR\uninstall.exe"
  
  ; Create a Start Menu shortcut
  CreateShortcut "$SMPROGRAMS\${APPNAME}.lnk" "$INSTDIR\ConBrioMusica.exe"
SectionEnd

Section "Uninstall"
  Delete "$INSTDIR\ConBrioMusica.exe"
  Delete "$INSTDIR\ConBrioMusica.exe.config"
  Delete "$INSTDIR\uninstall.exe"
  Delete "$SMPROGRAMS\${APPNAME}.lnk"
  RMDir "$INSTDIR"
SectionEnd
