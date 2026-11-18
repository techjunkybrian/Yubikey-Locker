#define AppName "YubiKey Locker"
#define AppVersion "1.0.0"
#define AppPublisher "YubiKey Locker contributors"
#define AppExeName "YubikeyLocker.exe"

[Setup]
AppId={{D7FA608E-30D9-4DB5-9F3D-485DC9F98C27}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\YubiKey Locker
DefaultGroupName={#AppName}
OutputDir=..\artifacts
OutputBaseFilename=YubiKeyLocker-Setup-{#AppVersion}-win-x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#AppExeName}

[Tasks]
Name: "startup"; Description: "Start YubiKey Locker when I sign in to Windows"; GroupDescription: "Additional options:"; Flags: unchecked

[Files]
Source: "..\publish\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\YubiKey Locker"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall YubiKey Locker"; Filename: "{uninstallexe}"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "YubiKeyLocker"; ValueData: """{app}\{#AppExeName}"""; Tasks: startup; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch YubiKey Locker"; Flags: nowait postinstall skipifsilent
