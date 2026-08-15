; EmailLock installer — Inno Setup 6.
; Built by build.ps1, which publishes the app self-contained first so the machine
; needs no .NET install. Per-user by default: no UAC prompt, no admin rights.

#define AppName    "EmailLock"
#define AppVersion "2.0.0"
#define AppExe     "EmailLock.exe"
#define AppUrl     "https://github.com/byensitmagnus/emaillock"

[Setup]
AppId={{6F2C1E7A-9B44-4D3E-A1C8-5E0D7F3B2A91}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppName}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}/issues
VersionInfoVersion={#AppVersion}

DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableDirPage=auto
DisableWelcomePage=no

; Per-user only. No UAC prompt, and no "for me or for everyone?" question to answer --
; the app is a personal habit tool, not something a machine shares.
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

OutputDir=..\dist
OutputBaseFilename={#AppName}-{#AppVersion}-setup
SetupIconFile=..\src\EmailLock\ui\app.ico
UninstallDisplayIcon={app}\{#AppExe}
WizardStyle=modern
Compression=lzma2/max
SolidCompression=yes
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "da"; MessagesFile: "compiler:Languages\Danish.isl"

[CustomMessages]
en.StartupTask=Start EmailLock when Windows starts (recommended — without it the lock is gone after a restart)
da.StartupTask=Start EmailLock sammen med Windows (anbefalet — uden det er låsen væk efter en genstart)
en.LaunchApp=Open EmailLock
da.LaunchApp=Åbn EmailLock
en.NeedWebView2=EmailLock needs Microsoft Edge WebView2, and it could not be installed automatically. Install it from https://developer.microsoft.com/microsoft-edge/webview2/ and run this installer again.
da.NeedWebView2=EmailLock kræver Microsoft Edge WebView2, og den kunne ikke installeres automatisk. Hent den på https://developer.microsoft.com/microsoft-edge/webview2/ og kør installeren igen.

[Tasks]
Name: startup; Description: "{cm:StartupTask}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\src\EmailLock\bin\Release\net8.0-windows\win-x64\publish\*"; \
  DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; \
  ValueType: string; ValueName: "{#AppName}"; ValueData: """{app}\{#AppExe}"""; \
  Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchApp}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/f /im {#AppExe}"; \
  Flags: runhidden; RunOnceId: "StopEmailLock"

[Code]

// The app deliberately resists being closed, so stop it before overwriting its files.
procedure StopRunningApp();
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/f /im {#AppExe}', '',
       SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

// Evergreen WebView2 ships with Windows 11 but not with every Windows 10.
function WebView2Missing(): Boolean;
var
  Version: String;
begin
  Result := not (
    RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version) or
    RegQueryStringValue(HKLM, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version) or
    RegQueryStringValue(HKCU, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version));
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  Result := '';
  StopRunningApp();

  if WebView2Missing() then
  begin
    try
      DownloadTemporaryFile('https://go.microsoft.com/fwlink/p/?LinkId=2124703',
                            'MicrosoftEdgeWebview2Setup.exe', '', nil);
      Exec(ExpandConstant('{tmp}\MicrosoftEdgeWebview2Setup.exe'), '/silent /install', '',
           SW_HIDE, ewWaitUntilTerminated, ResultCode);
    except
      Result := ExpandConstant('{cm:NeedWebView2}');
    end;
  end;
end;
