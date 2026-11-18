param([Parameter(Mandatory=$true)][string]$PluginDll,[Parameter(Mandatory=$true)][string]$LauncherExe,[string]$OutputDirectory=(Join-Path $PSScriptRoot 'bin'))
$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
'using System.IO; public static class Child { public static void Main() { File.WriteAllText("launcher-child.txt",System.Environment.CurrentDirectory); } }' | Set-Content -LiteralPath (Join-Path $OutputDirectory 'Child.cs')
& $compiler /nologo /target:exe "/out:$OutputDirectory/Child.exe" (Join-Path $OutputDirectory 'Child.cs')
if($LASTEXITCODE -ne 0){throw 'Fixture child compilation failed'}
& $compiler /nologo /target:exe "/out:$OutputDirectory/LauncherTests.exe" (Join-Path $PSScriptRoot 'LauncherCore.cs') (Join-Path $PSScriptRoot 'LauncherTests.cs')
if($LASTEXITCODE -ne 0){throw 'Launcher test compilation failed'}
& (Join-Path $OutputDirectory 'LauncherTests.exe') (Resolve-Path -LiteralPath $PluginDll).Path (Resolve-Path -LiteralPath $LauncherExe).Path (Join-Path $OutputDirectory 'Child.exe')
if($LASTEXITCODE -ne 0){throw 'Launcher tests failed'}
