#define AppName "MyShop POS"
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
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#InstallerRoot}\staging\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#InstallerRoot}\staging\database\*"; DestDir: "{app}\installer\database"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#InstallerRoot}\install-bootstrap.ps1"; DestDir: "{app}\installer"; Flags: ignoreversion
Source: "{#InstallerRoot}\prerequisites\windowsdesktop-runtime-8-win-x64.exe"; DestDir: "{app}\installer\prerequisites"; Flags: ignoreversion
Source: "{#InstallerRoot}\prerequisites\windowsappruntimeinstall-x64.exe"; DestDir: "{app}\installer\prerequisites"; Flags: ignoreversion
Source: "{#InstallerRoot}\prerequisites\postgresql-16-windows-x64.exe"; DestDir: "{app}\installer\prerequisites"; Flags: ignoreversion

[Run]
Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\installer\install-bootstrap.ps1"" -AppDir ""{app}"" -PrereqDir ""{app}\installer\prerequisites"" -DatabaseTool ""{app}\installer\database\MyShop.DatabaseBootstrapper.exe"""; StatusMsg: "Installing prerequisites and preparing local database..."; Flags: waituntilterminated runhidden

[Icons]
Name: "{group}\MyShop POS"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\Assets\StoreLogo.png"
Name: "{autodesktop}\MyShop POS"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\Assets\StoreLogo.png"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: checkedonce

[UninstallDelete]
Type: filesandordirs; Name: "{app}\installer"
