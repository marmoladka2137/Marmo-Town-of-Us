param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'bin'),[string]$SigningCertificateThumbprint)
$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /codepage:65001 /warn:4 "/win32icon:$PSScriptRoot/Logo.ico" "/win32manifest:$PSScriptRoot/Launcher.manifest" "/out:$OutputDirectory/Marmo Town of Us.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll "/resource:$PSScriptRoot/Banner.png,Marmo.Banner.png" "/resource:$PSScriptRoot/DiscordLogo.png,Marmo.DiscordLogo.png" (Join-Path $PSScriptRoot 'LauncherCore.cs') (Join-Path $PSScriptRoot 'FolderPicker.cs') (Join-Path $PSScriptRoot 'LauncherApp.cs') (Join-Path $PSScriptRoot 'LauncherForm.cs') (Join-Path $PSScriptRoot 'ReleaseUpdate.cs') (Join-Path $PSScriptRoot '../SteamInstaller/InstallerCore.cs')
if($LASTEXITCODE -ne 0){throw 'Launcher build failed.'}
if($SigningCertificateThumbprint){
    $cert=Get-Item -LiteralPath "Cert:/CurrentUser/My/$SigningCertificateThumbprint"
    if(!$cert.HasPrivateKey -or $cert.NotAfter -le (Get-Date)){throw 'Select a valid code-signing certificate with its private key.'}
    $signature=Set-AuthenticodeSignature -FilePath "$OutputDirectory/Marmo Town of Us.exe" -Certificate $cert -HashAlgorithm SHA256 -TimestampServer 'http://timestamp.digicert.com'
    if($signature.Status -ne 'Valid'){throw "Code signature is not trusted: $($signature.Status)"}
}
Write-Output "Built $OutputDirectory/Marmo Town of Us.exe"
