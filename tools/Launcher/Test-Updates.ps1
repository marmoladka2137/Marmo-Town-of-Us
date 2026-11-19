param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'bin'))
$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
& $compiler /nologo /target:exe /codepage:65001 "/out:$OutputDirectory/UpdateTests.exe" /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll (Join-Path $PSScriptRoot 'ReleaseUpdate.cs') (Join-Path $PSScriptRoot 'LauncherCore.cs') (Join-Path $PSScriptRoot 'UpdateTests.cs') (Join-Path $PSScriptRoot '../SteamInstaller/InstallerCore.cs')
if($LASTEXITCODE -ne 0){throw 'Updater tests did not compile.'}
& (Join-Path $OutputDirectory 'UpdateTests.exe')
if($LASTEXITCODE -ne 0){throw 'Updater tests failed.'}
