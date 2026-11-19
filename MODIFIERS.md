# Modifiers

All 51 assignable variants are listed below. Some are limited to specific roles or host settings. Double Shot and Triple Shot remain enabled in this beta; see [known issues](KNOWN_ISSUES.md).

## Alliance

| Modifier | Effect | Origin |
|---|---|---|
| [Lover](IntegratedTownOfUs/Modifiers/Game/Alliance/LoverModifier.cs) | Chat with your partner and survive together into the final three. | ToU |

## Assailant Utility

| Modifier | Effect | Origin |
|---|---|---|
| [Assassin](IntegratedTownOfUs/Modifiers/Game/Assailant/AssassinModifier.cs) | Guess roles at meetings to kill targets. A wrong guess can kill you. | ToU |
| [Double Shot](IntegratedTownOfUs/Modifiers/Game/Assailant/DoubleShotModifier.cs) | Survive your first wrong meeting guess; the next kills you. | ToU |
| [Overclocker](IntegratedTownOfUs/Modifiers/Game/Assailant/OverclockerModifier.cs) | Temporarily shorten cooldowns, then suffer longer ones before they return to normal. | ToU |
| [Triple Shot](Modifiers/TripleShotModifier.cs) | Survive two wrong meeting guesses; the third kills you. Requires guessing and cannot combine with Double Shot. | MTOU |

## Crewmate Alliance

| Modifier | Effect | Origin |
|---|---|---|
| [Crewpostor](IntegratedTownOfUs/Modifiers/Game/Alliance/CrewpostorModifier.cs) | Work for the Impostors. Become Traitor if all Impostors die. | ToU |
| [Egotist](IntegratedTownOfUs/Modifiers/Game/Alliance/EgotistModifier.cs) | Win only when Crewmates lose, even if you are dead. | ToU |

## Crewmate Passive

| Modifier | Effect | Origin |
|---|---|---|
| [Multitasker](IntegratedTownOfUs/Modifiers/Game/Crewmate/MultitaskerModifier.cs) | See through open menus. | ToU |
| [Taskmaster](IntegratedTownOfUs/Modifiers/Game/Crewmate/TaskmasterModifier.cs) | Automatically complete one task at the start of each round. | ToU |

## Crewmate Postmortem

| Modifier | Effect | Origin |
|---|---|---|
| [Aftermath](IntegratedTownOfUs/Modifiers/Game/Crewmate/AftermathModifier.cs) | Your killer is forced to use abilities on your body or themselves. | ToU |
| [Bait](IntegratedTownOfUs/Modifiers/Game/Crewmate/BaitModifier.cs) | Your killer automatically reports your body. | ToU |
| [Celebrity](IntegratedTownOfUs/Modifiers/Game/Crewmate/CelebrityModifier.cs) | Your death location and killer's role are announced at the meeting. | ToU |
| [Diseased](IntegratedTownOfUs/Modifiers/Game/Crewmate/DiseasedModifier.cs) | Your killer receives a longer kill cooldown. | ToU |
| [Frosty](IntegratedTownOfUs/Modifiers/Game/Crewmate/FrostyModifier.cs) | Your killer is slowed after killing you. | ToU |
| [Noisemaker](IntegratedTownOfUs/Modifiers/Game/Crewmate/NoisemakerModifier.cs) | Your death creates a body-location alert for everyone. | ToU |
| [Rotting](IntegratedTownOfUs/Modifiers/Game/Crewmate/RottingModifier.cs) | Your body disappears after a delay and can no longer be reported. | ToU |

## Crewmate Utility

| Modifier | Effect | Origin |
|---|---|---|
| [Investigator](IntegratedTownOfUs/Modifiers/Game/Crewmate/InvestigatorModifier.cs) | See player footprints, except those hidden by invisibility. | ToU |
| [Operative](IntegratedTownOfUs/Modifiers/Game/Crewmate/OperativeModifier.cs) | Use portable Security with limited battery. | ToU |
| [Scientist](IntegratedTownOfUs/Modifiers/Game/Crewmate/ScientistModifier.cs) | Use portable Vitals with limited battery. | ToU |
| [Spy](IntegratedTownOfUs/Modifiers/Game/Crewmate/SpyModifier.cs) | See player colors on Admin. | ToU |

## Crewmate Visibility

| Modifier | Effect | Origin |
|---|---|---|
| [Scout](IntegratedTownOfUs/Modifiers/Game/Crewmate/ScoutModifier.cs) | See farther, but lose that advantage during Lights sabotage. | ToU |
| [Torch](IntegratedTownOfUs/Modifiers/Game/Crewmate/TorchModifier.cs) | Lights sabotage does not reduce your vision. | ToU |

## Hider Passive

| Modifier | Effect | Origin |
|---|---|---|
| [Multitasker](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsMultitaskerModifier.cs) | See through open menus. | ToU |
| [Oblivious](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsObliviousModifier.cs) | Receive no warning when the seeker is close. | ToU |

## Hider Postmortem

| Modifier | Effect | Origin |
|---|---|---|
| [Frosty](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsFrostyModifier.cs) | Your killer is slowed after killing you. | ToU |
| [Transporter](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsTransporterModifier.cs) | On death, swap the seeker with a random hider. | ToU |

## Hider Visibility

| Modifier | Effect | Origin |
|---|---|---|
| [Giant](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsGiantModifier.cs) | Become larger and easier for the seeker to spot. | ToU |
| [Mini](IntegratedTownOfUs/Modifiers/HnsGame/Crewmate/HnsMiniModifier.cs) | Become smaller and easier to hide. | ToU |

## Impostor Passive

| Modifier | Effect | Origin |
|---|---|---|
| [Circumvent](IntegratedTownOfUs/Modifiers/Game/Impostor/CircumventModifier.cs) | Your vent use is restricted or disabled by settings. | ToU |
| [Deadly Quota](IntegratedTownOfUs/Modifiers/Game/Impostor/DeadlyQuotaModifier.cs) | Complete your assigned kill quota or lose. | ToU |
| [Saboteur](IntegratedTownOfUs/Modifiers/Game/Impostor/SaboteurModifier.cs) | Sabotage has a shorter cooldown. | ToU |
| [Simp](Modifiers/SimpModifier.cs) | Keep your random target from being voted out. Kill cooldown is 25% shorter until their ejection, then longer. | MTOU |
| [Underdog](IntegratedTownOfUs/Modifiers/Game/Impostor/UnderdogModifier.cs) | Kill faster when you are the last Impostor. | ToU |

## Impostor Postmortem

| Modifier | Effect | Origin |
|---|---|---|
| [Telepath](IntegratedTownOfUs/Modifiers/Game/Impostor/TelepathModifier.cs) | Learn when teammates kill; settings can also reveal kill locations and teammate deaths. | ToU |

## Impostor Utility

| Modifier | Effect | Origin |
|---|---|---|
| [Disperser](IntegratedTownOfUs/Modifiers/Game/Impostor/DisperserModifier.cs) | Send players to random vents. Immovable players stay put; cannot combine with other button modifiers. | ToU |

## Universal Passive

| Modifier | Effect | Origin |
|---|---|---|
| [Balloon](Modifiers/BalloonModifier.cs) | Players who target you get a visible balloon. | MTOU |
| [Big Brain](Modifiers/BigBrainModifier.cs) | If you survive, learn which roles targeted you at the next meeting. No names or protection. | MTOU |
| [Drunk](IntegratedTownOfUs/Modifiers/Game/Universal/DrunkModifier.cs) | Your movement controls are reversed, but normal roleblocks do not affect you. | ToU |
| [Immovable](IntegratedTownOfUs/Modifiers/Game/Universal/ImmovableModifier.cs) | Abilities and meetings cannot relocate you. | ToU |
| [Kemada](Modifiers/KemadaModifier.cs) | Your vote goes to a random other living player. | MTOU |
| [One More Chance](Modifiers/OneMoreChanceModifier.cs) | Ignore one wrong-kill penalty as Sheriff, Officer, Jailor or Furas. The victim still dies. | MTOU |
| [Sixth Sense](IntegratedTownOfUs/Modifiers/Game/Universal/SixthSenseModifier.cs) | Know when someone uses an ability on you. | ToU |
| [Sleuth](IntegratedTownOfUs/Modifiers/Game/Universal/SleuthModifier.cs) | Learn the roles of bodies you report. | ToU |
| [Tiebreaker](IntegratedTownOfUs/Modifiers/Game/Universal/TiebreakerModifier.cs) | Your vote breaks a tied result. | ToU |

## Universal Utility

| Modifier | Effect | Origin |
|---|---|---|
| [Button Barry](IntegratedTownOfUs/Modifiers/Game/Universal/ButtonBarryModifier.cs) | Call an emergency meeting from anywhere. | ToU |
| [Radar](IntegratedTownOfUs/Modifiers/Game/Universal/RadarModifier.cs) | An arrow points to the nearest player. | ToU |
| [Satellite](IntegratedTownOfUs/Modifiers/Game/Universal/SatelliteModifier.cs) | Locate bodies on the map with limited uses. | ToU |

## Universal Visibility

| Modifier | Effect | Origin |
|---|---|---|
| [Flash](IntegratedTownOfUs/Modifiers/Game/Universal/FlashModifier.cs) | Move faster. | ToU |
| [Giant](IntegratedTownOfUs/Modifiers/Game/Universal/GiantModifier.cs) | Become larger and slower. | ToU |
| [Mini](IntegratedTownOfUs/Modifiers/Game/Universal/MiniModifier.cs) | Become smaller and faster. | ToU |
| [Shy](IntegratedTownOfUs/Modifiers/Game/Universal/ShyModifier.cs) | Become transparent while standing still. | ToU |

## Availability

- **One More Chance:** Sheriff, Officer, Jailor and Furas only.
- **Simp:** Impostors only.
- **Triple Shot:** Impostors/Neutrals with guessing; cannot combine with Double Shot.
- **Big Brain:** team availability is configurable.
- Shields, role marks and temporary status effects are assigned by abilities; they are not random lobby modifiers.

Chance, count and role-specific limits are configured in the lobby.
