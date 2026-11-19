# Building from source

The project targets `net6.0` for BepInEx compatibility. Install .NET SDK **10.0.403** (or a compatible patch selected by `global.json`); .NET 6 SDK alone cannot compile the modern C# syntax. PowerShell 7 is used by the included rule tests. Do not change the runtime target solely because the build SDK is newer.

1. Obtain the source and a legally owned compatible Among Us installation.
2. Install the required BepInEx IL2CPP and dependency plugins from [DEPENDENCIES.md](DEPENDENCIES.md), launch once to generate `BepInEx/interop`, then close that development instance.
3. Set the path, preserving spaces:

```powershell
$env:AMONG_US_ROOT = 'D:\Games\Among Us Development'
dotnet restore TOU-Marmo.csproj --locked-mode
dotnet build TOU-Marmo.csproj -c Release --no-restore
```

Alternatively pass `"-p:GameRoot=D:\Games\Among Us Development"` to both commands. `GameRoot` is only a reference root; output stays in `bin/Release/net6.0/TOU-Marmo.dll`. No post-build deployment runs. No `TownOfUsMira.dll` reference is required.

NuGet restores the exact `PerfectComms.Api` 4.1.7.1 compile-time package. It does not install the Perfect Comms runtime. Costura/Fody was removed from this publication copy: inspected original binaries did not embed dependent DLLs, and dynamically replaceable dependencies remain external.

Core/game references use `Private=false`; do not ship Unity, Among Us, generated interop DLLs or the entire development installation. Resource logical names beginning `TownOfUs.Resources` are intentionally preserved for the integrated loader.

## Tests

```powershell
pwsh -NoProfile -File Tests/Check-Rules.ps1
pwsh -NoProfile -File Tests/Check-JesterExile.ps1
pwsh -NoProfile -File Tests/Check-Spikes.ps1
pwsh -NoProfile -File Tests/Check-Source.ps1
```

Run each script in a separate PowerShell process because Add-Type caches generated classes. These test deterministic rule code and compilation, not complete meetings, input handling, animation or network synchronization. See TEST_RESULTS.md.

## Reproducibility limits

The source archive was extracted into a different directory and rebuilt with the same local GameRoot; the output SHA-256 matched exactly. The exact game-generated interop and development MiraAPI DLL must still be supplied independently. The observed MiraAPI build has no proven source-to-binary match. A fresh third-party build with only public prerequisites has not been certified. Do not generalize the local matching build to all environments or publish private game references as a shortcut.

## Profile persistence tests

```powershell
dotnet run --project tools/ProfileStoreTests/StoreTests.csproj -c Release
```

This pure managed test runner uses .NET 10 and links the same cosmetic-store source that the mod compiles for .NET 6. It uses synthetic temporary data and does not launch Among Us or touch its profile. The two-launch native fixture results and limits are documented in PROFILE_DISPLAY_FIX.md.

## Corresponding source scope

This revision supplies a complete source candidate with preferred frames and sanitized resource graphs. Earlier reduced code-review archives are historical and are not matching source for this DLL. Rebuild against separately obtained legally owned game references; do not publish those references as part of the source package.

## Presentation resources

Compile against the checked-in native bundle resources. Procedural animation replacement tools and frames from earlier experiments are excluded from this revision. Their dated audit reports describe historical builds, not the current DLL. Upstream resource authorship and separate rights remain applicable; no blanket legal clearance is asserted by compilation.
