> Follow-up 1.1.0-beta.2: see [GAMEPLAY_FIX_REPORT.md](GAMEPLAY_FIX_REPORT.md). The earlier publication snapshot and test history below remain baseline evidence, not verification of a complete match for this build.

# Modifiers

Native registration inventory, with assignable GameModifier types separated from temporary effects. Availability can be further restricted by IsModifierValidOn and host options. Double Shot and Triple Shot are still present in the input source: the owner confirmed retaining the current code on 2026-11-17, while retirement remains unresolved (see docs/RELEASE_DECISIONS.md).

## MTOU assignment settings

| Modifier | Eligibility / settings |
|---|---|
| Balloon | Universal; amount 0–15, chance 0–100%, balloon duration 1–15 seconds (default 3). |
| Big Brain | Separate Crew/Impostor/Neutral toggles; chance 0–100%, maximum players 0–15, detections per round 1–15, failed-ability detection toggle. No protection. |
| Kemada | Universal; amount 0–15, chance 0–100%, Can Be Guessed toggle. |
| One More Chance | Sheriff/Officer/Jailor/Furas only; chance 0–100%, count 0–15. |
| Simp | Impostor-only; amount 0–15, chance 0–100%, post-ejection cooldown multiplier 1.05–3 (default 1.25); pre-ejection multiplier 0.75. |
| Triple Shot | Existing Assassin/Double Shot eligibility with Impostor/Neutral switches in AssailantModifiersOptions; current code retained by owner decision; retirement unresolved. |

## Assignable modifiers

### Lover

**Faction/category:** Alliance · **Origin:** TOU Mira / see provenance for modifications

As a lover, you can chat with your other lover (signified with [symbol]) during the round, and you can win with your lover if you are both a part of the final three players.

**Eligibility, settings and restrictions:** [LoverModifier](IntegratedTownOfUs/Modifiers/Game/Alliance/LoverModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Assassin

**Faction/category:** AssailantUtility · **Origin:** TOU Mira / see provenance for modifications

You have the ability to guess a player in a meeting.

**Eligibility, settings and restrictions:** [AssassinModifier](IntegratedTownOfUs/Modifiers/Game/Assailant/AssassinModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Double Shot

**Faction/category:** AssailantUtility · **Origin:** TOU Mira / see provenance for modifications

You get a second chance when you fail to guess a player in a meeting.

**Eligibility, settings and restrictions:** [DoubleShotModifier](IntegratedTownOfUs/Modifiers/Game/Assailant/DoubleShotModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Overclocker

**Faction/category:** AssailantUtility · **Origin:** TOU Mira / see provenance for modifications

You may decrease your cooldowns at the cost of longer cooldowns once your Overclock is over. After an additional round, your cooldowns will normalize.

**Eligibility, settings and restrictions:** [OverclockerModifier](IntegratedTownOfUs/Modifiers/Game/Assailant/OverclockerModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Triple Shot

**Faction/category:** AssailantUtility · **Origin:** MTOU

Your first two incorrect guesses do not kill you. The third does. Requires guessing, is available to Impostors and Neutrals, and cannot coexist with Double Shot.

**Eligibility, settings and restrictions:** [TripleShotModifier](Modifiers/TripleShotModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Crewpostor

**Faction/category:** CrewmateAlliance · **Origin:** TOU Mira / see provenance for modifications

The Crewpostor is a Crewmate Alliance modifier, which forces a crewmate to work for the impostor faction. If all impostors are dead, you become a Traitor.

**Eligibility, settings and restrictions:** [CrewpostorModifier](IntegratedTownOfUs/Modifiers/Game/Alliance/CrewpostorModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Egotist

**Faction/category:** CrewmateAlliance · **Origin:** TOU Mira / see provenance for modifications

The Egotist is a Crewmate Alliance modifier (signified by [symbol]). As the Egotist, you can only win if Crewmates lose, even when dead. If no crewmates remain after a meeting ends, you will leave in victory, but the game will continue.

**Eligibility, settings and restrictions:** [EgotistModifier](IntegratedTownOfUs/Modifiers/Game/Alliance/EgotistModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Multitasker

**Faction/category:** CrewmatePassive · **Origin:** TOU Mira / see provenance for modifications

All your menus are see-through, allowing you to look behind menus!

**Eligibility, settings and restrictions:** [MultitaskerModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/MultitaskerModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Taskmaster

**Faction/category:** CrewmatePassive · **Origin:** TOU Mira / see provenance for modifications

Every time a round starts, you will automatically finish a task.

**Eligibility, settings and restrictions:** [TaskmasterModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/TaskmasterModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Aftermath

**Faction/category:** CrewmatePostmortem · **Origin:** TOU Mira / see provenance for modifications

After you die, your killer will be forced to use their abilities on your body or themselves.

**Eligibility, settings and restrictions:** [AftermathModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/AftermathModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Bait

**Faction/category:** CrewmatePostmortem · **Origin:** TOU Mira / see provenance for modifications

After you die, your killer will self-report, reporting your body.

**Eligibility, settings and restrictions:** [BaitModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/BaitModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Celebrity

**Faction/category:** CrewmatePostmortem · **Origin:** TOU Mira / see provenance for modifications

After you die, details about your death will be revealed such as where you were killed and which role killed you during the meeting.

**Eligibility, settings and restrictions:** [CelebrityModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/CelebrityModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Diseased

**Faction/category:** CrewmatePostmortem · **Origin:** TOU Mira / see provenance for modifications

Your murderer's kill cooldown is multiplied by a factor of [cooldownMultiplier]x.

**Eligibility, settings and restrictions:** [DiseasedModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/DiseasedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Frosty

**Faction/category:** CrewmatePostmortem · **Origin:** TOU Mira / see provenance for modifications

After you die, your killer will be slowed down!

**Eligibility, settings and restrictions:** [FrostyModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/FrostyModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Noisemaker

**Faction/category:** CrewmatePostmortem · **Origin:** TOU Mira / see provenance for modifications

After your death, you will show a red body indicator to everyone on the map.

**Eligibility, settings and restrictions:** [NoisemakerModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/NoisemakerModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Rotting

**Faction/category:** CrewmatePostmortem · **Origin:** TOU Mira / see provenance for modifications

After [rotDelay] second(s), your body will rot away, preventing you from being reported

**Eligibility, settings and restrictions:** [RottingModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/RottingModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Investigator

**Faction/category:** CrewmateUtility · **Origin:** TOU Mira / see provenance for modifications

The Investigator can see player's footprints throughout the game. Swooped players' footprints will not be visible to the Investigator.

**Eligibility, settings and restrictions:** [InvestigatorModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/InvestigatorModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Operative

**Faction/category:** CrewmateUtility · **Origin:** TOU Mira / see provenance for modifications

Use cameras at anytime with a limited battery charge.

**Eligibility, settings and restrictions:** [OperativeModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/OperativeModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Scientist

**Faction/category:** CrewmateUtility · **Origin:** TOU Mira / see provenance for modifications

Access Vitals at anytime with a limited battery charge.

**Eligibility, settings and restrictions:** [ScientistModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/ScientistModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Spy

**Faction/category:** CrewmateUtility · **Origin:** TOU Mira / see provenance for modifications

The Spy gains extra information on the admin table. They now not only see how many people are in a room, but see who is in every room.

**Eligibility, settings and restrictions:** [SpyModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/SpyModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Scout

**Faction/category:** CrewmateVisibility · **Origin:** TOU Mira / see provenance for modifications

While you can see twice as far as a regular crewmate, your vision falters when lights are off.

**Eligibility, settings and restrictions:** [ScoutModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/ScoutModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Torch

**Faction/category:** CrewmateVisibility · **Origin:** TOU Mira / see provenance for modifications

The lights being off do not affect your vision.

**Eligibility, settings and restrictions:** [TorchModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/TorchModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Multitasker

**Faction/category:** HiderPassive · **Origin:** TOU Mira / see provenance for modifications

All your menus are see-through, allowing you to look behind menus!

**Eligibility, settings and restrictions:** [HnsMultitaskerModifier](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsMultitaskerModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Oblivious

**Faction/category:** HiderPassive · **Origin:** TOU Mira / see provenance for modifications

No indicator is present to show you when the seeker is close.

**Eligibility, settings and restrictions:** [HnsObliviousModifier](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsObliviousModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Frosty

**Faction/category:** HiderPostmortem · **Origin:** TOU Mira / see provenance for modifications

After you die, your killer will be slowed down!

**Eligibility, settings and restrictions:** [HnsFrostyModifier](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsFrostyModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Transporter

**Faction/category:** HiderPostmortem · **Origin:** TOU Mira / see provenance for modifications

When killed, you will transport the seeker with a random hider.

**Eligibility, settings and restrictions:** [HnsTransporterModifier](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsTransporterModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Giant

**Faction/category:** HiderVisibility · **Origin:** TOU Mira / see provenance for modifications

You are bigger than the average player, sticking out to the seeker.

**Eligibility, settings and restrictions:** [HnsGiantModifier](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsGiantModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Mini

**Faction/category:** HiderVisibility · **Origin:** TOU Mira / see provenance for modifications

You are smaller than the average player, allowing you to hide from the seeker with ease.

**Eligibility, settings and restrictions:** [HnsMiniModifier](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsMiniModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Circumvent

**Faction/category:** ImpostorPassive · **Origin:** TOU Mira / see provenance for modifications

Your ability to vent around the map is severely weakened, or may be unavailable in general.

**Eligibility, settings and restrictions:** [CircumventModifier](IntegratedTownOfUs/Modifiers/Game/Impostor/CircumventModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Deadly Quota

**Faction/category:** ImpostorPassive · **Origin:** TOU Mira / see provenance for modifications

You lose if you do not complete your quota. To complete your quota, you need to kill a set number of people chosen at the start of the game.

**Eligibility, settings and restrictions:** [DeadlyQuotaModifier](IntegratedTownOfUs/Modifiers/Game/Impostor/DeadlyQuotaModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Saboteur

**Faction/category:** ImpostorPassive · **Origin:** TOU Mira / see provenance for modifications

You have a reduced cooldown when sabotaging.

**Eligibility, settings and restrictions:** [SaboteurModifier](IntegratedTownOfUs/Modifiers/Game/Impostor/SaboteurModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Simp

**Faction/category:** ImpostorPassive · **Origin:** MTOU

Impostor-only. A random protected target grants a 25% shorter kill cooldown while the target has not been ejected; ejection switches to the configured cooldown penalty.

**Eligibility, settings and restrictions:** [SimpModifier](Modifiers/SimpModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Underdog

**Faction/category:** ImpostorPassive · **Origin:** TOU Mira / see provenance for modifications

Your kill cooldown is lower if you're solo or your teammate is dead.

**Eligibility, settings and restrictions:** [UnderdogModifier](IntegratedTownOfUs/Modifiers/Game/Impostor/UnderdogModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Telepath

**Faction/category:** ImpostorPostmortem · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [TelepathModifier](IntegratedTownOfUs/Modifiers/Game/Impostor/TelepathModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Disperser

**Faction/category:** ImpostorUtility · **Origin:** TOU Mira / see provenance for modifications

Disperse everyone on the map to a random vent, given that they are not Immovable. You cannot have any other button modifiers with Disperser.

**Eligibility, settings and restrictions:** [DisperserModifier](IntegratedTownOfUs/Modifiers/Game/Impostor/DisperserModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Balloon

**Faction/category:** UniversalPassive · **Origin:** MTOU

When another player uses a targeted ability or attempts to kill the Balloon holder, a red balloon appears above the interacting player's head for the configured duration.

**Eligibility, settings and restrictions:** [BalloonModifier](Modifiers/BalloonModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Big Brain

**Faction/category:** UniversalPassive · **Origin:** MTOU

Learn the roles of players who used abilities on you at the next meeting, only if you survived. No names or ability names are shown.

**Eligibility, settings and restrictions:** [BigBrainModifier](Modifiers/BigBrainModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Drunk

**Faction/category:** UniversalPassive · **Origin:** TOU Mira / see provenance for modifications

Your controls are permanently inverted. You are also immune to normal roleblocks.

**Eligibility, settings and restrictions:** [DrunkModifier](IntegratedTownOfUs/Modifiers/Game/Universal/DrunkModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Immovable

**Faction/category:** UniversalPassive · **Origin:** TOU Mira / see provenance for modifications

You are unable to be moved via abilities and meetings.

**Eligibility, settings and restrictions:** [ImmovableModifier](IntegratedTownOfUs/Modifiers/Game/Universal/ImmovableModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Kemada

**Faction/category:** UniversalPassive · **Origin:** MTOU

Every vote cast by Kemada goes to a random living player other than the voter. The chosen vote is synchronized across the lobby. Kemada is guessable.

**Eligibility, settings and restrictions:** [KemadaModifier](Modifiers/KemadaModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### One More Chance

**Faction/category:** UniversalPassive · **Origin:** MTOU

Sheriff, Officer, Jailor and Furas only. The first qualifying mistake still kills the victim but consumes/removes this modifier and cancels the owner penalty. A subsequent mistake follows normal role rules. Not valid on Vigilante.

**Eligibility, settings and restrictions:** [OneMoreChanceModifier](Modifiers/OneMoreChanceModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Sixth Sense

**Faction/category:** UniversalPassive · **Origin:** TOU Mira / see provenance for modifications

You will know when someone uses their ability on you.

**Eligibility, settings and restrictions:** [SixthSenseModifier](IntegratedTownOfUs/Modifiers/Game/Universal/SixthSenseModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Sleuth

**Faction/category:** UniversalPassive · **Origin:** TOU Mira / see provenance for modifications

You will see the roles of bodies you report.

**Eligibility, settings and restrictions:** [SleuthModifier](IntegratedTownOfUs/Modifiers/Game/Universal/SleuthModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Tiebreaker

**Faction/category:** UniversalPassive · **Origin:** TOU Mira / see provenance for modifications

Your vote allows you to break ties.

**Eligibility, settings and restrictions:** [TiebreakerModifier](IntegratedTownOfUs/Modifiers/Game/Universal/TiebreakerModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Button Barry

**Faction/category:** UniversalUtility · **Origin:** TOU Mira / see provenance for modifications

You can button from anywhere on the map.

**Eligibility, settings and restrictions:** [ButtonBarryModifier](IntegratedTownOfUs/Modifiers/Game/Universal/ButtonBarryModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Radar

**Faction/category:** UniversalUtility · **Origin:** TOU Mira / see provenance for modifications

You have an arrow pointing  to the closest player.

**Eligibility, settings and restrictions:** [RadarModifier](IntegratedTownOfUs/Modifiers/Game/Universal/RadarModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Satellite

**Faction/category:** UniversalUtility · **Origin:** TOU Mira / see provenance for modifications

You can check for bodies on the map, which you can do [maxUses] time(s) per game.

**Eligibility, settings and restrictions:** [SatelliteModifier](IntegratedTownOfUs/Modifiers/Game/Universal/SatelliteModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Flash

**Faction/category:** UniversalVisibility · **Origin:** TOU Mira / see provenance for modifications

You move [flashSpeed] x faster than regular players.

**Eligibility, settings and restrictions:** [FlashModifier](IntegratedTownOfUs/Modifiers/Game/Universal/FlashModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Giant

**Faction/category:** UniversalVisibility · **Origin:** TOU Mira / see provenance for modifications

You are bigger than regular players, and you also move [giantSpeed]x slower than regular players.

**Eligibility, settings and restrictions:** [GiantModifier](IntegratedTownOfUs/Modifiers/Game/Universal/GiantModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Mini

**Faction/category:** UniversalVisibility · **Origin:** TOU Mira / see provenance for modifications

You are smaller than the average player, moving [miniSpeed]x faster.

**Eligibility, settings and restrictions:** [MiniModifier](IntegratedTownOfUs/Modifiers/Game/Universal/MiniModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Shy

**Faction/category:** UniversalVisibility · **Origin:** TOU Mira / see provenance for modifications

You blend in with the environment, becoming transparent when staying still.

**Eligibility, settings and restrictions:** [ShyModifier](IntegratedTownOfUs/Modifiers/Game/Universal/ShyModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

## Internal effects and role markers (not random lobby modifiers)

### Alerted

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [VeteranAlertModifier](IntegratedTownOfUs/Modifiers/Crewmate/VeteranAlertModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Altruist Arrow

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [AltruistArrowModifier](IntegratedTownOfUs/Modifiers/Crewmate/AltruistArrowModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Ambusher Arrow

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [AmbusherArrowTargetModifier](IntegratedTownOfUs/Modifiers/Impostor/AmbusherArrowTargetModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Amnesiac  Arrow

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [AmnesiacArrowModifier](IntegratedTownOfUs/Modifiers/Neutral/AmnesiacArrowModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Ballooned

**Faction/category:**  · **Origin:** MTOU

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [BalloonHeadModifier](Modifiers/BalloonHeadModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Barrier

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HerbalistProtectionModifier](IntegratedTownOfUs/Modifiers/Impostor/Herbalist/HerbalistProtectionModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Barrier

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ClericBarrierModifier](IntegratedTownOfUs/Modifiers/Crewmate/ClericBarrierModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Basic Ghost

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [BasicGhostModifier](IntegratedTownOfUs/Modifiers/BasicGhostModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Bitten

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [VampireBittenModifier](IntegratedTownOfUs/Modifiers/Neutral/VampireBittenModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Blackmailed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [BlackmailedModifier](IntegratedTownOfUs/Modifiers/Impostor/BlackmailedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Blackmailed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [BlackmailSparedModifier](IntegratedTownOfUs/Modifiers/Impostor/BlackmailSparedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Blessed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [OracleBlessedModifier](IntegratedTownOfUs/Modifiers/Crewmate/OracleBlessedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Blinded

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [EclipsalBlindModifier](IntegratedTownOfUs/Modifiers/Impostor/EclipsalBlindModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Camouflaged

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HnsGlobalCamouflageModifier](IntegratedTownOfUs/Modifiers/HnsImpostor/HnsGlobalCamouflageModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Camouflaged

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [VenererCamouflageModifier](IntegratedTownOfUs/Modifiers/Impostor/Venerer/VenererCamouflageModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Campaigned

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [PoliticianCampaignedModifier](IntegratedTownOfUs/Modifiers/Crewmate/PoliticianCampaignedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Camped

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [DeputyCampedModifier](IntegratedTownOfUs/Modifiers/Crewmate/DeputyCampedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Chef Served

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ChefServedModifier](IntegratedTownOfUs/Modifiers/Neutral/ChefServedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Cleric Cleanse

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ClericCleanseModifier](IntegratedTownOfUs/Modifiers/Crewmate/ClericCleanseModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Confess

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [OracleConfessModifier](IntegratedTownOfUs/Modifiers/Crewmate/OracleConfessModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Confused

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HerbalistConfusedModifier](IntegratedTownOfUs/Modifiers/Impostor/Herbalist/HerbalistConfusedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Death Notifier

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HnsMysticDeathNotifModifier](IntegratedTownOfUs/Modifiers/HnsCrewmate/HnsMysticDeathNotifModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Death Notifier

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [TelepathDeathNotifierModifier](IntegratedTownOfUs/Modifiers/Game/Impostor/TelepathDeathNotifierModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Death Notifier

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ChefArrowModifier](IntegratedTownOfUs/Modifiers/Neutral/ChefArrowModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Death Notifier

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MysticDeathNotifierModifier](IntegratedTownOfUs/Modifiers/Crewmate/MysticDeathNotifierModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Doctor Shield

**Faction/category:**  · **Origin:** MTOU

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [DoctorShieldModifier](Modifiers/DoctorShieldModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Doused

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ArsonistDousedModifier](IntegratedTownOfUs/Modifiers/Neutral/ArsonistDousedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Drag

**Faction/category:**  · **Origin:** MTOU

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ExorcistDragModifier](Modifiers/ExorcistDragModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Drag

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [DragModifier](IntegratedTownOfUs/Modifiers/Impostor/DragModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Executioner Target

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ExecutionerTargetModifier](IntegratedTownOfUs/Modifiers/Neutral/ExecutionerTargetModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Exposed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HerbalistExposedModifier](IntegratedTownOfUs/Modifiers/Impostor/Herbalist/HerbalistExposeModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### First Body Arrow

**Faction/category:**  · **Origin:** MTOU

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [JesterFirstBodyArrowModifier](Modifiers/JesterFirstBodyArrowModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### First Death Shield (Disguise)

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [FirstDeadShieldDisguiseVisual](IntegratedTownOfUs/Modifiers/FirstDeadShieldDisguiseVisual.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### First Round Death Indicator

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [FirstRoundIndicator](IntegratedTownOfUs/Modifiers/FirstRoundIndicator.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Flashed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [GrenadierFlashModifier](IntegratedTownOfUs/Modifiers/Impostor/GrenadierFlashModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Footsteps

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [FootstepsModifier](IntegratedTownOfUs/Modifiers/Crewmate/FootstepsModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Fortified

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [WardenFortifiedModifier](IntegratedTownOfUs/Modifiers/Crewmate/WardenFortifiedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Freeze

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [VenererFreezeModifier](IntegratedTownOfUs/Modifiers/Impostor/Venerer/VenererFreezeModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Frozen

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [FrozenModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/FrozenModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Frozen

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HnsFrozenModifier](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsFrozenModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Gazing

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MedusaGazingModifier](IntegratedTownOfUs/Modifiers/Neutral/MedusaGazingModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Guardian Angel Target

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [GuardianAngelTargetModifier](IntegratedTownOfUs/Modifiers/Neutral/GuardianAngelTargetModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Hacked

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [GlitchHackedModifier](IntegratedTownOfUs/Modifiers/Neutral/GlitchHackedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Hangover

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HangoverModifier](IntegratedTownOfUs/Modifiers/Other/HangoverModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Haunter Arrow

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HaunterArrowModifier](IntegratedTownOfUs/Modifiers/Crewmate/HaunterArrowModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Hexed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [SpellslingerHexedModifier](IntegratedTownOfUs/Modifiers/Impostor/SpellslingerHexedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Hidden

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MediumHiddenModifier](IntegratedTownOfUs/Modifiers/Crewmate/MediumHiddenModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Hint Arrow

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [PuppeteerHintArrowModifier](IntegratedTownOfUs/Modifiers/Impostor/PuppeteerHintArrowModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Hypnotised

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HypnotisedModifier](IntegratedTownOfUs/Modifiers/Impostor/HypnotisedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Hypnotist Hysteria

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HypnotistHysteriaModifier](IntegratedTownOfUs/Modifiers/Impostor/HypnotistHysteriaModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Ignore Invulnerability

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [IgnoreInvulnerabilityModifier](IntegratedTownOfUs/Modifiers/IgnoreInvulnerabilityModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Imitator

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ImitatorCacheModifier](IntegratedTownOfUs/Modifiers/Crewmate/ImitatorCacheModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Indirect Attacker

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [IndirectAttackerModifier](IntegratedTownOfUs/Modifiers/IndirectAttackerModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Infected

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [PlaguebearerInfectedModifier](IntegratedTownOfUs/Modifiers/Neutral/PlaguebearerInfectedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Inquired

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [InquisitorInquiredModifier](IntegratedTownOfUs/Modifiers/Neutral/InquisitorInquiredModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Inquisitor Heretic

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [InquisitorHereticModifier](IntegratedTownOfUs/Modifiers/Neutral/InquisitorHerericModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Invulnerability

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [InvulnerabilityModifier](IntegratedTownOfUs/Modifiers/InvulnerabilityModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Jail Immune

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [JailSparedModifier](IntegratedTownOfUs/Modifiers/Crewmate/JailSparedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Jailed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [JailedModifier](IntegratedTownOfUs/Modifiers/Crewmate/JailedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Knighted

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [KnightedModifier](IntegratedTownOfUs/Modifiers/KnightedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Magic Mirror

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MagicMirrorModifier](IntegratedTownOfUs/Modifiers/Crewmate/MagicMirrorModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Marked

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [WarlockMarkedModifier](IntegratedTownOfUs/Modifiers/Impostor/WarlockMarkedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Mayor Reveal

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MayorRevealModifier](IntegratedTownOfUs/Modifiers/Crewmate/MayorRevealModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Mediated

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MediatedModifier](IntegratedTownOfUs/Modifiers/Crewmate/MediatedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Medic Shield

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MedicShieldModifier](IntegratedTownOfUs/Modifiers/Crewmate/MedicShieldModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Mercenary Bribed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MercenaryBribedModifier](IntegratedTownOfUs/Modifiers/Neutral/MercenaryBribedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Mercenary Guard

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MercenaryGuardModifier](IntegratedTownOfUs/Modifiers/Neutral/MercenaryGuardModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Mimic

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [GlitchMimicModifier](IntegratedTownOfUs/Modifiers/Neutral/GlitchMimicModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Misfortunate (Can Be Spooked/Tormented/Haunted)

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MisfortuneTargetModifier](IntegratedTownOfUs/Modifiers/Neutral/MisfortuneTargetModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Morph

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [MorphlingMorphModifier](IntegratedTownOfUs/Modifiers/Impostor/MorphlingMorphModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Observed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [DoomsayerObservedModifier](IntegratedTownOfUs/Modifiers/Neutral/DoomsayerObservedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Parasite Infected

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ParasiteInfectedModifier](IntegratedTownOfUs/Modifiers/Impostor/ParasiteInfectedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Plumber Venter Arrow

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [PlumberVenterModifier](IntegratedTownOfUs/Modifiers/Crewmate/PlumberVenterModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Poison

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [BootleggerPoisonModifier](IntegratedTownOfUs/Modifiers/Impostor/BootleggerPoisonModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Protected

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [GuardianAngelProtectModifier](IntegratedTownOfUs/Modifiers/Neutral/GuardianAngelProtectModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Puppeteer Controlled

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [PuppeteerControlModifier](IntegratedTownOfUs/Modifiers/Impostor/PuppeteerControlModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Retrained

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [AmbassadorRetrainedModifier](IntegratedTownOfUs/Modifiers/Impostor/AmbassadorRetrainedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Revealed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [DeputyRevealedModifier](IntegratedTownOfUs/Modifiers/Crewmate/DeputyRevealedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Revealed Impostor

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [SnitchImpostorRevealModifier](IntegratedTownOfUs/Modifiers/Crewmate/SnitchImpostorRevealModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Revealed Snitch

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [SnitchPlayerRevealModifier](IntegratedTownOfUs/Modifiers/Crewmate/SnitchPlayerRevealModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Role Revealed

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ImitatedRevealedModifier](IntegratedTownOfUs/Modifiers/Crewmate/ImitatedRevealedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Roleblocked

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [RoleblockedModifier](IntegratedTownOfUs/Modifiers/Other/RoleblockedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Satellite Arrow

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [SatelliteArrowModifier](IntegratedTownOfUs/Modifiers/Game/Universal/SatelliteArrowModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Scatter

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ScatterModifier](IntegratedTownOfUs/Modifiers/ScatterModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Scavenger Arrow

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ScavengerArrowModifier](IntegratedTownOfUs/Modifiers/Impostor/ScavengerArrowModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### SeerEvilReveal

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [SeerEvilRevealModifier](IntegratedTownOfUs/Modifiers/Crewmate/SeerEvilRevealModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### SeerGoodReveal

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [SeerGoodRevealModifier](IntegratedTownOfUs/Modifiers/Crewmate/SeerGoodRevealModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Shapeshifted

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ShapeshifterShiftModifier](IntegratedTownOfUs/Modifiers/Impostor/ShapeshifterShiftModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Snitch Arrow

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HnsSnitchArrowModifier](IntegratedTownOfUs/Modifiers/HnsCrewmate/HnsSnitchArrowModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Sonar Arrow

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [SonarArrowTargetModifier](IntegratedTownOfUs/Modifiers/Crewmate/SonarArrowTargetModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Sonar Heartbeat

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [SonarHeartbeatTargetModifier](IntegratedTownOfUs/Modifiers/Crewmate/SonarHeartbeatTargetModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Spill Effect

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [BarkeeperSpillEffectModifier](IntegratedTownOfUs/Modifiers/Crewmate/BarkeeperSpillEffectModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Sprint

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [VenererSprintModifier](IntegratedTownOfUs/Modifiers/Impostor/Venerer/VenererSprintModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Stalked

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HunterStalkedModifier](IntegratedTownOfUs/Modifiers/Crewmate/HunterStalkedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Swooped

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [PolusSwoopModifier](IntegratedTownOfUs/Modifiers/PolusImpostor/PolusSwoopModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Swooped

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [HnsChameleonSwoopModifier](IntegratedTownOfUs/Modifiers/HnsCrewmate/HnsChameleonSwoopModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Swooped

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [SwoopModifier](IntegratedTownOfUs/Modifiers/Impostor/SwoopModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Swooped

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [AmbusherConcealedModifier](IntegratedTownOfUs/Modifiers/Impostor/AmbusherConcealedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Temporarily Revived

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [TimeLordTempReviveModifier](IntegratedTownOfUs/Modifiers/Crewmate/TimeLordTempReviveModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Traitor

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [TraitorCacheModifier](IntegratedTownOfUs/Modifiers/Impostor/TraitorCacheModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Vent

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [VentModifier](IntegratedTownOfUs/Modifiers/VentModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Vested

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [SurvivorVestModifier](IntegratedTownOfUs/Modifiers/Neutral/SurvivorVestModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Watched

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [LookoutWatchedModifier](IntegratedTownOfUs/Modifiers/Crewmate/LookoutWatchedModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

## System-assigned helpers (not regular modifier slots)

### First Death Shield

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [FirstDeadShield](IntegratedTownOfUs/Modifiers/FirstDeadShield.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Possible Traitor

**Faction/category:**  · **Origin:** TOU Mira / see provenance for modifications

Runtime effect; behavior is defined in the linked implementation.

**Eligibility, settings and restrictions:** [ToBecomeTraitorModifier](IntegratedTownOfUs/Modifiers/Crewmate/ToBecomeTraitorModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

## Testing helpers (never randomly assigned)

### Test Clean

**Faction/category:** CrewmateUtility · **Origin:** TOU Mira / see provenance for modifications

Test modifier that gives you a janitor clean button

**Eligibility, settings and restrictions:** [TestCleanModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/TestCleanModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.

### Test Time Lord

**Faction/category:** CrewmateUtility · **Origin:** TOU Mira / see provenance for modifications

Test modifier that gives you Time Lord rewind ability

**Eligibility, settings and restrictions:** [TestTimeLordModifier](IntegratedTownOfUs/Modifiers/Game/Crewmate/TestTimeLordModifier.cs). Inherits faction/shared assignment controls where not overridden; full-match behavior remains unverified.


