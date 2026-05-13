#define AppName "MyShop Gaming Accessories POS"
#define AppPublisher "MyShop Gaming Accessories POS"
#define AppExeName "ProjectTest.exe"
#define InstallerRoot SourcePath

[Setup]
AppId={{77C67730-4830-4E95-9A0F-5A6F5D2F1B65}
AppName={#AppName}
AppVersion=1.0.0
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\MyShop POS
DefaultGroupName=MyShop POS
DisableProgramGroupPage=yes
OutputDir={#InstallerRoot}\output
OutputBaseFilename=setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}
CloseApplications=yes
RestartApplications=no
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#InstallerRoot}\staging\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#InstallerRoot}\staging\database\*"; DestDir: "{app}\installer\database"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#InstallerRoot}\install-bootstrap.ps1"; DestDir: "{app}\installer"; Flags: ignoreversion
Source: "{#InstallerRoot}\..\scripts\restore-demo-database.ps1"; DestDir: "{app}\scripts"; Flags: ignoreversion
#ifexist InstallerRoot + "\database\myshop_demo.dump"
Source: "{#InstallerRoot}\database\myshop_demo.dump"; DestDir: "{app}\installer\database"; Flags: ignoreversion
#endif
Source: "{#InstallerRoot}\prerequisites\windowsdesktop-runtime-8-win-x64.exe"; DestDir: "{app}\installer\prerequisites"; Flags: ignoreversion
Source: "{#InstallerRoot}\prerequisites\windowsappruntimeinstall-x64.exe"; DestDir: "{app}\installer\prerequisites"; Flags: ignoreversion
Source: "{#InstallerRoot}\prerequisites\postgresql-18-windows-x64.exe"; DestDir: "{app}\installer\prerequisites"; Flags: ignoreversion

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  Parameters: String;
begin
  if CurStep = ssPostInstall then
  begin
    Parameters :=
      '-NoProfile -ExecutionPolicy Bypass -File "' + ExpandConstant('{app}\installer\install-bootstrap.ps1') + '"' +
      ' -AppDir "' + ExpandConstant('{app}') + '"' +
      ' -PrereqDir "' + ExpandConstant('{app}\installer\prerequisites') + '"' +
      ' -DatabaseTool "' + ExpandConstant('{app}\installer\database\MyShop.DatabaseBootstrapper.exe') + '"' +
      ' -RestoreScript "' + ExpandConstant('{app}\scripts\restore-demo-database.ps1') + '"' +
      ' -DemoDump "' + ExpandConstant('{app}\installer\database\myshop_demo.dump') + '"';

    WizardForm.StatusLabel.Caption := 'Installing prerequisites and preparing local database...';
    if not Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), Parameters, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      RaiseException('Could not start MyShop installer bootstrap.');

    if ResultCode <> 0 then
      RaiseException('MyShop installer bootstrap failed. See C:\ProgramData\MyShop POS\Logs\setup-log.txt.');
  end;
end;

[Icons]
Name: "{group}\MyShop Gaming Accessories POS"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\MyShop.ico"
Name: "{group}\Uninstall MyShop Gaming Accessories POS"; Filename: "{uninstallexe}"; IconFilename: "{uninstallexe}"
Name: "{autodesktop}\MyShop Gaming Accessories POS"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\MyShop.ico"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: checkedonce

[UninstallDelete]
Type: filesandordirs; Name: "{app}\installer"
Type: files; Name: "{app}\myshop.database.json"
