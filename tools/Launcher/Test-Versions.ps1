param([string]$OutputDirectory)
$ErrorActionPreference='Stop'
$launcherSource=$PSScriptRoot
New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
$versionTestFiles=@('VersionTests.cs','LauncherCore.cs','LauncherForm.cs','LauncherVersion.cs','VersionLibrary.cs','FolderPicker.cs','ReleaseUpdate.cs') | ForEach-Object {Join-Path $launcherSource $_}
$versionTestFiles+=Join-Path (Split-Path $launcherSource -Parent) 'SteamInstaller/InstallerCore.cs'
& "$env:WINDIR/Microsoft.NET/Framework64/v4.0.30319/csc.exe" /nologo /target:exe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll "/resource:$launcherSource\Banner.png,Marmo.Banner.png" "/resource:$launcherSource\DiscordLogo.png,Marmo.DiscordLogo.png" "/out:$OutputDirectory\VersionTests.exe" $versionTestFiles
if($LASTEXITCODE -ne 0){throw 'Version test compilation failed'}
& (Join-Path $OutputDirectory 'VersionTests.exe') (Join-Path $OutputDirectory 'isolated') (Join-Path $OutputDirectory 'releases.json')
if($LASTEXITCODE -ne 0){throw 'Version tests failed'}
