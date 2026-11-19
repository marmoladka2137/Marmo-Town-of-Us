param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'bin'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $compiler /nologo /target:exe /platform:x64 /codepage:65001 "/out:$OutputDirectory/SetupTests.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll (Join-Path $PSScriptRoot 'SetupTests.cs') (Join-Path $PSScriptRoot 'LauncherCore.cs') (Join-Path $PSScriptRoot 'LauncherVersion.cs') (Join-Path $PSScriptRoot 'LauncherForm.cs') (Join-Path $PSScriptRoot 'FolderPicker.cs') (Join-Path $PSScriptRoot 'ReleaseUpdate.cs') (Join-Path $PSScriptRoot '../SteamInstaller/InstallerCore.cs')
if($LASTEXITCODE -ne 0){throw 'Setup tests did not compile.'}
& (Join-Path $OutputDirectory 'SetupTests.exe')
if($LASTEXITCODE -ne 0){throw 'Setup tests failed.'}
