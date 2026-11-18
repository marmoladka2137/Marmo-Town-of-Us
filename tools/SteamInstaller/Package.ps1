param([Parameter(Mandatory=$true)][string]$PluginDll, [Parameter(Mandatory=$true)][string]$OutputDirectory)
$ErrorActionPreference='Stop'
$repository=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$manifest=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'installer-manifest.json') -Raw | ConvertFrom-Json
if ((Get-FileHash -LiteralPath $PluginDll).Hash -ne $manifest.Payload.Sha256) { throw 'The plugin must match the source-pinned installer manifest.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
& (Join-Path $PSScriptRoot 'Build.ps1') -OutputDirectory $OutputDirectory
& (Join-Path $PSScriptRoot '../Launcher/Build.ps1') -OutputDirectory $OutputDirectory
$launcher=Get-Item -LiteralPath (Join-Path $OutputDirectory 'Marmo Town of Us.exe')
$manifest | Add-Member -Force -NotePropertyName Launcher -NotePropertyValue ([ordered]@{ Path='Marmo Town of Us.exe'; Bytes=$launcher.Length; Sha256=(Get-FileHash -LiteralPath $launcher.FullName).Hash })
New-Item -ItemType Directory -Path (Join-Path $OutputDirectory 'Payload') -Force | Out-Null
Copy-Item -LiteralPath $PluginDll -Destination (Join-Path $OutputDirectory 'Payload/TOU-Marmo.dll')
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'installer-manifest.json') -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination (Join-Path $OutputDirectory 'README.txt')
Copy-Item -LiteralPath (Join-Path $repository 'STEAM_INSTALLER_TESTS.md') -Destination $OutputDirectory
$notices=Join-Path $OutputDirectory 'Notices'
New-Item -ItemType Directory -Path $notices -Force | Out-Null
foreach ($name in @('LICENSE','CREDITS.md','THIRD_PARTY_NOTICES.md','ASSET_CREDITS.md','MTOU_ARTWORK_LICENSE.md','DEPENDENCIES.md','MOD_POLICY_COMPLIANCE.md','TEXT_UPDATE_TESTS.md')) {
 Copy-Item -LiteralPath (Join-Path $repository $name) -Destination $notices
}
Copy-Item -LiteralPath (Join-Path $repository 'LICENSES') -Destination $notices -Recurse -Force
$sources=Join-Path $OutputDirectory 'Source/SteamInstaller'
New-Item -ItemType Directory -Path $sources -Force | Out-Null
foreach ($name in @('InstallerCore.cs','InstallerApp.cs','InstallerTests.cs','Build.ps1','Test.ps1','Package.ps1','installer-manifest.json','README.md')) {
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $sources
}
Copy-Item -LiteralPath (Join-Path $repository 'LICENSE') -Destination $sources
$launcherSources=Join-Path $OutputDirectory 'Source/Launcher'
New-Item -ItemType Directory -Path $launcherSources -Force | Out-Null
Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot '../Launcher') -File | Copy-Item -Destination $launcherSources
Copy-Item -LiteralPath (Join-Path $repository 'LICENSE') -Destination $launcherSources
Write-Output "Prepared installer package at $OutputDirectory"
