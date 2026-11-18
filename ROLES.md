> Follow-up 1.1.0-beta.2: see [GAMEPLAY_FIX_REPORT.md](GAMEPLAY_FIX_REPORT.md). The earlier publication snapshot and test history below remain baseline evidence, not verification of a complete match for this build.

# Roles

Source-derived catalog for 1.1.0-beta. Types were confirmed in a native Unity startup fixture; registration does not prove a successful match or multiplayer behavior. Settings below name real option fields/labels; defaults and bounds remain authoritative in the linked option source.

## Category and origin index

- **MTOU, Classic / transformation, CrewmateInvestigative**: Exorcist.
- **MTOU, Classic / transformation, CrewmateKilling**: Composter, Furas.
- **MTOU, Classic / transformation, CrewmatePower**: Bandit, Necromancer.
- **MTOU, Classic / transformation, CrewmateProtective**: Doctor.
- **MTOU, Classic / transformation, CrewmateSupport**: Wladca.
- **MTOU, Classic / transformation, ImpostorPower**: Hugger, Sweets.
- **MTOU, Classic / transformation, NeutralBenign**: Dirtek.
- **MTOU, Classic / transformation, NeutralEvil**: Gambler.
- **MTOU, Classic / transformation, NeutralKilling**: Barszcz.
- **Reworked TOU Mira, Classic / transformation, CrewmateKilling**: Deputy, Hunter, Officer, Sheriff.
- **Reworked TOU Mira, Classic / transformation, CrewmatePower**: Jailor.
- **Reworked TOU Mira, Classic / transformation, CrewmateProtective**: Cleric, Medic, Mirrorcaster, Warden.
- **Reworked TOU Mira, Classic / transformation, CrewmateSupport**: Plumber.
- **Reworked TOU Mira, Classic / transformation, ImpostorPower**: Herbalist.
- **Reworked TOU Mira, Classic / transformation, NeutralAfterlife**: Spectre.
- **Reworked TOU Mira, Classic / transformation, NeutralBenign**: Fairy.
- **Reworked TOU Mira, Classic / transformation, NeutralEvil**: Executioner, Jester.
- **Reworked TOU Mira, Classic / transformation, NeutralOutlier**: Inquisitor.
- **TOU Mira, Classic / transformation, CrewmateAfterlife**: Haunter.
- **TOU Mira, Classic / transformation, CrewmateInvestigative**: Aurial, Forensic, Investigator, Lookout, Medium, Mystic, Seer, Snitch, Sonar, Spy, Trapper.
- **TOU Mira, Classic / transformation, CrewmateKilling**: Veteran, Vigilante.
- **TOU Mira, Classic / transformation, CrewmatePower**: Mayor, Monarch, Politician, Prosecutor, Swapper, Time Lord.
- **TOU Mira, Classic / transformation, CrewmateProtective**: Altruist, Oracle.
- **TOU Mira, Classic / transformation, CrewmateSupport**: Barkeeper, Engineer, Imitator, Sentry, Transporter.
- **TOU Mira, Classic / transformation, GameOutlier**: Spectator.
- **TOU Mira, Classic / transformation, ImpostorConcealing**: Eclipsal, Escapist, Grenadier, Morphling, Swooper, Venerer.
- **TOU Mira, Classic / transformation, ImpostorKilling**: Ambusher, Bomber, Parasite, Scavenger, Warlock.
- **TOU Mira, Classic / transformation, ImpostorPower**: Ambassador, Puppeteer, Spellslinger, Traitor.
- **TOU Mira, Classic / transformation, ImpostorSupport**: Blackmailer, Bootlegger, Hypnotist, Janitor, Miner, Undertaker.
- **TOU Mira, Classic / transformation, NeutralBenign**: Amnesiac, Mercenary, Survivor.
- **TOU Mira, Classic / transformation, NeutralEvil**: Doomsayer.
- **TOU Mira, Classic / transformation, NeutralKilling**: Arsonist, Glitch, Juggernaut, Medusa, Pestilence, Plaguebearer, Vampire, Werewolf.
- **TOU Mira, Classic / transformation, NeutralOutlier**: Chef.
- **TOU Mira, Hide and Seek, CrewmateHider**: Chameleon, Mystic, Snitch.
- **TOU Mira, Hide and Seek, ImpostorSeeker**: Camouflager.
- **TOU Mira, Kill Frenzy, FrenzyKiller**: Bomber, Escapist, Glitch, Grenadier, Juggernaut, Outcast, Scavenger, Vampire, Venerer, Veteran, Werewolf.
- **TOU Mira, Kill Frenzy, NeutralBenign**: FrenzyGhostRole (mode-specific variant).
- **TOU Mira, Town of Polus, Crewmate**: Engineer, Crewmate (mode-specific variant).
- **TOU Mira, Town of Polus, Impostor**: Swooper, Impostor (mode-specific variant).
- **TOU Mira, Town of Polus, Neutral**: Serial Killer.

## Exorcist

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** MTOU

Drag and drop bodies. Prayer requires min(3, all unreported bodies on the map), with at least one body, in a valid nearby group. BodyGroupRules requires pairwise spacing within 1.5 units. A successful prayer privately reveals one qualifying killer name and role at the next meeting. Only actual history entries for the analyzed victims qualify; the killer must have an Impostor or Neutral Killing role. Failed conditions do not consume Prayer.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Drag Cooldown; Prayer Cooldown; Prayer Uses; Body Detection Radius

**Implementation / limits:** [ExorcistRole](Roles/ExorcistRole.cs). Options: [source](Options/ExorcistOptions.cs). Not verified in a full multiplayer match for this release.

## Composter

**Team:** Crewmate · **Category:** CrewmateKilling · **Mode:** Classic / transformation · **Origin:** MTOU

The Composter wins with the Crewmates but can kill indirectly. Their Place Trap button creates an invisible, single-use trap at their feet. The Composter is immune to their own traps. Other players cannot see them.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Trap Cooldown; Traps Per Round; Trap Radius; Arming Delay; Composter Sees Trap Markers; Traps Work After Composter Dies

**Implementation / limits:** [ComposterRole](Roles/ComposterRole.cs). Options: [source](Options/ComposterOptions.cs). Not verified in a full multiplayer match for this release.

## Furas

**Team:** Crewmate · **Category:** CrewmateKilling · **Mode:** Classic / transformation · **Origin:** MTOU

Furas wins with the Crew. Place up to the configured number of pet spawn points, choose one, then control a walking fox for a limited time. The fox uses normal movement controls and moves at 1.25 times normal player speed by default. It cannot cross walls or use vents, ladders, platforms or doors. Attacks respect shields and Dirtek protection. By default, killing a Crewmate permanently disables all Furas abilities. Options control spawn count, placement cooldown, control duration and cooldown, speed multiplier, attack range and cooldown, initial attack delay, kills per control, visibility, marker display, Comms blocking, meeting persistence and the Crewmate penalty.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Maximum Spawn Points; Place Spawn Cooldown; Pet Control Cooldown; Pet Control Duration; Pet Speed Multiplier; Pet Kill Cooldown; Pet First Kill Delay; Pet Kill Range; Kills Per Control; Lose Ability After Killing A Crewmate; Return After Pet Kill; Keep Spawn Points After Meetings; Other Players Can See Pet; Show Own Spawn Markers; Comms Blocks Pet Control

**Implementation / limits:** [FurasRole](Roles/FurasRole.cs). Options: [source](Options/FurasOptions.cs). Not verified in a full multiplayer match for this release.

## Bandit

**Team:** Crewmate · **Category:** CrewmatePower · **Mode:** Classic / transformation · **Origin:** MTOU

Choose a living player during a round. Both roles stay unchanged through the next meeting. After it ends, you borrow their current role for one round. After the following meeting, both original roles return. If either player dies before activation, the selection is cancelled. You win with the Crewmates.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Steal Cooldown; Maximum Steals Per Game; Can Steal Crewmate Roles; Can Steal Impostor Roles; Can Steal Neutral Roles

**Implementation / limits:** [BanditRole](Roles/BanditRole.cs). Options: [source](Options/NewCrewOptions.cs). Not verified in a full multiplayer match for this release.

## Necromancer

**Team:** Crewmate · **Category:** CrewmatePower · **Mode:** Classic / transformation · **Origin:** MTOU

Control a corpse while your own body stays still, with one attack per control. Alternatively, animate a corpse to follow its killer around obstacles. Its owner remains dead. Timers, death and meetings end control. Settings govern uses, speed, reporting and Crewmate attacks.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Corpse Ability Cooldown; Control Uses Per Game; Reanimate Uses Per Game; Corpse Control Duration; Corpse Kill Cooldown; Corpse Movement Speed; Reanimation Duration; Can Kill Crewmates; Moving Corpses Can Be Reported; End Corpse Abilities On Meeting

**Implementation / limits:** [NecromancerRole](Roles/NecromancerRole.cs). Options: [source](Options/NewCrewOptions.cs). Not verified in a full multiplayer match for this release.

## Doctor

**Team:** Crewmate · **Category:** CrewmateProtective · **Mode:** Classic / transformation · **Origin:** MTOU

Mark players to give them a one-hit shield. The mark that unlocks your reveal grants no shield. Your shields vanish when you die. An early reveal disables marking until the next meeting. Only marked living Crewmates count toward your reveal. After revealing, you become protected and can no longer mark. Voting can still eject you when enabled. You win with the Crewmates.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Mark Cooldown; Required Living Crewmates Marked; Invulnerable After Reveal; Can Be Voted Out After Reveal; Marks Per Round

**Implementation / limits:** [DoctorRole](Roles/DoctorRole.cs). Options: [source](Options/NewCrewOptions.cs). Not verified in a full multiplayer match for this release.

## Wladca

**Team:** Crewmate · **Category:** CrewmateSupport · **Mode:** Classic / transformation · **Origin:** MTOU

Empower activates Wladca's selected powers for the configured duration. Each power can be switched on or off separately in TOU-Marmo role settings. The role has its own crown and Empower icons.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Empower Cooldown; Empower Duration; Empower Uses (0 = Unlimited); Gain Impostor Vision; Ignore Comms Sabotage; See Clearly During Lights

**Implementation / limits:** [WladcaRole](Roles/WladcaRole.cs). Options: [source](Options/WladcaOptions.cs). Not verified in a full multiplayer match for this release.

## Hugger

**Team:** Impostor · **Category:** ImpostorPower · **Mode:** Classic / transformation · **Origin:** MTOU

Hug kills immediately in the world, using the standard special-murder animation and a configured body-disappearance chance. In meetings, one non-Impostor hug per game requests an immediate meeting kill. One Impostor hug per game grants Double Shot while preserving Assassin. This interaction is retained unchanged by owner decision; Double/Triple Shot retirement remains unresolved.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Hug Cooldown; Body Disappear Chance; Hugger Can Vent

**Implementation / limits:** [HuggerRole](Roles/HuggerRole.cs). Options: [source](Options/HuggerOptions.cs). Not verified in a full multiplayer match for this release.

## Sweets

**Team:** Impostor · **Category:** ImpostorPower · **Mode:** Classic / transformation · **Origin:** MTOU

Give flowers to non-Impostor-aligned living players. After the configured quota, the same button becomes Pollinate. Each flower holder independently rolls the kill percentage; all flowers and the counter reset afterwards, allowing further cycles. Flower state is currently shared statically across Sweets instances: multi-Sweets lobbies need testing.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Flower Cooldown; Max Flowers; Pollinated Flower Kill Chance

**Implementation / limits:** [SweetsRole](Roles/SweetsRole.cs). Options: [source](Options/SweetsOptions.cs). Not verified in a full multiplayer match for this release.

## Dirtek

**Team:** Neutral · **Category:** NeutralBenign · **Mode:** Classic / transformation · **Origin:** MTOU

Dirtek automatically receives one random living player at the start of the game. Protection is passive, permanent and has no cooldown or activation button. The target cannot be changed. The target dies when Dirtek dies. This protective Neutral is listed under Neutral Benign.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Dirtek and the assigned protected player must both be alive at game end (SoulguardRole.DidWin). This is a Neutral survival condition.

**Important settings:** Private Meeting Chat

**Implementation / limits:** [SoulguardRole](Roles/SoulguardRole.cs). Options: [source](Options/SoulguardOptions.cs). Not verified in a full multiplayer match for this release.

## Gambler

**Team:** Neutral · **Category:** NeutralEvil · **Mode:** Classic / transformation · **Origin:** MTOU

Gambler places marks during a meeting. Marks arm when the next round begins; marked players who are already dead do not count. The first marked death outside a meeting grants one shot. A successful Cash Out kill fulfills Gambler's win condition.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Successfully kill with the earned Cash Out shot; HasWon is latched. The end-game trigger additionally requires Gambler to be alive.

**Important settings:** Marks Per Meeting; Cash Out Cooldown

**Implementation / limits:** [GamblerRole](Roles/GamblerRole.cs). Options: [source](Options/GamblerOptions.cs). Not verified in a full multiplayer match for this release.

## Barszcz

**Team:** Neutral · **Category:** NeutralKilling · **Mode:** Classic / transformation · **Origin:** MTOU

Barszcz is a Neutral Killing role. After Barszcz kills, every player receives an arrow pointing to the new body. At the same time, Barszcz trades positions with the farthest living player. The arrow remains until the body disappears or a meeting begins.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Kill Cooldown; Barszcz Can Vent; Show Body Arrow To Everyone; Swap With Farthest Player

**Implementation / limits:** [BarszczRole](Roles/BarszczRole.cs). Options: [source](Options/BarszczOptions.cs). Not verified in a full multiplayer match for this release.

## Engineer

**Team:** Crewmate · **Category:** Crewmate · **Mode:** Town of Polus · **Origin:** TOU Mira

The Engineer is a Crewmate role that can fix one sabotage per game, from anywhere on the map.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [PolusEngineerRole](IntegratedTownOfUs/Roles/TownOfPolus/Crewmate/PolusEngineerRole.cs). Not verified in a full multiplayer match for this release.

## Crewmate (mode-specific variant)

**Team:** Crewmate · **Category:** Crewmate · **Mode:** Town of Polus · **Origin:** TOU Mira

Town of Polus base Crewmate variant; uses the mode-specific Crewmate mechanics and team victory rules.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [PolusCrewmateRole](IntegratedTownOfUs/Roles/TownOfPolus/Crewmate/PolusCrewmateRole.cs). Not verified in a full multiplayer match for this release.

## Haunter

**Team:** Crewmate · **Category:** CrewmateAfterlife · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Haunter is a Crewmate Ghost who can do tasks. They will appear as a transparent player. If they finish all their tasks, all living players will see who the Impostors are. However, if an Impostor clicks them first, they will become a normal ghost. Impostors get a warning shortly before and when the Haunter finishes their tasks.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Tasks Left Before Clickable; Tasks Left Before Alerted; Reveal Neutral Roles; Can Be Clicked By

**Implementation / limits:** [HaunterRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateAfterlife/HaunterRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/HaunterOptions.cs). Not verified in a full multiplayer match for this release.

## Chameleon

**Team:** Crewmate · **Category:** CrewmateHider · **Mode:** Hide and Seek · **Origin:** TOU Mira

The Chameleon is a Crewmate that can temporarily go invisible to hide from the seeker.

**Abilities:** Turn invisible to the seeker. Cancel your swoop early, or let it finish fully to make yourself visible once again.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Swoop Uses In Total; Swoop Cooldown; Swoop Duration; Swooper Can Vent

**Implementation / limits:** [HnsChameleonRole](IntegratedTownOfUs/Roles/HideAndSeek/Hider/HnsChameleonRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/HnsCrewmate/HnsChameleonOptions.cs). Not verified in a full multiplayer match for this release.

## Mystic

**Team:** Crewmate · **Category:** CrewmateHider · **Mode:** Hide and Seek · **Origin:** TOU Mira

The Mystic is a Crewmate that gets an arrow pointing to every kill.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Dead Body Arrow Duration

**Implementation / limits:** [HnsMysticRole](IntegratedTownOfUs/Roles/HideAndSeek/Hider/HnsMysticRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/HnsCrewmate/HnsMysticOptions.cs). Not verified in a full multiplayer match for this release.

## Snitch

**Team:** Crewmate · **Category:** CrewmateHider · **Mode:** Hide and Seek · **Origin:** TOU Mira

The Snitch is a Crewmate that decreases the time remaining quicker than other players with tasks, but at the cost of outing themselves to the seeker.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Common Task Multiplier; Short Task Multiplier; Long Task Multiplier; Seeker Notify Duration

**Implementation / limits:** [HnsSnitchRole](IntegratedTownOfUs/Roles/HideAndSeek/Hider/HnsSnitchRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/HnsCrewmate/HnsSnitchOptions.cs). Not verified in a full multiplayer match for this release.

## Aurial

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Aurial is a Crewmate Investigative role that will be alerted whenever a player near them uses one of their abilities.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Radiate Color Range; Radiate Max Range; Sense Duration

**Implementation / limits:** [AurialRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/AurialRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/AurialOptions.cs). Not verified in a full multiplayer match for this release.

## Forensic

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Forensic is a Crewmate Investigative role that can inspect crime scenes and examine players to see if they were at the crime scene, flashing red if they were there.

**Abilities:** Examine players after inspecting a crime scene. You will be told if the player was at the crime scene. Crime scenes will spawn with all dead bodies. Inspect the crime scene and then examine players to discover clues. During the next meeting, you will receive a report revealing the killer's role.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Examine Cooldown; Show Forensic Reports; Time Where Forensic Will Have Role; Time Where Forensic Will Have Faction

**Implementation / limits:** [ForensicRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/ForensicRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/ForensicOptions.cs). Not verified in a full multiplayer match for this release.

## Investigator

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Investigator is a Crewmate Investigative role can see player's footprints throughout the game. Swooped players' footprints will not be visible to the Investigator.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Footprints Added Based On; Footprint Interval; Footprint Size; Footprint Duration; Anonymous Footprint; Footprint Vent Visible

**Implementation / limits:** [InvestigatorRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/InvestigatorRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/InvestigatorOptions.cs). Not verified in a full multiplayer match for this release.

## Lookout

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Lookout is a Crewmate Investigative role that can watch other players during rounds. During meetings, they will see all roles or players who interact with each watched player.

**Abilities:** Watch a player or multiple, the next meeting you will know which players interacted with the watched ones.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Watch Cooldown; Watched Player Feedback Reveals; Initial Players That Can be Watched; See Indirect Attacks; Lookout Watches Reset After Each Round; Get More Uses From Completing Tasks

**Implementation / limits:** [LookoutRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/LookoutRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/LookoutOptions.cs). Not verified in a full multiplayer match for this release.

## Medium

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Medium is a Crewmate Investigative role that can temporarily release their spirit. They can pass through walls and see ghosts, but leave their body unprotected.

**Abilities:** Project your spirit outside your body for some time to uncover clues. Once time is up, return to your mortal flesh and ghosts will disappear.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Mediate Cooldown; Mediate Duration; Spirit Speed When Mediating; Time Before Spirit is Visible to Others; Medium can Identify; Arrow Visibility; Who is Revealed

**Implementation / limits:** [MediumRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/MediumRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/MediumOptions.cs). Not verified in a full multiplayer match for this release.

## Mystic

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Mystic is a Crewmate Investigative role that gets an alert when someone dies.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Dead Body Arrow Duration; Show Death Pop-Up

**Implementation / limits:** [MysticRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/MysticRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/MysticOptions.cs). Not verified in a full multiplayer match for this release.

## Seer

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Seer is a Crewmate Investigative role that can reveal the alliance of other players.

**Abilities:** Compare the alignments of two players by using your Intuit and Gaze abilities on two different people. If they appear as enemies, you will be informed of such. Reveal the faction of a player

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Compare Players Instead of Checking Alignments; Reveal/Compare Cooldown; Max Uses of Reveal/Compare; Can Reveal/Compare More Than Once Per Round; Can Only Compare Each Player Once; Neutral Benign Show Friends To All; Neutral Evils Show Friends To All; Neutral Outliers Show Friends To All; Crewmate Killing Roles Are Red; Neutral Benign Roles Are Red; Neutral Evil Roles Are Red; Neutral Killing Roles Are Red; Neutral Outlier Roles Are Red; Traitor Swaps Colors

**Implementation / limits:** [SeerRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/SeerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/SeerOptions.cs). Not verified in a full multiplayer match for this release.

## Snitch

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Snitch is a Crewmate Investigative role that can reveal the Impostors to themselves by finishing all their tasks. Upon completing all tasks, the Impostors will be revealed to the Snitch with an arrow and their red name.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Snitch Reveals Neutral Killers; Snitch Sees Traitor; Snitch Sees Impostors In Meetings; Tasks Remaining When Revealed

**Implementation / limits:** [SnitchRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/SnitchRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/SnitchOptions.cs). Not verified in a full multiplayer match for this release.

## Sonar

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Sonar is a Crewmate Investigative role that can track other players to see their general position across the map, getting colored arrows towards all tracked players.

**Abilities:** Track a player to see where they go. You will have an arrow pointing to their location that will update periodically. It will disappear if they die, or depending on settings, the trackers will be reset after a meeting.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Track Cooldown; Initial Number Of Tracks; Arrow Update Interval; Trackers Make Sound On Death; Trackers Reset After Each Round; Get More Uses From Completing Tasks

**Implementation / limits:** [SonarRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/SonarRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/SonarOptions.cs). Not verified in a full multiplayer match for this release.

## Spy

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Spy is a Crewmate Investigative role that gains extra information on the admin table. They not only see how many people are in a room, but see who is in every room.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Who Sees Dead Bodies On Admin; Allow Portable Admin Table For; Move While Using Portable Admin; Starting Charge; Battery Charged Each Round; Battery Charged Per Task; Portable Admin Display Cooldown; Portable Admin Display Duration

**Implementation / limits:** [SpyRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/SpyRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/SpyOptions.cs). Not verified in a full multiplayer match for this release.

## Trapper

**Team:** Crewmate · **Category:** CrewmateInvestigative · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Trapper is a Crewmate Investigative role that can place traps around the map. If someone stays in it for enough time and enough players go through, they will get a list of their roles in the next meeting in random order.

**Abilities:** Places a trap. Depending on settings they may stay the entire game or reset after meetings.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Trap Cooldown; Min Amount Of Time In Trap To Register; Initial Number Of Traps; Trap Size; Traps Reset After Each Round; Get More Uses From Completing Tasks; Minimum Number Of Roles Required To Trigger Trap

**Implementation / limits:** [TrapperRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/TrapperRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/TrapperOptions.cs). Not verified in a full multiplayer match for this release.

## Deputy

**Team:** Crewmate · **Category:** CrewmateKilling · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Deputy is a Crewmate Killing role that can camp other players. Once a camped player dies, the Deputy is alerted to their death. The following meeting, the Deputy may then attempt to shoot the killer of the camped player. If successful the killer dies and if not nothing happens.

**Abilities:** Camp a player to be alerted once they die. After their death, you may attempt to shoot the killer. If your shot is successful, the killer dies, if not, nothing will happen. Shoot a player in the meeting. The player will be killed successfully if they are the killer and they are not unkillable by natural means.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Warn Killer Upon Killing Camped Target; Reveal Deputy Upon Successful Shot

**Implementation / limits:** [DeputyRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/DeputyRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/DeputyOptions.cs). Not verified in a full multiplayer match for this release.

## Hunter

**Team:** Crewmate · **Category:** CrewmateKilling · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Hunter is a Crewmate Killing role that can stalk players during the round. If a stalked player uses an ability, they can be killed by the Hunter at any point in the game, even Crew. However, if the Hunter kills a caught Crewmate in the same round they were caught, the Hunter will leave in shame at the end of the next meeting.

**Abilities:** Choose a target to stalk. You can stalk [hunterMaxStalkUsages] players. If they use any ability while stalked, they're added to your hitlist and can be killed.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Hunter Kill Cooldown; Hunter Stalk Cooldown; Hunter Stalk Duration; Initial Stalk Uses; Tasks Required For Additional Stalk Use; Stalk Triggered By; Hunter Sees Interaction Type; Hunter Kills Last Voter If Voted Out; Hunter Leaves If Killing Caught Crew Same Round; Hunter Can Report Who They've Killed

**Implementation / limits:** [HunterRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/HunterRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/HunterOptions.cs). Not verified in a full multiplayer match for this release.

## Officer

**Team:** Crewmate · **Category:** CrewmateKilling · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Officer is a Crewmate Killing role that can shoot players with limited ammo. They must load bullets, which only last one round unless they are added within 10 seconds.

**Abilities:** Load a bullet into your gun, allowing you to shoot. Shoot a player to kill them. If the player is considered "innocent," then you will put away your gun temporarily.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Shoot Cooldown; Load Cooldown; Can Self Report; Allow Shooting in First Round; Officer Can Only Shoot Players That Have Killed; Crewmate Killing Roles are Innocent; Non-Killing Neutrals are Innocent; Maximum Bullets in Total; Maximum Bullets at Once; # of Rounds to Shoot Again After Misfire

**Implementation / limits:** [OfficerRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/OfficerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/OfficerOptions.cs). Not verified in a full multiplayer match for this release.

## Sheriff

**Team:** Crewmate · **Category:** CrewmateKilling · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Sheriff is a Crewmate Killing role that can shoot a player to attempt to kill them. If Sheriff doesn't die to misfire, they will lose the ability to shoot.

**Abilities:** Shoot a player to kill them, misfiring if they aren't an Impostor or one of the other selected shootable factions

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Kill Cooldown; Can Self Report; Allow Shooting in First Round; Can Shoot Neutral Benign Roles; Can Shoot Neutral Evil Roles; Can Shoot Neutral Killing Roles; Can Shoot Neutral Outlier Roles; Misfire Kills

**Implementation / limits:** [SheriffRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/SheriffRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/SheriffOptions.cs). Not verified in a full multiplayer match for this release.

## Veteran

**Team:** Crewmate · **Category:** CrewmateKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Veteran is a Crewmate Killing role that can go on alert and kill anyone who interacts with them.

**Abilities:** When the Veteran is on alert, any player who interacts with them will be instantly killed, except Pestilence and shielded players, who will ignore the attack.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Alert Cooldown; Alert Duration; Initial Number of Alerts; Can Be Killed On Alert; Knows When Attacked In Meeting; Get More Uses From Completing Tasks

**Implementation / limits:** [VeteranRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/VeteranRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/VeteranOptions.cs). Not verified in a full multiplayer match for this release.

## Vigilante

**Team:** Crewmate · **Category:** CrewmateKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Vigilante is a Crewmate Killing role that can guess roles of players in meetings. If they guess correctly, the other player will die. If not, they will die.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Number Of Guesses; Can Guess More Than Once Per Meeting; Can Guess Neutral Benign Roles; Can Guess Neutral Evil Roles; Can Guess Neutral Killing Roles; Can Guess Neutral Outlier Roles; Can Guess Killer Modifiers; Can Guess Alliance Modifiers; Safe Shots Available

**Implementation / limits:** [VigilanteRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/VigilanteRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/VigilanteOptions.cs). Not verified in a full multiplayer match for this release.

## Jailor

**Team:** Crewmate · **Category:** CrewmatePower · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Jailor is a Crewmate Power role that can jail other players. During a meeting, the Jailor can choose to execute their jailed player. (Unless the Jailor is an Imitator)

**Abilities:** Execute the detained player. If the player is a crewmate, the Jailor will lose the ability to Jail. Jail a player. During the meeting everyone will see who is jailed. You can privately talk with your detained player using the private chatbox toggle.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Jail Cooldown; Max Number Of Executes; Jail Same Person Twice In A Row; Jailee Can Use Public Chat

**Implementation / limits:** [JailorRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/JailorRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/JailorOptions.cs). Not verified in a full multiplayer match for this release.

## Mayor

**Team:** Crewmate · **Category:** CrewmatePower · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Mayor is a Crewmate Power role that gains three votes and is revealed to all players, also changing their look in meetings.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [MayorRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/MayorRole.cs). Not verified in a full multiplayer match for this release.

## Monarch

**Team:** Crewmate · **Category:** CrewmatePower · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Monarch is a Crewmate Power role that can hand out additional votes to other players.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Knight Cooldown; Maximum Knights; Votes Per Knight; Knight Delay (Cancelable); Reveal Knighting At Meeting; Show Knighted Votes; Allow Round One Knighting; Inform Monarch If Knight Dies; Crew Knights Grant Kill Immunity

**Implementation / limits:** [MonarchRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/MonarchRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/MonarchOptions.cs). Not verified in a full multiplayer match for this release.

## Politician

**Team:** Crewmate · **Category:** CrewmatePower · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Politician is a Crewmate Power role that can reveal themselves to the crew as the Mayor, given that they have campaigned at least half of the crewmates.

**Abilities:** Give a player a ballot, which will only be useful to you if they are a Crewmate. If you reveal, and you have more than half of the crewmates campaigned (or no other crewmates remain), you will become the Mayor! Otherwise, your ability will fail.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Campaign Cooldown; Prevent Campaigning on Failed Reveal

**Implementation / limits:** [PoliticianRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/PoliticianRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/PoliticianOptions.cs). Not verified in a full multiplayer match for this release.

## Prosecutor

**Team:** Crewmate · **Category:** CrewmatePower · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Prosecutor is a Crewmate Power role that can Exile a player, applying five votes to a player of their choosing. They can also see who voted for whom, even if they're anonymous.

**Abilities:** Exile any player of your choosing, throwing five votes on them and ignoring all other votes.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** On Wrongful Prosecution; Max Prosecutions

**Implementation / limits:** [ProsecutorRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/ProsecutorRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/ProsecutorOptions.cs). Not verified in a full multiplayer match for this release.

## Swapper

**Team:** Crewmate · **Category:** CrewmatePower · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Swapper is a Crewmate Power that can swap the votes of two players in a meeting. Their meeting vote areas will be swapped visually, and the votes will be swapped. If player 1 received the most votes, and you swap them with player 2, player 2 will now be ejected instead.

**Abilities:** Select two players to swap votes for, and at the end of the meeting, they will swap spots!

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Can Call Button

**Implementation / limits:** [SwapperRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/SwapperRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/SwapperOptions.cs). Not verified in a full multiplayer match for this release.

## Time Lord

**Team:** Crewmate · **Category:** CrewmatePower · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Time Lord is a Crewmate Power role that can rewind time, forcing every player to retrace their steps for a short duration.

**Abilities:** Rewind time for everyone. Players are forced back along their recent path for the duration. Any deaths that happened within that window are reversed.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Rewind Cooldown; Rewind Duration; Rewind History; Initial Uses; Tasks Required For Additional Rewind Use; Can Use Vitals; Revive On Rewind; Undo Tasks On Rewind; Unclean Bodies On Rewind; Notify Time Lord On Revive

**Implementation / limits:** [TimeLordRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/TimeLordRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/TimeLordOptions.cs). Not verified in a full multiplayer match for this release.

## Altruist

**Team:** Crewmate · **Category:** CrewmateProtective · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Altruist is a Crewmate Protective role that can revive dead crewmates, but prevents meetings being called by themselves or the crewmates they revive.

**Abilities:** Revive a group of dead bodies near you.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Revive Type; Revive Range; Revive Duration; Revive Uses; Kill Altruist When Starting Revive; Freeze Altruist During Revive; Hide Bodies at Beginning Of Revive; Killers Alerted Before Revive; Killers Alerted After Revive

**Implementation / limits:** [AltruistRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/AltruistRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/AltruistOptions.cs). Not verified in a full multiplayer match for this release.

## Cleric

**Team:** Crewmate · **Category:** CrewmateProtective · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Cleric is a Crewmate Protective that can protect crewmates by negating their negative effects, as well as placing barriers on them to prevent interactions.

**Abilities:** Prevent a Crewmate from being interacted with. The shield will last for [BarrierCooldown] seconds. Remove all negative effects on a player. (Douse, Hack, Infect, Blackmail, Blind, Flash, and Hypnosis)

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Barrier Cooldown; Barrier Duration; Cleanse Cooldown; Protected Player Sees Barrier; Cleric Gets Attack Notification

**Implementation / limits:** [ClericRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/ClericRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/ClericOptions.cs). Not verified in a full multiplayer match for this release.

## Medic

**Team:** Crewmate · **Category:** CrewmateProtective · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Medic is a Crewmate Protective role that can give a Shield to a player.

**Abilities:** Give a Shield to a player, protecting them from being killed by others

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Show Shielded Player; Who Gets Murder Attempt Indicator; Can Give Shield Away Next Round; Max Amount of Shield Uses; Shield Breaks On Murder Attempt; Show Reports in Chat; Time Where Medic Will Have Name; Time Where Medic Will Have Color Type

**Implementation / limits:** [MedicRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/MedicRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/MedicOptions.cs). Not verified in a full multiplayer match for this release.

## Mirrorcaster

**Team:** Crewmate · **Category:** CrewmateProtective · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Mirrorcaster is a Crewmate Protective role that can cast a Magic Mirror on a player to protect them. If attacked directly, the Mirrorcaster can then unleash onto another player.

**Abilities:** Place a Mirror on a player. If the player is attacked directly, then you will be notified, and you will be able to unleash onto another player. Roles that ignore the Magic Mirror are Arsonist, Veteran, Pestilence, Bomber (if the player is bombed), and a few others. Once the Magic Mirror shatters, use its power to unleash the attack onto another player!

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Who Gets Murder Attempt Indicator; Information Received Upon Attack; Accumulate Multiple Unleashes; Max Number Of Magic Mirrors

**Implementation / limits:** [MirrorcasterRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/MirrorcasterRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/MirrorcasterOptions.cs). Not verified in a full multiplayer match for this release.

## Oracle

**Team:** Crewmate · **Category:** CrewmateProtective · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Oracle is a Crewmate Protective role that can get another player to confess (revealing their faction with [revealAccuracy]% accuracy if the Oracle dies) or can protect a player from meeting abilities.

**Abilities:** Blessing a player prevents any harm from being done to them in the meeting. Make a player confess in a meeting, giving a vision of three possible evils (including the confessor), and also reveal their faction to everyone with [revealAccuracy]% accuracy when the Oracle dies.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Confess Cooldown; Bless Cooldown; Reveal Accuracy; Neutral Benign Show Up As Evil; Neutral Evils Show Up As Evil; Neutral Killers Show Up As Evil; Neutral Outliers Show Up As Evil

**Implementation / limits:** [OracleRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/OracleRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/OracleOptions.cs). Not verified in a full multiplayer match for this release.

## Warden

**Team:** Crewmate · **Category:** CrewmateProtective · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Warden is a Crewmate Protective role that can fortify players to prevent them from being interacted with.

**Abilities:** Fortify a player to prevent them from being interacted with. If anyone tries to interact with a fortified player, the ability will not work and both the Warden and fortified player will be alerted with a purple flash.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Show Fortified Player

**Implementation / limits:** [WardenRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/WardenRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/WardenOptions.cs). Not verified in a full multiplayer match for this release.

## Barkeeper

**Team:** Crewmate · **Category:** CrewmateSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Barkeeper is a Crewmate Support role that can drink with others to roleblock them. They may also spill a drink to speed or slow down players.

**Abilities:** Drink with a player, roleblocking them for [blockTime]s. Spill one of your drinks, making anyone speed up if it's recently been spilled or make others slow down if it's old enough. This will only trigger once for each player.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Roleblock Cooldown; Minimum Roleblock Delay; Maximum Roleblock Delay; Spill Settle Delay; Time Before Spill Slows Players Down; Additional Time For Spill To Be Removed; Amount of Time Player is Affected By Spill; Spill Speed Buff Multiplier; Spill Speed Debuff Multiplier

**Implementation / limits:** [BarkeeperRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/BarkeeperRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/BarkeeperOptions.cs). Not verified in a full multiplayer match for this release.

## Engineer

**Team:** Crewmate · **Category:** CrewmateSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Engineer is a Crewmate Support role that can vent and fix sabotages remotely.

**Abilities:** It doesn't matter where you are on the map, you can use your fix button to instantly fix the active sabotage. You can do this [engiMaxFixes] times per game.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Vent Uses Per Game; Tasks Required For Additional Vent Use; Vent Cooldown; Vent Duration; Fix Uses Per Game; Tasks Required For Additional Fix Use; Fix Delay

**Implementation / limits:** [EngineerTouRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/EngineerTouRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/EngineerOptions.cs). Not verified in a full multiplayer match for this release.

## Imitator

**Team:** Crewmate · **Category:** CrewmateSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Imitator is a Crewmate Support role that can select a dead crewmate to imitate their role until they change targets. Certain roles are inaccessible if there are multiple living imitators.

**Abilities:** All crewmate roles are available besides Politician and Mayor. Roles like Jailor and Prosecutor cannot use their meeting abilities, and Vigi does not get safe shots.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Imitate Neutral Counterparts; Imitate Impostor Counterparts; Imitate Basic Crewmate; Imitator Must Be Guessed As

**Implementation / limits:** [ImitatorRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/ImitatorRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/ImitatorOptions.cs). Not verified in a full multiplayer match for this release.

## Plumber

**Team:** Crewmate · **Category:** CrewmateSupport · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

Drop a nail into a nearby vent. The vent records eligible entries and triggers the configured single-use trap threshold, then removes the trap. Task progression can replenish uses. The installed role no longer builds vent walls. Its standard Engineer-style vent access is retained.

**Abilities:** Block a vent by placing a barricade on it for the next round, preventing players from using it. Flushing the vents kicks out anyone actively in a vent, and prevents them from being used temporarily. The Plumber also gets an arrow pointing to every flushed player for one second. Drop a nail into a nearby vent. Its next visitor consumes the nail and receives one hit. Repeated hits kill at the configured threshold and respect shields.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Flush Cooldown; Plumber Hits Required to Kill; Plumber Max Trapped Vents; Plumber Trap Cooldown; Flush Duration; Get More Nails From Completing Tasks

**Implementation / limits:** [PlumberRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/PlumberRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/PlumberOptions.cs). Not verified in a full multiplayer match for this release.

## Sentry

**Team:** Crewmate · **Category:** CrewmateSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Sentry is a Crewmate Support role that can deploy cameras around the map that function like Security cameras.

**Abilities:** Deploy a camera at your current location. When it appears depends on settings. Access Security cameras from anywhere with a limited battery. Battery drains while viewing and does not recharge. Disabled by Comms sabotage.

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Placement Cooldown; Deployed Cameras Visibility; Cameras Visible After; Can Move While Placing Cameras; Instant Portable Cameras (or After Tasks); Initial Cameras; Amount Of Rounds Cameras Last; Tasks Required Per Camera; Max Cameras Placed At Once; Portable Cameras Battery; Blind Spots; Blindspot 1 Room; Blindspot 2 Room; Blindspot 3 Room; Blindspot 4 Room; Blindspot 5 Room; Blindspot 6 Room; Blindspot 7 Room; Blindspot 8 Room; Blindspot 9 Room; Blindspot 10 Room

**Implementation / limits:** [SentryRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/SentryRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/SentryOptions.cs). Not verified in a full multiplayer match for this release.

## Transporter

**Team:** Crewmate · **Category:** CrewmateSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Transporter is a Crewmate Support role that can transport two players, dead or alive, to swap their locations.

**Abilities:** Switch the positions of two players. Players can be transported out of vents. A red flash means one of the players became an invalid target, such as going on a ladder or zipline

**Win condition:** Win with the Crewmates, subject to alliance modifiers and the selected game mode.

**Important settings:** Transport Cooldown; Initial Uses; Move While Using Transport Menu (KB ONLY); Can Use Vitals; Get More Uses From Completing Tasks

**Implementation / limits:** [TransporterRole](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/TransporterRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Crewmate/TransporterOptions.cs). Not verified in a full multiplayer match for this release.

## Bomber

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Bomber is an Impostor Killing role that can drop a bomb on the map, which detonates after [detonateDelay] second(s).

**Abilities:** Place a bomb, showing the radius in which it'll kill, killing up to [maxKills] player(s)

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyBomberRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyBomberRole.cs). Not verified in a full multiplayer match for this release.

## Escapist

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Escapist is an Impostor Concealing role that can mark a location and then recall (teleport) to that location.

**Abilities:** Mark a location for later use. Recall to the marked location.

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyEscapistRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyEscapistRole.cs). Not verified in a full multiplayer match for this release.

## Glitch

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Glitch is a Neutral Killing role that wins by being the last killer alive. They can Mimic into another player, or they can hack a player.

**Abilities:** Disable a player's abilities. Mimic the appearance of another player, taking on their whole appearance.

**Win condition:** Be the last killer alive and satisfy the Glitch end-game population checks.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyGlitchRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyGlitchRole.cs). Not verified in a full multiplayer match for this release.

## Grenadier

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Grenadier is an Impostor Concealing role that can throw down a grenade that will blind all other players

**Abilities:** Throw down a grenade flashing all players in its radius.

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyGrenadierRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyGrenadierRole.cs). Not verified in a full multiplayer match for this release.

## Juggernaut

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Juggernaut is a Neutral Killing role that wins by being the last killer alive. For each kill they get, their kill cooldown gets reduced.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Be the last killer alive and satisfy its end-game population checks.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyJuggernautRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyJuggernautRole.cs). Not verified in a full multiplayer match for this release.

## Outcast

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Outcast is a basic killer, with the ability to vent and being able to see farther than any other killer.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyOutcastRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyOutcastRole.cs). Not verified in a full multiplayer match for this release.

## Scavenger

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Scavenger is an Impostor Killing role that gets new targets after every kill and when the round starts. If they kill their target, they get a reduced kill cooldown, but if they don't, their cooldown is increased significantly.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyScavengerRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyScavengerRole.cs). Not verified in a full multiplayer match for this release.

## Vampire

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Vampire is a Neutral Killing role that wins by being the last killer(s) alive. They can bite, changing others into Vampires, or kill players.

**Abilities:** Bite a player. If the bitten player is convertible, and you have not exceeded the maximum number of vampires in a game yet, you convert them into a vampire. Otherwise, they will just die.

**Win condition:** The surviving Vampire faction must be the last killer faction and meet its end-game population checks.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyVampireRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyVampireRole.cs). Not verified in a full multiplayer match for this release.

## Venerer

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Venerer is an Impostor Concealing role that can kill players to gain new abilities, preventing others from catching them! However, the ability will be used immediately as they receive it, which will stack up.

**Abilities:** Stage 1 of the abilities. You will appear as a gray bean for all players, allowing you to sneak away from kills. The Final Stage of the abilities. You will slow down players around you in a radius, as well as being fast and hidden from camo. Stage 2 of the abilities. You will gain the speed of the Flash while hidden from camo.

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyVenererRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyVenererRole.cs). Not verified in a full multiplayer match for this release.

## Veteran

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Veteran is a Crewmate Killing role that can go on alert and kill anyone who interacts with them.

**Abilities:** When the Veteran is on alert, any player who interacts with them will be instantly killed, except Pestilence and shielded players, who will ignore the attack.

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyVeteranRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyVeteranRole.cs). Not verified in a full multiplayer match for this release.

## Werewolf

**Team:** Custom · **Category:** FrenzyKiller · **Mode:** Kill Frenzy · **Origin:** TOU Mira

The Werewolf is a Neutral Killing role that wins by being the last killer alive. They can go on a rampage to gain the ability to kill.

**Abilities:** Go on a Rampage, gaining the ability to kill and gain Impostor vision. During the Rampage your kill cooldown is significantly lower.

**Win condition:** Be the last killer alive and satisfy its end-game population checks.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyWerewolfRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyWerewolfRole.cs). Not verified in a full multiplayer match for this release.

## Spectator

**Team:** Custom · **Category:** GameOutlier · **Mode:** Classic / transformation · **Origin:** TOU Mira

You don't participate in-game nor interact with players. All you can do is watch.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [SpectatorRole](IntegratedTownOfUs/Roles/Other/SpectatorRole.cs). Not verified in a full multiplayer match for this release.

## Swooper

**Team:** Impostor · **Category:** Impostor · **Mode:** Town of Polus · **Origin:** TOU Mira

The Swooper is an Impostor role that can go invisible at any time to either hide or run away from kills. However, they cannot vent.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Swoop Cooldown; Swoop Duration

**Implementation / limits:** [PolusSwooperRole](IntegratedTownOfUs/Roles/TownOfPolus/Impostor/PolusSwooperRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/PolusImpostor/PolusSwooperOptions.cs). Not verified in a full multiplayer match for this release.

## Impostor (mode-specific variant)

**Team:** Impostor · **Category:** Impostor · **Mode:** Town of Polus · **Origin:** TOU Mira

Town of Polus base Impostor variant; uses the mode-specific Impostor mechanics and team victory rules.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [PolusImpostorRole](IntegratedTownOfUs/Roles/TownOfPolus/Impostor/PolusImpostorRole.cs). Not verified in a full multiplayer match for this release.

## Eclipsal

**Team:** Impostor · **Category:** ImpostorConcealing · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Eclipsal is an Impostor Concealing role that can hinder the vision of all crewmates and neutrals alike, given that they are near the Eclipsal.

**Abilities:** Blinding players causes their fog of war to overtake their screen, only letting them see the map and prevents reporting. After a while, they will regain their vision and have vision like normal.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Blind Cooldown; Blind Duration; Blind Radius

**Implementation / limits:** [EclipsalRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/EclipsalRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/EclipsalOptions.cs). Not verified in a full multiplayer match for this release.

## Escapist

**Team:** Impostor · **Category:** ImpostorConcealing · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Escapist is an Impostor Concealing role that can mark a location and then recall (teleport) to that location.

**Abilities:** Mark a location for later use. Recall to the marked location.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Recall Uses Per Game; Recall Cooldown; Escapist Can Vent

**Implementation / limits:** [EscapistRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/EscapistRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/EscapistOptions.cs). Not verified in a full multiplayer match for this release.

## Grenadier

**Team:** Impostor · **Category:** ImpostorConcealing · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Grenadier is an Impostor Concealing role that can throw down a grenade that will blind all other players

**Abilities:** Throw down a grenade flashing all players in its radius.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Flash Uses Per Game; Flash Grenade Cooldown; Flash Grenade Duration; Flash Radius; Allow Flashing During Sabotage; Grenadier Can Vent

**Implementation / limits:** [GrenadierRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/GrenadierRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/GrenadierOptions.cs). Not verified in a full multiplayer match for this release.

## Morphling

**Team:** Impostor · **Category:** ImpostorConcealing · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Morphling is an Impostor Concealing role that can Sample a player and Morph into it's appearance.

**Abilities:** Morph into the appearance of the sampled player, which can be canceled early. Take a DNA sample of a player to morph into them later.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Samples Per Game; Morph Uses Per Round; Morph Cooldown; Morph Duration; Morphling Can Vent

**Implementation / limits:** [MorphlingRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/MorphlingRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/MorphlingOptions.cs). Not verified in a full multiplayer match for this release.

## Swooper

**Team:** Impostor · **Category:** ImpostorConcealing · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Swooper is an Impostor Concealing role that can temporarily turn invisible.

**Abilities:** Turn invisible to all players except Impostors. Cancel your swoop early, or let it finish fully to make yourself visible once again.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Swoop Uses Per Round; Swoop Cooldown; Swoop Duration; Can be Tracked while Invisible; Swooper Can Vent

**Implementation / limits:** [SwooperRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/SwooperRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/SwooperOptions.cs). Not verified in a full multiplayer match for this release.

## Venerer

**Team:** Impostor · **Category:** ImpostorConcealing · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Venerer is an Impostor Concealing role that can kill players to gain new abilities, preventing others from catching them! However, the ability will be used immediately as they receive it, which will stack up.

**Abilities:** Stage 1 of the abilities. You will appear as a gray bean for all players, allowing you to sneak away from kills. The Final Stage of the abilities. You will slow down players around you in a radius, as well as being fast and hidden from camo. Stage 2 of the abilities. You will gain the speed of the Flash while hidden from camo.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Ability Cooldown; Ability Duration; Sprint Speed; Minimum Freeze Speed; Freeze Radius

**Implementation / limits:** [VenererRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/VenererRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/VenererOptions.cs). Not verified in a full multiplayer match for this release.

## Ambusher

**Team:** Impostor · **Category:** ImpostorKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Ambusher is an Impostor Killing role that can pursue a player, getting an arrow to them. They may ambush the closest player next to them. If they manage to kill the player, they will drag their body into the shadows, teleporting back with the Ambusher.

**Abilities:** Ambush the closest player to the pursued target to kill them. Pursue a player to be able to ambush another player next to them at a later time.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Ambush Uses Per Game; Ambush Cooldown; Pursue Arrow Update Interval; Stop Pursuing Player On Ambush; Ambusher Can Vent

**Implementation / limits:** [AmbusherRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorKilling/AmbusherRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/AmbusherOptions.cs). Not verified in a full multiplayer match for this release.

## Bomber

**Team:** Impostor · **Category:** ImpostorKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Bomber is an Impostor Killing role that can drop a bomb on the map, which detonates after [detonateDelay] second(s).

**Abilities:** Place a bomb, showing the radius in which it'll kill, killing up to [maxKills] player(s)

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Bomb Uses Per Game; Detonate Delay; Detonate Radius; Max Kills In Detonation; All Impostors See Bomb; Allow Bombing in First Round; Bomber Can Vent

**Implementation / limits:** [BomberRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorKilling/BomberRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/BomberOptions.cs). Not verified in a full multiplayer match for this release.

## Parasite

**Team:** Impostor · **Category:** ImpostorKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Parasite can Overtake nearby non-impostors to take control of their movement. By default, while overtaken, the victim looks like the Parasite and cannot use abilities. You can watch them with a PiP camera and kill them at any time, or wait for the control timer to kill them.

**Abilities:** Kill your currently overtaken victim immediately. Overtake a nearby non-impostor to take control of their movement. By default, you move with WASD and control them with Arrow Keys. If "Parasite Can Move Independently" is disabled, you cannot move and control them with WASD instead.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Overtake Cooldown; Kill Cooldown; Control Duration; Save Victim If Parasite Dies; Save Victim If Meeting Called; Parasite Can Vent; Parasite Can Move Independently; Overtaken Looks Like Parasite

**Implementation / limits:** [ParasiteRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorKilling/ParasiteRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/ParasiteOptions.cs). Not verified in a full multiplayer match for this release.

## Scavenger

**Team:** Impostor · **Category:** ImpostorKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Scavenger is an Impostor Killing role that gets new targets after every kill and when the round starts. If they kill their target, they get a reduced kill cooldown, but if they don't, their cooldown is increased significantly.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Scavenge Duration; Scavenge Duration Increase Per Kill; Scavenge Kill Cooldown On Correct Kill; Kill Cooldown Multiplier On Incorrect Kill

**Implementation / limits:** [ScavengerRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorKilling/ScavengerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/ScavengerOptions.cs). Not verified in a full multiplayer match for this release.

## Warlock

**Team:** Impostor · **Category:** ImpostorKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Warlock is an Impostor Killing role that can charge up attacks to wipe out the crew quickly.

**Abilities:** Replaces your regular kill button with three stages: On Cooldown, Uncharged, and Charged. If you kill while charging, your burst will be fully reset. When it is charged, you can kill in a small burst to kill multiple players in a short time.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Time It Takes To Fully Charge; Time Multiplier Added Per Kill For Next Charge; Time It Takes To Use Full Charge

**Implementation / limits:** [WarlockRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorKilling/WarlockRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/WarlockOptions.cs). Not verified in a full multiplayer match for this release.

## Ambassador

**Team:** Impostor · **Category:** ImpostorPower · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Ambassador is an Impostor Power role that can retrain impostors into other impostor roles.

**Abilities:** Retrain yourself or other impostors into a role within their alignment. Impostor Killing roles can be turned into any non-Power roles, and Concealing/Support roles can become Concealing or Support roles.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Max Retrains Available; Retrain Requires Confirmation; Kills Needed By Ambassador Or Teammate To Retrain; Round In Which Retraining Is Possible; Rounds Needed To Retrain Again

**Implementation / limits:** [AmbassadorRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorPower/AmbassadorRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/AmbassadorOptions.cs). Not verified in a full multiplayer match for this release.

## Herbalist

**Team:** Impostor · **Category:** ImpostorPower · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Herbalist is an Impostor Power role that has many abilities tied to one cooldown, either to help Impostors or to affect what others see.

**Abilities:** Confuse a player with fungal spores to prevent them from identifying players. Expose a player to all Impostors, revealing their role but preventing them from being guessed. Protect a player temporarily as if you were a Cleric, including other impostors.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Herb Cooldown; Max Expose Uses; Max Confuse Uses; Max Protect Uses; Confuse Delay; Confuse Duration; Glamour Duration; Protect Duration; Protected Player Sees Barrier; Notify Herbalist Upon Attack

**Implementation / limits:** [HerbalistRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorPower/HerbalistRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/HerbalistOptions.cs). Not verified in a full multiplayer match for this release.

## Puppeteer

**Team:** Impostor · **Category:** ImpostorPower · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Puppeteer is an Impostor Power role that controls other players to kill for them. When a player is controlled, they must watch as they are forced to kill their fellow crewmates.

**Abilities:** Use your menu to control whoever you wish. In this state, the controlled player will kill as if they were an impostor, and not as whatever role they actually are.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Kills Required For Additional Control Use

**Implementation / limits:** [PuppeteerRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorPower/PuppeteerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/PuppeteerOptions.cs). Not verified in a full multiplayer match for this release.

## Spellslinger

**Team:** Impostor · **Category:** ImpostorPower · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Spellslinger is an Impostor Power role that can hex a player, priming them for detonation. Once all non-Impostors are hexed, they may set off a Hex Bomb after a set amount of time to win.

**Abilities:** Hex a player, priming them for detonation. Begin the countdown for the hex bomb. If you are alive when the hex bomb triggers, impostors win!

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Hex Cooldown; Max Hexes; Hex Bomb Countdown Duration; Hex Bomb Timer Decrease From Meetings

**Implementation / limits:** [SpellslingerRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorPower/SpellslingerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/SpellslingerOptions.cs). Not verified in a full multiplayer match for this release.

## Traitor

**Team:** Impostor · **Category:** ImpostorPower · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Traitor is an Impostor Power role that spawns after a meeting, in which the spawn conditions are suitable. The Traitor will always be a crewmate, and they may change to a few select roles.

**Abilities:** The Traitor can change their role to one of the provided role cards, or gamble on the random. Once they select a role, they stay as that role until they die. However, they must still be guessed as Traitor.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Minimum People Alive When Traitor Can Spawn; Traitor Won't Spawn If NK Is Alive; Disable Existing Impostor Roles; Traitor Must Be Guessed As; Traitor Becomes Assassin

**Implementation / limits:** [TraitorRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorPower/TraitorRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/TraitorOptions.cs). Not verified in a full multiplayer match for this release.

## Camouflager

**Team:** Impostor · **Category:** ImpostorSeeker · **Mode:** Hide and Seek · **Origin:** TOU Mira

The Camouflager is an Impostor that can temporarily make all players appear the same as themselves.

**Abilities:** Trigger a global camouflage to blend in.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Camo Uses In Total; Camo Cooldown; Camo Duration; Camo Disables Proximity Bar

**Implementation / limits:** [HnsCamouflagerRole](IntegratedTownOfUs/Roles/HideAndSeek/Seeker/HnsCamouflagerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/HnsImpostor/HnsCamouflagerOptions.cs). Not verified in a full multiplayer match for this release.

## Blackmailer

**Team:** Impostor · **Category:** ImpostorSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Blackmailer is an Impostor Support role that can stop a player from speaking (marked with [symbol]) in the next meeting

**Abilities:** Silence a player for the next meeting. They will be unable to speak. They also will not be able to vote until less or equal number of people are alive than the blackmailer settings allow. The blackmail will be visible to other players only if the setting is toggled

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Number Of Blackmail Uses Per Game; Blackmail Cooldown; Max Players Alive To Allow Voting; Blackmail Same Person Twice In A Row; Only Target Sees Blackmail; Blackmailer Can Kill With Teammate

**Implementation / limits:** [BlackmailerRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/BlackmailerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/BlackmailerOptions.cs). Not verified in a full multiplayer match for this release.

## Bootlegger

**Team:** Impostor · **Category:** ImpostorSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Bootlegger is an Impostor Support role that can drink with others to roleblock them. Once a player is roleblocked three times, they will die of poison.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Roleblock Cooldown; Minimum Roleblock Delay; Maximum Roleblock Delay; Poison Triggers On; Poison Delay

**Implementation / limits:** [BootleggerRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/BootleggerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/BootleggerOptions.cs). Not verified in a full multiplayer match for this release.

## Hypnotist

**Team:** Impostor · **Category:** ImpostorSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Hypnotist is an Impostor Support role that can hypnotize players. During a meeting they can release Mass Hysteria, which makes all hypnotized players (marked with [symbol]) have different visuals applied to players the following round.

**Abilities:** Hypnotize a player, causing them to see the game differently than non-hypnotized players if mass hysteria is active. Cause all hypnotized players to have different visuals applied to players on their screen the following round.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Hypnotize Cooldown; Hypnotist Can Kill With Teammate

**Implementation / limits:** [HypnotistRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/HypnotistRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/HypnotistOptions.cs). Not verified in a full multiplayer match for this release.

## Janitor

**Team:** Impostor · **Category:** ImpostorSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Janitor is an Impostor Support role that can clean dead bodies.

**Abilities:** Clean a dead body, making it disappear, and making it unreportable.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Clean Uses Per Game; Clean Cooldown; Clean Delay; Reset Kill [and] Clean Cooldowns Together; Janitor Can Kill With Teammate

**Implementation / limits:** [JanitorRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/JanitorRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/JanitorOptions.cs). Not verified in a full multiplayer match for this release.

## Miner

**Team:** Impostor · **Category:** ImpostorSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Miner is an Impostor Support role that can create vents.

**Abilities:** Place a vent where you are standing. These vents won't connect to already existing vents on the map but with each other.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Number Of Miner Vents Per Game; Mine Cooldown; Mine Visibility; Mine Delay; Miner Can Kill With Teammate

**Implementation / limits:** [MinerRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/MinerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/MinerOptions.cs). Not verified in a full multiplayer match for this release.

## Undertaker

**Team:** Impostor · **Category:** ImpostorSupport · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Undertaker is an Impostor Support role that can drag dead bodies around the map.

**Abilities:** Drag a dead body, if allowed through settings you can also take it into a vent. Drop the dragged dead body, stopping it from being dragged any further.

**Win condition:** Win with the Impostors, subject to alliance modifiers and the selected game mode.

**Important settings:** Drag Cooldown; Drag Speed; Dragging Speed Is Affected by Body Size; Undertaker Can Vent; Can Vent With Body; Undertaker Can Kill With Teammate

**Implementation / limits:** [UndertakerRole](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/UndertakerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Impostor/UndertakerOptions.cs). Not verified in a full multiplayer match for this release.

## Serial Killer

**Team:** Neutral · **Category:** Neutral · **Mode:** Town of Polus · **Origin:** TOU Mira

The Serial Killer is a Neutral role that has the sole goal of killing every player. If they do not kill the minimum amount of players needed, they will lose.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Kill Cooldown; Cooldown Reduction For Kills In A Round; Minimum Players to Kill

**Implementation / limits:** [PolusSerialKillerRole](IntegratedTownOfUs/Roles/TownOfPolus/Neutral/PolusSerialKillerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/PolusNeutral/PolusSerialKillerOptions.cs). Not verified in a full multiplayer match for this release.

## Spectre

**Team:** Neutral · **Category:** NeutralAfterlife · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Spectre is a Neutral Ghost role that wins the game by finishing their tasks before a living player has clicked on them.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Use the haunting/reveal and Spectre end-game rules; a previously confirmed Jester ejection remains a distinct recorded result.

**Important settings:** Tasks Left Before Clickable; Spectre Win

**Implementation / limits:** [SpectreRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralAfterlife/SpectreRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/SpectreOptions.cs). Not verified in a full multiplayer match for this release.

## Amnesiac

**Team:** Neutral · **Category:** NeutralBenign · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Amnesiac is a Neutral Benign role that gains access to a new role from remembering a dead body’s role. Use the role you remember to win the game.

**Abilities:** Remember the role of a dead body. If the dead body's role is a unique role, you will remember the base faction's role instead.

**Win condition:** Remember a dead player and follow the resulting role and its victory condition.

**Important settings:** Inherit Faction Modifiers; Show Arrows Pointing To Dead Bodies; Time After Death Arrow Appears

**Implementation / limits:** [AmnesiacRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralBenign/AmnesiacRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/AmnesiacOptions.cs). Not verified in a full multiplayer match for this release.

## Fairy

**Team:** Neutral · **Category:** NeutralBenign · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Fairy is a Neutral Benign that needs to protect their target (signified by [symbol]) from getting killed/ejected.

**Abilities:** Protect your target from getting killed.

**Win condition:** Protect the assigned target and follow the Fairy survival/target outcome; the configured target-death transformation can change the role and win condition.

**Important settings:** Protect Cooldown; Protect Duration; Max Number Of Protects; Get More Uses From Completing Tasks; Show Protected Player; On Target Death, Fairy Becomes; Target Knows Fairy Exists; Fairy Knows Target's Role; Odds Of Target Being Evil

**Implementation / limits:** [FairyRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralBenign/FairyRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/FairyOptions.cs). Not verified in a full multiplayer match for this release.

## Mercenary

**Team:** Neutral · **Category:** NeutralBenign · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Mercenary is a Neutral Benign role that can only win by bribing players, allowing them to gain multiple win conditions.

**Abilities:** Bribing a player allows the Mercenary to gain their win condition, given that they have gold to spare. Guarding a player allows the Mercenary to absorb an ability used on the target. This will grant them gold, for Bribing. If any bribed targets win, the Mercenary will win with them.

**Win condition:** Bribe players to acquire their associated victory conditions; follow the bribe-specific rules in the linked implementation.

**Important settings:** Guard Cooldown; Max Number Of Guards; Get More Uses From Completing Tasks; Bribe Cost; Guarding Stops Attacks; Gold Given From An Attack; Wins with Neutral Benign; Wins with Neutral Evil; Wins with Neutral Killing; Wins with Neutral Outlier

**Implementation / limits:** [MercenaryRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralBenign/MercenaryRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/MercenaryOptions.cs). Not verified in a full multiplayer match for this release.

## Survivor

**Team:** Neutral · **Category:** NeutralBenign · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Survivor is a Neutral Benign role that just needs to survive until the end of the game.

**Abilities:** Put on a Vest protecting you from attacks.

**Win condition:** Be alive when the game ends.

**Important settings:** Vest Cooldown; Vest Duration; Max Number Of Vests; Get More Uses From Completing Tasks; Survivor Scatter Mechanic Enabled; Survivor Scatter Timer

**Implementation / limits:** [SurvivorRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralBenign/SurvivorRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/SurvivorOptions.cs). Not verified in a full multiplayer match for this release.

## FrenzyGhostRole (mode-specific variant)

**Team:** Neutral · **Category:** NeutralBenign · **Mode:** Kill Frenzy · **Origin:** TOU Mira

Internal Kill Frenzy dead-player wrapper. Its visible description and victory evaluation delegate to the player role recorded before death; it is not an additional independently assigned living role.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Role-specific condition: see the description and linked WinConditionMet/DidWin implementation. No generic Neutral team victory is implied.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [FrenzyGhostRole](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyGhostRole.cs). Not verified in a full multiplayer match for this release.

## Doomsayer

**Team:** Neutral · **Category:** NeutralEvil · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Doomsayer is a Neutral Evil role that wins by guessing [guessCount] players' roles.

**Abilities:** Observe a player, gaining a hint in the next meeting what their role could be.

**Win condition:** Successfully guess the configured number of players roles.

**Important settings:** Observe Cooldown; Number Of Guesses Needed To Win; Doomsayer Can Guess Crew Investigative Roles; Doomsayer Guesses All Roles At Once; Kill Only The Last Victim; Doomsayer Can't Observe; Doomsayer Win; Hide Role On Win Notification; Doomsayer Continues The Game

**Implementation / limits:** [DoomsayerRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralEvil/DoomsayerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/DoomsayerOptions.cs). Not verified in a full multiplayer match for this release.

## Executioner

**Team:** Neutral · **Category:** NeutralEvil · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Executioner is a Neutral Evil role that wins by getting their target (signified by [symbol]) ejected in a meeting.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Have the assigned target ejected according to ExecutionerOptions. Renouncing changes the actual role to Amnesiac and its win rules; changing target does not itself win.

**Important settings:** On Target Death, Executioner Becomes; Executioner Can Button; Executioner Win; Hide Role On Win Notification

**Implementation / limits:** [ExecutionerRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralEvil/ExecutionerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/ExecutionerOptions.cs). Not verified in a full multiplayer match for this release.

## Jester

**Team:** Neutral · **Category:** NeutralEvil · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Jester is a Neutral Evil role that wins by getting themselves ejected.

**Abilities:** Poke a player to trigger an interaction, setting off a variety of roles. Use it every so often to make yourself suspicious!

**Win condition:** Qualifying vote ejection satisfies the Jester result. The configured victory mode controls immediate game end versus haunt/no immediate end. JesterExileRuntime preserves the confirmed voting result across Spectre replacement; ordinary kills, skip and tie do not qualify.

**Important settings:** Can Use Button; Can Hide In Vents; Vent Cooldown; Vent Duration; Can Poke Others; Poke Cooldown; Has Impostor Vision; Scatter Mechanic Enabled; Scatter Timer; After Win Type; Notify Others On Win

**Implementation / limits:** [JesterRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralEvil/JesterRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/JesterOptions.cs). Not verified in a full multiplayer match for this release.

## Arsonist

**Team:** Neutral · **Category:** NeutralKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Arsonist is a Neutral Killing role that wins by being the last killer alive.

**Abilities:** Douse a player in gasoline. Kill multiple doused players around you, given that they are within your radius.

**Win condition:** Douse and ignite the required remaining players; use the role-specific end-game condition.

**Important settings:** Douse Cooldown; Douses Possible at Once; Douse From Interactions; Legacy Mode (No Radius); Ignite Radius; Arsonist Can Vent; Has Impostor Vision

**Implementation / limits:** [ArsonistRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/ArsonistRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/ArsonistOptions.cs). Not verified in a full multiplayer match for this release.

## Glitch

**Team:** Neutral · **Category:** NeutralKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Glitch is a Neutral Killing role that wins by being the last killer alive. They can Mimic into another player, or they can hack a player.

**Abilities:** Disable a player's abilities. Mimic the appearance of another player, taking on their whole appearance.

**Win condition:** Be the last killer alive and satisfy the Glitch end-game population checks.

**Important settings:** Kill Cooldown; Mimic Cooldown; Mimic Duration; Move While Using Mimic Menu (KB ONLY); Hack Cooldown; Hack Duration; Glitch Can Vent

**Implementation / limits:** [GlitchRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/GlitchRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/GlitchOptions.cs). Not verified in a full multiplayer match for this release.

## Juggernaut

**Team:** Neutral · **Category:** NeutralKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Juggernaut is a Neutral Killing role that wins by being the last killer alive. For each kill they get, their kill cooldown gets reduced.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Be the last killer alive and satisfy its end-game population checks.

**Important settings:** Initial Kill Cooldown; Kill Cooldown Reduction; Juggernaut Can Vent

**Implementation / limits:** [JuggernautRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/JuggernautRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/JuggernautOptions.cs). Not verified in a full multiplayer match for this release.

## Medusa

**Team:** Neutral · **Category:** NeutralKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Medusa is a Neutral Killing role that forces its victims to stand still and slowly turn into stone. Stoned players that haven't shattered will remain throughout rounds.

**Abilities:** Petrify a player to turn them into stone, killing them and leaving a statue. Petrify anyone who tries to interact with you while gazing, unless they attempt to kill you. You will also not lunge towards others.

**Win condition:** Satisfy the Medusa neutral-killer end-game condition; petrification and surviving killers affect its population checks.

**Important settings:** Petrify Cooldown; Time For Victim To Become Stoned; Time Before Stone Shatters; Allow Stone Gazing; Stone Gaze Cooldown; Stone Gaze Duration; Stone Gaze Uses; Medusa Can Vent

**Implementation / limits:** [MedusaRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/MedusaRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/MedusaOptions.cs). Not verified in a full multiplayer match for this release.

## Pestilence

**Team:** Neutral · **Category:** NeutralKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Pestilence is a Neutral Killing role that can kill and is invincible to everything but being exiled or guessing incorrectly. They win by being the last killer alive.

**Abilities:** As described above; ability and lifecycle code is linked below.

**Win condition:** Use the Pestilence neutral-killer victory condition.

**Important settings:** Role amount/chance and shared game-mode settings; no separate option group resolved.

**Implementation / limits:** [PestilenceRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/PestilenceRole.cs). Not verified in a full multiplayer match for this release.

## Plaguebearer

**Team:** Neutral · **Category:** NeutralKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Plaguebearer is a Neutral Killing role that needs to infect all other players to turn into the Pestilence.

**Abilities:** Infect a player, causing them to be infected. When an infected player or dead body interacts or gets interacted with, the infection will spread to the interacted/interacting player.

**Win condition:** Spread infection to transform into Pestilence; the transformed role determines victory.

**Important settings:** Instant Pestilence Chance; Infect Cooldown; Announce Pestilence Transformation; Pestilence Kill Cooldown; Pestilence Can Vent

**Implementation / limits:** [PlaguebearerRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/PlaguebearerRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/PlaguebearerOptions.cs). Not verified in a full multiplayer match for this release.

## Vampire

**Team:** Neutral · **Category:** NeutralKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Vampire is a Neutral Killing role that wins by being the last killer(s) alive. They can bite, changing others into Vampires, or kill players.

**Abilities:** Bite a player. If the bitten player is convertible, and you have not exceeded the maximum number of vampires in a game yet, you convert them into a vampire. Otherwise, they will just die.

**Win condition:** The surviving Vampire faction must be the last killer faction and meet its end-game population checks.

**Important settings:** Bite Cooldown; Max Number Of Vampires Per Game; Vampires Have Impostor Vision; New Vampires Can Assassinate; Can Convert Neutral Benign Roles; Can Convert Neutral Evil Roles; Can Convert Neutral Outlier Roles; Can Convert Lovers; New Vampires Can Convert; Vampires Can Vent

**Implementation / limits:** [VampireRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/VampireRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/VampireOptions.cs). Not verified in a full multiplayer match for this release.

## Werewolf

**Team:** Neutral · **Category:** NeutralKilling · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Werewolf is a Neutral Killing role that wins by being the last killer alive. They can go on a rampage to gain the ability to kill.

**Abilities:** Go on a Rampage, gaining the ability to kill and gain Impostor vision. During the Rampage your kill cooldown is significantly lower.

**Win condition:** Be the last killer alive and satisfy its end-game population checks.

**Important settings:** Rampage Cooldown; Rampage Duration; Rampage Kill Cooldown; Werewolf Can Vent When Rampaged

**Implementation / limits:** [WerewolfRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/WerewolfRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/WerewolfOptions.cs). Not verified in a full multiplayer match for this release.

## Chef

**Team:** Neutral · **Category:** NeutralOutlier · **Mode:** Classic / transformation · **Origin:** TOU Mira

The Chef is a Neutral Outlier role that wins with any other faction as long as they have served enough dead bodies to the crew.

**Abilities:** Cook a dead body and save it for later use. Serve the oldest meal you have on hand to another empty stomach.

**Win condition:** Serve the required number of dead bodies; can share another factions victory when that requirement is met.

**Important settings:** Cook Cooldown; Serve Cooldown; Reset Cook [and] Serve Cooldowns Together; Amount Of Servings Needed; Duration of Side Effects; Show Arrows Pointing To Dead Bodies; Dead Body Arrow Delay; Dead Body Arrow Duration; Hide Role On Win Notification

**Implementation / limits:** [ChefRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralOutlier/ChefRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/ChefOptions.cs). Not verified in a full multiplayer match for this release.

## Inquisitor

**Team:** Neutral · **Category:** NeutralOutlier · **Mode:** Classic / transformation · **Origin:** Reworked TOU Mira

The Inquisitor is a Neutral Outlier role that wins if their targets (Heretics) die. The only information provided is their roles, and it's up to the Inquisitor to identify those players (marked with [symbol] to the dead) and get them killed by any means necessary.

**Abilities:** Inquire a player, which will tell you if they are one of your targets within the meeting. Vanquish a player to kill them. If they are a heretic, you will be told, and you can continue vanquishing. However, if the victim isn't a heretic, you will lose the ability to vanquish for the rest of the game.

**Win condition:** Have all assigned Heretic targets die, by any allowed means.

**Important settings:** Vanquish Cooldown; Allow Vanquish in First Round; Inquisitor Continues Game In Final 3; Inquisitor Can't Inquire; Inquire Cooldown; Max Number Of Inquiries; Amount Of Heretics Needed; Hide Role On Win Notification

**Implementation / limits:** [InquisitorRole](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralOutlier/InquisitorRole.cs). Options: [source](IntegratedTownOfUs/Options/Roles/Neutral/InquisitorOptions.cs). Not verified in a full multiplayer match for this release.

## Vanilla registrations

The startup registry also contains these base-game roles; their presence is not a promise that every vanilla role is enabled in every MTOU mode: Crewmate, Impostor, Scientist, Guardian Angel, Shapeshifter, Noisemaker, Phantom, Tracker, Viper, Detective, Influencer.

