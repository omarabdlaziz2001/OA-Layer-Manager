; Inno Setup script for OA Layer Manager
; Produces a single per-user installer EXE (no admin rights required) that installs
; the plugin bundle into %APPDATA%\Autodesk\ApplicationPlugins so AutoCAD auto-loads it.
;
; Build:  see installer\build-installer.ps1  (or open this file in Inno Setup and press F9)

#define AppName    "OA Layer Manager"
#define AppVersion "1.1.0"
#define AppPublisher "Omar Abdelaziz (OA)"
#define BundleName "OALayerManager.bundle"

[Setup]
; AppId ties upgrades/uninstall together - keep it stable across versions.
AppId={{CBE7C224-850C-4A3C-93E4-D4C811CA44D5}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/omarabdlaziz2001/OA-Layer-Manager
; Install straight into the AutoCAD per-user plugins folder.
DefaultDirName={userappdata}\Autodesk\ApplicationPlugins\{#BundleName}
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=Output
OutputBaseFilename=OALayerManager-Setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName={#AppName}
DisableWelcomePage=no

[Files]
; The built bundle (run a Release build first so this folder is current).
Source: "..\{#BundleName}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Code]
function AutoCadInstalled(): Boolean;
var
  Y: Integer;
begin
  Result := False;
  // Supported: AutoCAD 2021-2026.
  for Y := 2021 to 2026 do
    if DirExists('C:\Program Files\Autodesk\AutoCAD ' + IntToStr(Y)) then
    begin
      Result := True;
      Exit;
    end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if not AutoCadInstalled() then
  begin
    if MsgBox('AutoCAD 2021-2026 was not detected.' #13#10 #13#10 +
              'This plugin supports AutoCAD 2021 and newer.' #13#10 #13#10 +
              'Install anyway?', mbConfirmation, MB_YESNO) = IDNO then
      Result := False;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssDone then
    MsgBox('Installation complete.' #13#10 #13#10 +
           'Start AutoCAD and open the "OA Tools" ribbon tab, or type OA_BATCHLAYERS.' #13#10 +
           'If AutoCAD shows a security prompt about an unapproved app, choose "Always Load".',
           mbInformation, MB_OK);
end;
