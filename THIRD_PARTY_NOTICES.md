# Third-party notices â€” current publication

Combined TOU Mira/MTOU C# is GPLv3. Preserve copyright, modification notices and corresponding source. Original MTOU graphics are separately CC BY 4.0, Marmo attribution. The retained Liberation Sans font is OFL-1.1. Full texts are in LICENSES.

Static TOU Mira sprites keep AU-Avengers/AtonyGit/other original credits under the official credited-sprite permission. Native Mira presentation resources retain independent rights; see ASSET_CREDITS.md and LICENSE_AUDIT.md for unresolved permissions.

## External dependencies â€” no DLL payload included

The [35-file runtime table](docs/audit/recheck-2026-11-17/COMPONENTS.md) lists current hashes, version-specific primary license evidence, package matches and limitations for each inspected core/direct runtime binary. **None of those DLLs is embedded in MTOU or redistributed in the thin plugin ZIP.**

- **MiraAPI 0.5.0-dev:** available source pin 7a4e891 is LGPL 2.1; installed binary's exact modified source remains unproven. All Of Us, XtraCube, Angxl and contributors are retained as upstream credits. Exact-source uncertainty is not represented as a proven license conflict.
- **Steam installer:** downloads pinned loader/runtime/plugins directly from the official TOU Mira 1.7.9-beta1 distribution. Its MiraAPI 0.6.0 DLL is a separate public baseline, not a claimed source match for the historical local 0.5.0-dev DLL. No external dependency implementations are redistributed in the installer ZIP. Their existing authors, independent terms and upstream source locations remain applicable; see DEPENDENCIES.md and tools/SteamInstaller/installer-manifest.json.
- **Reactor 2.5.1:** LGPLv3 source text verified at the exact version. **BepInEx be.753 / 0d275a4:** LGPL 2.1 text verified at the source commit. Preserve required notices/source access if later redistributing them.
- **HarmonyX 2.10.2, CorsacCosmetics 4.0.0 / 9ddf483:** MIT. Corsac code copyright Filip Kiraa OpĂˇlenĂ˝ and contributors; preserve the exact MIT notice when conveying code. Its art has separate provenance.
- **Il2CppInterop Runtime/Common 1.5.3:** official package expression LGPL-3.0-only. Installed metadata identifies 5d129493; official NuGet package identifies dbda1cb3 and does not byte-match. **Generator/HarmonySupport CI 620 / 763f427:** exact commit LICENSE is GPLv3, not the later LGPL text. The previous single-family classification was incomplete.
- **AsmResolver 6.0.0-beta.2, AssetRipper.CIL 1.1.2, AssetRipper.Primitives 3.1.3, Cecil 0.11.4, MonoMod 22.7.31.1 / helpers, Iced 1.21.0, SemanticVersioning 2.0.2, Disarm and Cpp2IL-family files:** MIT evidence is recorded at the source pin or exact NuGet package. AssetRipper.Primitives includes a separate uTinyRipper MIT notice (Â© 2020 mafaca), now preserved. Capstone.NET exact source pin b90e380c is MIT (Ahmed Garhy); an additionally bundled native Capstone engine would need its own review.
- **Dobby:** upstream Apache-2.0 text collected as context. Installed native binary's exact source/version remains unknown; its current-upstream license is not certified as an exact binary match.
- **PerfectComms.Api 4.1.7.1:** LGPL-2.1-only in the exact NuGet package, commit 59068ee4. Compile-only API integration; runtime implementation is not bundled. This is separate from the full voice-chat mod.

Dynamic separate references and complete application source support library replacement/relinking. No modified-library replacement test or full-loader compliance certification is claimed. If conveying a modified LGPL/GPL library later, supply its applicable notices and exact corresponding source. No blanket relicensing of dependency implementations was performed.

## Optional cosmetics and historical components

TownOfUs-Cosmetics has an explicit [GPLv3 license](https://github.com/AU-Avengers/TownOfUs-Cosmetics/blob/c39926b6f1037304caa558687fe07558ebb27c86/LICENSE); all 17 installed bundle hashes match its inspected tree. They are runtime downloads, not ZIP payload. Their GPL text is included as provenance/notice. Artist-specific sub-assets, preferred editable source and future mutable-main downloads are not certified.

AchievementsAPI was removed from MTOU's references and required plugins from the current build; the prior row calling it a direct dependency was stale. Its old GPLv3 text at e62c9c9 remains historical evidence. Fody 6.5.5 and Costura.Fody 5.7.0 were checked from their exact packages (MIT) but are absent from the current build and release payload. No Costura-embedded implementation DLLs were found.

.NET, Unity, Rewired and the game-provided Discord/TMP/IL2CPP wrappers are references to the development/game environment, not redistributed files or open-source licenses inferred from MTOU. A complete .NET/native/loader pack would require its own version-specific notices. AleLuduMod, AUnlocker, Mini.RegionInstall and the separate TownOfUsMira plugin are not MTOU prerequisites or package payload.


## Source ancestry and replacement tooling

Existing GPL ancestry/attribution for Town of Us, Redux, Town-Of-Us-R, Launchpad Reloaded, Better Among Us and Unlock dlekS remains. The previously uncertain external AutoRejoin implementation is replaced by new GPL MTOU source. Maxi0fc remains credited historically; no new permission is fabricated and no formal clean-room claim is made.

Optional asset rebuilding uses UnityPy 1.25.2 (MIT) and Pillow (HPND/Pillow terms) as separately installed development tools. Their implementation files are not bundled in source/release ZIPs. requirements.txt pins tested versions; obtain tool licenses with their official distributions. The original frame generator is included GPL C# with no third-party drawing library. Unity Editor and authentication are not required by this resource pipeline.

Remaining binary/source uncertainty concerns the external loader/dependency environment, not embedded dependency copies. No full-loader redistribution is cleared by this plugin-only preparation.

## Additional launcher plugin payload (2026-11-18)

The launcher supplement includes AleLuduMod v1.2.3 (townofus-pl, MIT), AUnlocker v1.3.2 (AstralKitsune, GPLv3) and Mini.RegionInstall v1.2.0 (miniduikboot, GPLv3). Exact tagged source archives and license texts accompany these binaries in MTOU Notices/CorrespondingSource and LICENSES. Independent authorship and licenses are retained; these are not MTOU originals. The initial pinned official dependency download supplies BepInEx, Reactor, MiraAPI and CorsacCosmetics; this supplement does not redistribute that loader archive. Existing audited dependency notices remain applicable.
