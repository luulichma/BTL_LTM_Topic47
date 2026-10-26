; Bộ cài demo T47 (Inno Setup 6).
; Build: chạy DemoSetup.BuildWindows trong Unity trước, rồi
;   "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" demo\installer\T47Demo.iss
; Kết quả: demo\installer\Output\T47Demo_Setup_<phiên bản>.exe

#define AppName "T47 Demo"
#define AppVersion "0.1.0"
#define AppExe "T47Demo.exe"

[Setup]
AppId={{7E3A4C2B-9F41-4D7A-8B1E-47D3C0A1F547}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Nhóm T47 · Lập trình mạng · PTIT
AppComments=Demo sơ bộ: dự đoán phía client, đối chiếu với server, nội suy thực thể
DefaultDirName={autopf}\T47Demo
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
; Cài cho riêng người dùng hiện tại, không cần quyền admin (vẫn cho chọn cài cho mọi người).
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=Output
OutputBaseFilename=T47Demo_Setup_{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
SetupIconFile=compiler:SetupClassicIcon.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}

[Languages]
Name: "vietnamese"; MessagesFile: "Vietnamese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\T47Demo\Build\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "HuongDan.txt"; DestDir: "{app}"; Flags: ignoreversion isreadme

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\{#AppName} (tập một mình: A tự chạy)"; Filename: "{app}\{#AppExe}"; Parameters: "-autoplay"
Name: "{group}\Hướng dẫn sử dụng"; Filename: "{app}\HuongDan.txt"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
