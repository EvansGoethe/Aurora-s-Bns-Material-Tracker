#define AppName "洛洛劍靈材料追蹤器"
#define AppVersion "1.0.0"
#define AppPublisher "EvansGoethe"
#define AppURL "https://github.com/EvansGoethe/Aurora-s-Bns-Material-Tracker"
#define AppExeName "Aurora's BnS Material Tracker.exe"
#define SourceDir "D:\代碼農\BnsMaterialTracker\publish"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
OutputDir={#SourceDir}\..\installer_output
OutputBaseFilename=Setup_Aurora_BnS_Material_Tracker_v{#AppVersion}
SetupIconFile={#SourceDir}\..\ABnS_.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
; .NET 6 Desktop Runtime 最低需求
MinVersion=10.0

[Languages]
Name: "tradchinese";   MessagesFile: "ChineseTraditional.isl"
Name: "simpchinese";   MessagesFile: "ChineseSimplified.isl"
Name: "english";       MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; 主程式
Source: "{#SourceDir}\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; Data 資料夾（JSON 遊戲資料）
Source: "{#SourceDir}\Data\*"; DestDir: "{app}\Data"; Flags: ignoreversion recursesubdirs createallsubdirs; AfterInstall: MarkDataFilesAsUserModifiable

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\解除安裝 {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
// 檢查 .NET 6 Desktop Runtime 是否已安裝
function IsDotNet6Installed(): Boolean;
var
  RegKey: String;
begin
  RegKey := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
  Result := RegKeyExists(HKLM, RegKey) or RegKeyExists(HKCU, RegKey);
  // 備用：檢查常見安裝路徑
  if not Result then
    Result := DirExists(ExpandConstant('{pf}\dotnet\shared\Microsoft.WindowsDesktop.App'));
end;

procedure InitializeWizard();
var
  DotNetMsg: String;
begin
  if not IsDotNet6Installed() then
  begin
    if ActiveLanguage() = 'english' then
      DotNetMsg :=
        '.NET 6.0 Desktop Runtime was not detected on your computer.' + #13#10 + #13#10 +
        'After installation, please download and install it from:' + #13#10 +
        'https://dotnet.microsoft.com/en-us/download/dotnet/6.0' + #13#10 + #13#10 +
        'Select ".NET Desktop Runtime 6.x.x" (x64).'
    else
      DotNetMsg :=
        '偵測到您的電腦尚未安裝 .NET 6.0 Desktop Runtime。' + #13#10 + #13#10 +
        '安裝完成後，請至以下網址下載並安裝：' + #13#10 +
        'https://dotnet.microsoft.com/en-us/download/dotnet/6.0' + #13#10 + #13#10 +
        '請選擇「.NET Desktop Runtime 6.x.x」的 x64 版本。';
    MsgBox(DotNetMsg, mbInformation, MB_OK);
  end;
end;

procedure MarkDataFilesAsUserModifiable();
begin
  // Data 資料夾不在解除安裝時刪除（保留使用者自訂資料）
end;

[UninstallRun]
; 解除安裝時不刪除 Data 資料夾（保留使用者設定的遊戲資料）

[UninstallDelete]
; 只刪除程式本體，Data 資料夾由使用者自行決定是否保留
Type: files; Name: "{app}\{#AppExeName}"
