param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'bin'),[string]$SigningCertificateThumbprint)
$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$launcherMetadata=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'launcher-version.json') -Raw | ConvertFrom-Json
$gameRevision=[string]$launcherMetadata.AssemblyVersion
$gameDisplayVersion=[string]$launcherMetadata.DisplayVersion
if($gameRevision -notmatch '^\d+\.\d+\.\d+\.\d+$' -or $gameDisplayVersion -notmatch '^\d+\.\d+\.\d+$'){throw 'Invalid launcher version.'}
$versionCode=@"
// Generated from launcher-version.json; independent of the mod version.
namespace Marmo.Launcher {
    public static class LauncherVersion {
        public const string DisplayVersion = "$gameDisplayVersion";
        public const string AssemblyVersion = "$gameRevision";
    }
}
"@
Set-Content -LiteralPath (Join-Path $PSScriptRoot 'LauncherVersion.cs') -Value $versionCode -Encoding UTF8
[xml]$nativeManifest=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Launcher.manifest') -Raw -Encoding UTF8
$nativeManifest.assembly.assemblyIdentity.version=$gameRevision
$manifestPath=Join-Path (Resolve-Path $OutputDirectory).Path 'Launcher.generated.manifest'
$nativeManifest.Save($manifestPath)
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /codepage:65001 /warn:4 "/win32icon:$PSScriptRoot/Logo.ico" "/win32manifest:$manifestPath" "/out:$OutputDirectory/Marmo Town of Us.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll "/resource:$PSScriptRoot/Banner.png,Marmo.Banner.png" "/resource:$PSScriptRoot/DiscordLogo.png,Marmo.DiscordLogo.png" (Join-Path $PSScriptRoot 'LauncherCore.cs') (Join-Path $PSScriptRoot 'FolderPicker.cs') (Join-Path $PSScriptRoot 'LauncherVersion.cs') (Join-Path $PSScriptRoot 'LauncherApp.cs') (Join-Path $PSScriptRoot 'LauncherForm.cs') (Join-Path $PSScriptRoot 'ReleaseUpdate.cs') (Join-Path $PSScriptRoot 'VersionLibrary.cs') (Join-Path $PSScriptRoot '../SteamInstaller/InstallerCore.cs')
if($LASTEXITCODE -ne 0){throw 'Launcher build failed.'}
if($SigningCertificateThumbprint){
    $cert=Get-Item -LiteralPath "Cert:/CurrentUser/My/$SigningCertificateThumbprint"
    if(!$cert.HasPrivateKey -or $cert.NotAfter -le (Get-Date)){throw 'Select a valid code-signing certificate with its private key.'}
    $signature=Set-AuthenticodeSignature -FilePath "$OutputDirectory/Marmo Town of Us.exe" -Certificate $cert -HashAlgorithm SHA256 -TimestampServer 'http://timestamp.digicert.com'
    if($signature.Status -ne 'Valid'){throw "Code signature is not trusted: $($signature.Status)"}
}
Write-Output "Built $OutputDirectory/Marmo Town of Us.exe"
