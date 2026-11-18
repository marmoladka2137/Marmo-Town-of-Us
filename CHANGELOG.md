# Changelog

## 1.1.0-beta — publication candidate, 2026-11-17

Prepared from the latest local feature baseline internally numbered 1.1.5-beta. The installed test DLL was internally 1.1.4; the older source ZIP was 1.1.0-beta. Normalizing the publication version is not a rollback of source fixes. Display version remains 1.1 BETA and plugin ID remains pl.marmo.toumarmo. Do not mix these binaries despite the same display label.

- Portable GameRoot configuration, explicit missing-reference errors and pinned compile-only Perfect Comms API.
- Removed unused Costura/Fody weaving; dependencies remain external and replaceable.
- Kept native ModManager updates and explicitly activates the required stamp.
- Added a stable self-assigned UUID through the game's official all-client registration helper; no manual protocol rewrite.
- Corrected mod metadata to identify MTOU and retain upstream authors.
- Added source/asset/dependency inventories, catalogs, licensing evidence, build/install and review materials.
- Preserved the supplied Jester/Spectre, Deputy, Doctor, protection and other gameplay implementations. This preparation does not claim new full-match validation of those fixes.

Older per-feature dates cannot be reconstructed reliably from the non-Git root project. See SOURCE_PROVENANCE.md instead of treating filesystem timestamps as commit history.

### Owner clarification — 2026-11-17

Retain current Double Shot, Triple Shot and Hugger code. Retirement remains unresolved. Documentation and source review archive updated; DLL unchanged. See docs/RELEASE_DECISIONS.md.

## 1.1.0-beta.2 — gameplay fixes, 2026-11-17

See GAMEPLAY_FIX_REPORT.md: shared typed options, random Executioner selection, persistent Doctor uniform, living corpse presentation/material and attack handling, Gambler checks, private Dirtek bond, new Composter floor sprite. Full match/multiplayer remains unverified; Practice startup was blocked by EOS initialization.

## 1.1.0-beta.3 — 2026-11-17
- Synchronize accepted, blocked and successful Furas attacks, cooldowns, duplicate/stale packet handling and puppet cleanup.
- Add independent All / ToU / MToU origin filters to Modifiers.
- Preserve beta.2 gameplay changes; live multiplayer validation remains pending.


## 1.1.0-beta.4 — 2026-11-17
- Replace local menu/loading/splash/Legacy/achievement/plugin branding with owner-supplied Marmo logo; preserve image proportions and upstream credits.
- Keep beta.3 gameplay and modifier filters unchanged. Discord remote asset upload remains separate.


## 1.1.0-beta.5 — 2026-11-17
- Replace One More Chance modifier artwork with a mint heart and golden return arrow; preserve previous icon height.
- Keep beta.4 branding and all preceding gameplay changes.


## 1.1.0-beta.6 — 2026-11-17
- Fit the two-line Marmo logo between account strip and actual Play button with responsive sizing.
- Remove MTOU achievement registration, progress/unlock effects, lottery startup and event registrations.
- Remove AchievementsAPI build/runtime reference; hide its menu/option entry if independently installed.
- Keep role mechanics, previous branding/icon updates and existing achievement save data.


## 1.1.0-beta.7 — 2026-11-17
- Add clickable dc.gg/mtou text in the former achievement-button area; share its HTTPS destination with the existing Discord button.
- Retain achievement removal, corrected logo placement and all previous gameplay changes.


## 1.1.0-beta.8 — 2026-11-17

- Preserve explicit wardrobe choices in a small separate cosmetic-ID preference file, protect native profile saves/reloads, and retain intentional unequip.
- Remove replacement of PlayerData/SettingsData instances during integrated startup.
- Wait for asynchronous custom-cosmetic installation before refreshing a lobby outfit.
- Enlarge the supplied logo within the available menu space.
- Fix the invisible community label by initializing all native PassiveButton text colors; restore the link after menu rebuilding and add lobby display with chat/settings/customization visibility guards.
- Add opt-in isolated QA profile paths and a write boundary. No startup resolution override is added. The test machine's saved window size was repaired separately.
- See PROFILE_DISPLAY_FIX.md for test evidence and remaining limits.

## 1.1.0 Beta — public version label, 2026-11-17

- Normalize the visible version to `1.1.0 Beta` and plugin version to `1.1.0-beta`.
- Keep all beta.8 gameplay, profile and menu fixes; distinguish the binary with assembly/file revision `1.1.0.9`.
- Refresh publication/submission documentation. No GitHub repository or public release was created; see PUBLICATION_STATUS.md.

## License and asset recheck — 2026-11-17

Added a standalone OFL-1.1 notice for the identified embedded Liberation Sans 2.00.1 font, corrected transitive/runtime license classifications, refreshed current asset/bundle inventories and recorded concrete unresolved provenance. No C# mechanics, original asset bytes, installed game files or DLL bytes changed. See LICENSE_AUDIT.md.

## 2026-11-18 — artwork licensing and publication review

Record Marmo attribution and owner-selected CC BY 4.0 for 38 pinned original PNGs. Keep GPLv3 code/upstream credits, original author credits and independent media terms. Refresh documentation; locate official TOU Unity source/animation terms; verify unchanged source/DLL correspondence. No mechanics or asset bytes changed. Full public binary release remains held for separate inherited-media issues.

## 1.1.0-beta — original animation candidate, 2026-11-18

- Replace Doctor Reveal and Deputy shot presentation with original code-defined motion, geometry and synthesized audio.
- Keep Doctor wardrobe and role mechanics; retain Deputy queue/deduplication/interrupt cleanup.
- Exclude two unused legacy Doctor WAVs from the DLL; full inherited bundles still require clearance.
- Internal assembly revision: 1.1.0.10. Public version label remains 1.1.0 Beta.


## 1.1.0-beta — internal revision 1.1.0.12 (2026-11-18)

- Replace all 55 bundled visual motion contracts with 1,511 original generated frames and owned runtime playback.
- Remove inherited motion/controller/audio/shader payload, preserve layout/clip interfaces, synthesize 59 cues and use native game shaders with original surface fills.
- Supply preferred resource sources and reproducible generators; replace ambiguous duplicate Composter icons with verified originals.
- Replace uncertain AutoRejoin implementation with original deadline/UI code while retaining preferences and historical credits.
- Update licensing, publication evidence, manifests and package documentation; full gameplay/multiplayer remains unverified.

## 1.1.0-beta — publication revision 1.1.0.12

- Connect the Source Code menu button to the public MTOU repository.
- Update publication documentation and metadata; role mechanics unchanged.
- Preserve predecessor test scope; no full gameplay or multiplayer certification.
