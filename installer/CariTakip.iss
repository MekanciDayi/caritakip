[Setup]
AppId={{8A43D4EF-3554-4A64-9BE7-83D56B9B4DD0}}
AppName=Cari Takip
AppVersion=1.0.0
AppPublisher=Cari Takip
DefaultDirName={localappdata}\Programs\CariTakip
DefaultGroupName=Cari Takip
OutputBaseFilename=CariTakipSetup
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest

#if FileExists("..\\publish\\app.ico")
SetupIconFile=..\\publish\\app.ico
#endif

[Tasks]
Name: "desktopicon"; Description: "Masaüstü kısayolu oluştur"; GroupDescription: "Kısayollar";

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
#if FileExists("..\publish\app.ico")
Name: "{group}\Cari Takip"; Filename: "{app}\CariTakip.exe"; IconFilename: "{app}\app.ico"
Name: "{autodesktop}\Cari Takip"; Filename: "{app}\CariTakip.exe"; Tasks: desktopicon; IconFilename: "{app}\app.ico"
#else
Name: "{group}\Cari Takip"; Filename: "{app}\CariTakip.exe"
Name: "{autodesktop}\Cari Takip"; Filename: "{app}\CariTakip.exe"; Tasks: desktopicon
#endif

[Run]
Filename: "{app}\CariTakip.exe"; Description: "Cari Takip'i çalıştır"; Flags: nowait postinstall skipifsilent
