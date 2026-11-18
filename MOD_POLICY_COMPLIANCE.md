# Innersloth mod policy review

Rechecked 2026-11-18 against the official [Mod Policy](https://www.innersloth.com/among-us-mod-policy/) (displayed modification date July 30, 2026) and [technical modding information](https://github.com/Innersloth-LLC/AmongUsModdingInformation). These may change; this document records observed status rather than promising continuing approval.

| Requirement / topic | Evidence and status |
|---|---|
| Unofficial identification and disclaimer | Exact disclaimer included in README and submission material; no upstream/Innersloth affiliation claimed. Prepared. |
| Stamp throughout gameplay | Publication patch preserves native ModManager.LateUpdate and activates ShowModStamp when inactive. Native startup fixture reports activeInHierarchy=true. Full gameplay/meeting/scene coverage still needs observation. |
| All-client classification | RequireOnAllClients attribute plus custom role/RPC architecture. All participants need compatible mods. Not host-only. |
| Lobby registration on official servers | Added self-assigned UUID `f3dc9545-e8f3-4a99-a95d-17ae396dd698` via `CurrentModRegistration.ModRegistrationGuidString`, the documented built-in helper. Native parser returns true on local game 2026.9.29. Real official-server hosting, filtering and joining not tested. |
| Correct registration mechanism | The official documentation explicitly describes a self-assigned UUID. No request for an assigned UUID is invented. No +25 host-only protocol adjustment or hand-written packet modification was added. Plugin ID stays pl.marmo.toumarmo. |
| No base game in distribution | Source/plugin-only archives exclude game executable, Unity libraries, generated interop and complete game data. Archive checks recorded separately. |
| Among Us IP / third-party permissions | Owner-confirmed original art and new original motion replace the formerly uncertain animation/recording/GPU payload. Separate credits and current scope: ASSET_CREDITS.md. This is not a legal or moderator certification. |
| Monetization / prohibited behavior | No payment requirement or advertisement system was added. This review did not certify every historical debug/compatibility path against every policy restriction. Unrelated AUnlocker is not included or required. |

The main plugin uses the helper during Load, before creating or browsing a lobby. All MTOU releases should retain the same UUID. Do not reuse a different mod's registration or silently change the UUID per client. Interoperability with another all-client mod that overwrites the registration field requires a separately documented compatibility design and is not certified.

This preparation is not a claim of complete compliance. Live registration/matchmaking and uninterrupted gameplay stamp visibility remain unverified. Current resource replacement evidence is in LICENSE_AUDIT.md. No mod registration email, public publication or network-lobby submission was sent on the owner's behalf.

## Required disclaimer

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. Â© Innersloth LLC.

## Licensing recheck â€” 2026-11-17

The exact required disclaimer remains in README. The final thin packages were inspected and contain no base game, core/interop files or private profile data. This recheck examined source and packaging; it did not repeat official-server lobby or all-state gameplay stamp tests. The presence of ShowModStamp/registration code is not treated as proof of every runtime policy condition. Fonts and third-party compiled assets retain their separate rights; see the refreshed LICENSE_AUDIT.md and ASSET_CREDITS.md. No full legal/policy clearance or moderator acceptance is asserted.


## Current review â€” 2026-11-18

The official policy was fetched again; its displayed modification date remains July 30, 2026. Source/notice checks are current; historic native fixture outcomes are not described as newly executed tests. Full binary publication is not cleared merely by the artwork declaration: inherited-media terms and live registration/stamp/install behavior remain separate. Current archives contain no base game or dependent implementations. Reduced public code review is explicitly non-playable and has no binary release download.
