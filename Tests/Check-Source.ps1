param([string]$Root = (Join-Path $PSScriptRoot '..'), [string]$GameRoot = $env:AMONG_US_ROOT, [string]$Dotnet = 'dotnet')
$ErrorActionPreference='Stop'
$project=Join-Path (Resolve-Path $Root).Path 'TOU-Marmo.csproj'
# Use the evaluated project's exclusions, symbols, resources and actual integrated references.
& $Dotnet build $project -c Release -v:q "-p:GameRoot=$GameRoot"
if ($LASTEXITCODE -ne 0) { throw "Integrated source compilation failed ($LASTEXITCODE)." }
Write-Output 'PASS: evaluated integrated C# project compiles. This does not verify gameplay.'
