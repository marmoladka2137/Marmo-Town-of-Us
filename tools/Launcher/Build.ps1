param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'bin'))
$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /codepage:65001 /warn:4 "/win32icon:$PSScriptRoot/Logo.ico" "/out:$OutputDirectory/Marmo Town of Us.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll "/resource:$PSScriptRoot/Banner.png,Marmo.Banner.png" "/resource:$PSScriptRoot/DiscordLogo.png,Marmo.DiscordLogo.png" (Join-Path $PSScriptRoot 'LauncherCore.cs') (Join-Path $PSScriptRoot 'LauncherApp.cs') (Join-Path $PSScriptRoot 'LauncherForm.cs') (Join-Path $PSScriptRoot 'ReleaseUpdate.cs') (Join-Path $PSScriptRoot '../SteamInstaller/InstallerCore.cs')
if($LASTEXITCODE -ne 0){throw 'Launcher build failed.'}
Write-Output "Built $OutputDirectory/Marmo Town of Us.exe"
