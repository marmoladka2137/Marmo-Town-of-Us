# Roles

Short descriptions of the 111 registered role variants, grouped by mode and category. Lobby settings and alliance modifiers can change the rules. [Known issues](KNOWN_ISSUES.md) apply; registration is not proof of live multiplayer testing.

## Classic — Crewmate Investigative

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Exorcist](Roles/ExorcistRole.cs) | Gather three bodies, or all available if fewer. Pray to learn an eligible killer's name and role at the next meeting. | Crew victory. | MTOU |
| [Aurial](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/AurialRole.cs) | Sense nearby ability use. Very close use can reveal the player color. | Crew victory. | TOU Mira |
| [Forensic](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/ForensicRole.cs) | Inspect crime scenes and check players for evidence they were there. | Crew victory. | TOU Mira |
| [Investigator](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/InvestigatorRole.cs) | Follow player footprints. Invisible players leave no visible trail. | Crew victory. | TOU Mira |
| [Lookout](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/LookoutRole.cs) | Watch players, then learn who or which roles interacted with them at the meeting. | Crew victory. | TOU Mira |
| [Medium](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/MediumRole.cs) | Leave your body to move through walls and see ghosts. Your unattended body can be killed. | Crew victory. | TOU Mira |
| [Mystic](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/MysticRole.cs) | Receive death alerts and arrows toward bodies. | Crew victory. | TOU Mira |
| [Seer](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/SeerRole.cs) | Check a player to reveal their alliance. | Crew victory. | TOU Mira |
| [Snitch](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/SnitchRole.cs) | Finish your tasks to reveal Impostors; approaching completion also exposes you to them. | Crew victory. | TOU Mira |
| [Sonar](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/SonarRole.cs) | Track players with directional arrows. | Crew victory. | TOU Mira |
| [Spy](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/SpyRole.cs) | See player colors on Admin instead of only room counts. | Crew victory. | TOU Mira |
| [Trapper](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateInvestigative/TrapperRole.cs) | Place traps to learn the roles of players who passed through at the next meeting. | Crew victory. | TOU Mira |

## Classic — Crewmate Killing

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Composter](Roles/ComposterRole.cs) | Place invisible, single-use traps. The first other player to enter dies; you are immune. | Crew victory. | MTOU |
| [Furas](Roles/FurasRole.cs) | Set spawn points and control your pet to attack. Your body stays vulnerable; killing Crew may cost your abilities. | Crew victory. | MTOU |
| [Deputy](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/DeputyRole.cs) | Camp a player. If they die, try to shoot their killer at the next meeting; a wrong attempt kills nobody. | Crew victory. | ToU, reworked |
| [Hunter](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/HunterRole.cs) | Stalk a player. If they use an ability, you may kill them. Killing a caught Crewmate that round makes you leave after the meeting. | Crew victory. | ToU, reworked |
| [Officer](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/OfficerRole.cs) | Load limited ammunition and shoot suspects. Misfires carry a penalty. | Crew victory. | ToU, reworked |
| [Sheriff](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/SheriffRole.cs) | Shoot suspected evils. Shooting a Crewmate can kill you or cost your gun, depending on settings. | Crew victory. | ToU, reworked |
| [Veteran](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/VeteranRole.cs) | Go on alert to kill anyone who interacts with you. | Crew victory. | TOU Mira |
| [Vigilante](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateKilling/VigilanteRole.cs) | Guess roles at meetings. A correct guess kills the target; a wrong guess kills you. | Crew victory. | TOU Mira |

## Classic — Crewmate Power

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Bandit](Roles/BanditRole.cs) | Choose a target. Borrow their role after the next meeting, then return it after the following meeting. You stay Crew. | Crew victory. | MTOU |
| [Necromancer](Roles/NecromancerRole.cs) | Control a corpse for one attack, or send it toward its killer. Your body stays still; the dead player remains dead. | Crew victory. | MTOU |
| [Jailor](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/JailorRole.cs) | Jail a player, question them privately and choose whether to execute them at the meeting. | Crew victory. | ToU, reworked |
| [Mayor](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/MayorRole.cs) | Reveal yourself to gain extra votes and show your role to everyone. | Crew victory. | TOU Mira |
| [Monarch](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/MonarchRole.cs) | Knight players to give them extra meeting votes. | Crew victory. | TOU Mira |
| [Politician](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/PoliticianRole.cs) | Campaign to half the Crewmates, then reveal as Mayor. | Crew victory. | TOU Mira |
| [Prosecutor](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/ProsecutorRole.cs) | Force an ejection with extra votes and see who voted for whom. | Crew victory. | TOU Mira |
| [Swapper](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/SwapperRole.cs) | Swap the votes received by two players at a meeting. | Crew victory. | TOU Mira |
| [Time Lord](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmatePower/TimeLordRole.cs) | Rewind recent movement. The configured rewind risk can cause a random death. | Crew victory. | TOU Mira |

## Classic — Crewmate Protective

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Doctor](Roles/DoctorRole.cs) | Shield players and mark living Crewmates to unlock Reveal. Revealing too early disables marking for one round. | Crew victory. | MTOU |
| [Altruist](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/AltruistRole.cs) | Revive dead Crewmates. You and revived players cannot call emergency meetings. | Crew victory. | TOU Mira |
| [Cleric](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/ClericRole.cs) | Cleanse harmful effects or place barriers that block interactions. | Crew victory. | ToU, reworked |
| [Medic](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/MedicRole.cs) | Give a player a protective shield. | Crew victory. | ToU, reworked |
| [Mirrorcaster](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/MirrorcasterRole.cs) | Protect a player with a mirror. A blocked direct attack lets you unleash against another player. | Crew victory. | ToU, reworked |
| [Oracle](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/OracleRole.cs) | Ask players to confess or protect them from meeting abilities. Your death can reveal a confession. | Crew victory. | TOU Mira |
| [Warden](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateProtective/WardenRole.cs) | Fortify players to block interactions outside meetings. | Crew victory. | ToU, reworked |

## Classic — Crewmate Support

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Wladca](Roles/WladcaRole.cs) | Empower temporarily improves vision and resistance to Lights and Comms, as enabled in lobby settings. | Crew victory. | MTOU |
| [Barkeeper](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/BarkeeperRole.cs) | Share drinks to roleblock players; spilled drinks can change movement speed. | Crew victory. | TOU Mira |
| [Engineer](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/EngineerTouRole.cs) | Fix sabotages remotely and use vents. | Crew victory. | TOU Mira |
| [Imitator](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/ImitatorRole.cs) | Copy a dead Crewmate role. Some roles are restricted when several Imitators are alive. | Crew victory. | TOU Mira |
| [Plumber](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/PlumberRole.cs) | Drop nails into a nearby vent to trap eligible vent users. The trap disappears after triggering. | Crew victory. | ToU, reworked |
| [Sentry](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/SentryRole.cs) | Place cameras and watch them like Security. | Crew victory. | TOU Mira |
| [Transporter](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateSupport/TransporterRole.cs) | Swap the positions of two players, alive or dead. | Crew victory. | TOU Mira |

## Classic — Impostor Power

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Hugger](Roles/HuggerRole.cs) | Hug to kill instantly. At meetings, once each per game: kill a non-Impostor or give an Impostor Double Shot. | Impostor victory. | MTOU |
| [Sweets](Roles/SweetsRole.cs) | Give flowers to non-Impostors, then Pollinate for a chance to kill each recipient. Flowers reset so you can repeat. | Impostor victory. | MTOU |
| [Ambassador](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorPower/AmbassadorRole.cs) | Retrain Impostors into other allowed roles. | Impostor victory. | TOU Mira |
| [Herbalist](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorPower/HerbalistRole.cs) | Use plants to expose roles, confuse players or protect a target. | Impostor victory. | ToU, reworked |
| [Puppeteer](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorPower/PuppeteerRole.cs) | Control another player and make them attack. | Impostor victory. | TOU Mira |
| [Spellslinger](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorPower/SpellslingerRole.cs) | Hex players, then trigger a Hex Bomb when the remaining targets are marked. | Impostor victory. | TOU Mira |
| [Traitor](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorPower/TraitorRole.cs) | Join the Impostors when the original Impostors are gone. | Impostor victory. | TOU Mira |

## Classic — Neutral Benign

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Dirtek](Roles/SoulguardRole.cs) | An assigned player cannot be killed while you live. Your death kills them; you cannot be protected. | You and your target survive. | MTOU |
| [Amnesiac](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralBenign/AmnesiacRole.cs) | Remember a dead player to take their role and win condition. | Follow your new role. | TOU Mira |
| [Fairy](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralBenign/FairyRole.cs) | Protect your assigned target and share their victory. | Your target wins. | ToU, reworked |
| [Mercenary](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralBenign/MercenaryRole.cs) | Bribe players to gain access to their win conditions. | Win through a bribed player. | TOU Mira |
| [Survivor](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralBenign/SurvivorRole.cs) | Stay alive; use vests to survive attacks. | Survive. | TOU Mira |

## Classic — Neutral Evil

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Gambler](Roles/GamblerRole.cs) | Mark players at meetings. The first active marked death next round earns a Cash Out shot. Land it and survive to win. | Land Cash Out and survive. | MTOU |
| [Doomsayer](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralEvil/DoomsayerRole.cs) | Guess enough player roles correctly to win. | Reach the correct-guess quota. | TOU Mira |
| [Executioner](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralEvil/ExecutionerRole.cs) | Get your target voted out. If enabled, change to a random Crewmate target or become Amnesiac once in the first round. | Have your target voted out. | ToU, reworked |
| [Jester](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralEvil/JesterRole.cs) | Get voted out to win. A normal kill does not count. | Get voted out. | ToU, reworked |

## Classic — Neutral Killing

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Barszcz](Roles/BarszczRole.cs) | After a kill, swap with the farthest living player and reveal the body with an arrow to everyone. | Be the last killer alive. | MTOU |
| [Arsonist](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/ArsonistRole.cs) | Douse players, then ignite your marked targets. | Ignite the required survivors. | TOU Mira |
| [Glitch](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/GlitchRole.cs) | Hack players to block abilities, or mimic their appearance. | Be the last killer alive. | TOU Mira |
| [Juggernaut](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/JuggernautRole.cs) | Each kill reduces your kill cooldown. | Be the last killer alive. | TOU Mira |
| [Medusa](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/MedusaRole.cs) | Petrify attackers and hide their bodies as statues. | Be the last killer alive. | TOU Mira |
| [Pestilence](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/PestilenceRole.cs) | Kill as the transformed Plaguebearer. Meeting-guess protection depends on lobby settings. | Be the last killer alive. | TOU Mira |
| [Plaguebearer](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/PlaguebearerRole.cs) | Infect everyone to transform into Pestilence. | Transform, then win as Pestilence. | TOU Mira |
| [Vampire](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/VampireRole.cs) | Kill or convert eligible players to build the Vampire faction. | Vampires are the last killers. | TOU Mira |
| [Werewolf](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralKilling/WerewolfRole.cs) | Rampage to gain the ability to kill. | Be the last killer alive. | TOU Mira |

## Town of Polus — Crewmate

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Engineer](IntegratedTownOfUs/Roles/TownOfPolus/Crewmate/PolusEngineerRole.cs) | Fix one sabotage per game from anywhere. | Crew victory. | TOU Mira |
| [Crewmate (mode-specific variant)](IntegratedTownOfUs/Roles/TownOfPolus/Crewmate/PolusCrewmateRole.cs) | Complete tasks and vote out Impostors. | Crew victory. | TOU Mira |

## Classic — Crewmate Afterlife

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Haunter](IntegratedTownOfUs/Roles/Classic/Crewmate/CrewmateAfterlife/HaunterRole.cs) | Finish your ghost tasks to reveal Impostors to everyone. An Impostor can stop you by catching you first. | Crew victory. | TOU Mira |

## Hide and Seek — Crewmate Hider

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Chameleon](IntegratedTownOfUs/Roles/HideAndSeek/Hider/HnsChameleonRole.cs) | Turn invisible briefly to hide from the seeker. | Crew victory. | TOU Mira |
| [Mystic](IntegratedTownOfUs/Roles/HideAndSeek/Hider/HnsMysticRole.cs) | An arrow points to each new kill. | Crew victory. | TOU Mira |
| [Snitch](IntegratedTownOfUs/Roles/HideAndSeek/Hider/HnsSnitchRole.cs) | Tasks reduce the timer faster, but reveal your location to the seeker. | Crew victory. | TOU Mira |

## Kill Frenzy — Frenzy Killer

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Bomber](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyBomberRole.cs) | Place bombs that kill players in range after a delay. | Your role's team is last alive. | TOU Mira |
| [Escapist](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyEscapistRole.cs) | Mark a location, then teleport back to it. | Your role's team is last alive. | TOU Mira |
| [Glitch](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyGlitchRole.cs) | Hack players to block abilities, or mimic their appearance. | Your role's team is last alive. | TOU Mira |
| [Grenadier](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyGrenadierRole.cs) | Throw a flashbang to blind nearby players. | Your role's team is last alive. | TOU Mira |
| [Juggernaut](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyJuggernautRole.cs) | Each kill reduces your kill cooldown. | Your role's team is last alive. | TOU Mira |
| [Outcast](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyOutcastRole.cs) | Use vents and extended vision to hunt other killers. | Your role's team is last alive. | TOU Mira |
| [Scavenger](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyScavengerRole.cs) | Kill assigned targets for a shorter cooldown. Killing others increases it. | Your role's team is last alive. | TOU Mira |
| [Vampire](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyVampireRole.cs) | Kill or convert eligible players to build the Vampire faction. | Your role's team is last alive. | TOU Mira |
| [Venerer](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyVenererRole.cs) | Kills upgrade your power: camouflage, sprint, then freeze nearby players. | Your role's team is last alive. | TOU Mira |
| [Veteran](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyVeteranRole.cs) | Go on alert to kill anyone who interacts with you. | Your role's team is last alive. | TOU Mira |
| [Werewolf](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyWerewolfRole.cs) | Rampage to gain the ability to kill. | Your role's team is last alive. | TOU Mira |

## Classic — Game Outlier

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Spectator](IntegratedTownOfUs/Roles/Other/SpectatorRole.cs) | Watch the match without taking part. | Spectate. | TOU Mira |

## Town of Polus — Impostor

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Swooper](IntegratedTownOfUs/Roles/TownOfPolus/Impostor/PolusSwooperRole.cs) | Turn invisible briefly to approach or escape players. | Impostor victory. | TOU Mira |
| [Impostor (mode-specific variant)](IntegratedTownOfUs/Roles/TownOfPolus/Impostor/PolusImpostorRole.cs) | Kill Crewmates and use sabotage to help the Impostors win. | Impostor victory. | TOU Mira |

## Classic — Impostor Concealing

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Eclipsal](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/EclipsalRole.cs) | Reduce other players' vision. | Impostor victory. | TOU Mira |
| [Escapist](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/EscapistRole.cs) | Mark a location, then teleport back to it. | Impostor victory. | TOU Mira |
| [Grenadier](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/GrenadierRole.cs) | Throw a flashbang to blind nearby players. | Impostor victory. | TOU Mira |
| [Morphling](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/MorphlingRole.cs) | Sample a player, then copy their appearance. | Impostor victory. | TOU Mira |
| [Swooper](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/SwooperRole.cs) | Turn invisible briefly to approach or escape players. | Impostor victory. | TOU Mira |
| [Venerer](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorConcealing/VenererRole.cs) | Kills upgrade your power: camouflage, sprint, then freeze nearby players. | Impostor victory. | TOU Mira |

## Classic — Impostor Killing

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Ambusher](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorKilling/AmbusherRole.cs) | Track a player, then ambush someone near them. A successful kill returns you with the body. | Impostor victory. | TOU Mira |
| [Bomber](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorKilling/BomberRole.cs) | Place bombs that kill players in range after a delay. | Impostor victory. | TOU Mira |
| [Parasite](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorKilling/ParasiteRole.cs) | Control an infected player, then leave them to die away from you. | Impostor victory. | TOU Mira |
| [Scavenger](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorKilling/ScavengerRole.cs) | Kill assigned targets for a shorter cooldown. Killing others increases it. | Impostor victory. | TOU Mira |
| [Warlock](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorKilling/WarlockRole.cs) | Curse a player, then use them to kill someone nearby. | Impostor victory. | TOU Mira |

## Hide and Seek — Impostor Seeker

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Camouflager](IntegratedTownOfUs/Roles/HideAndSeek/Seeker/HnsCamouflagerRole.cs) | Disguise as a hider to blend in while hunting. | Impostor victory. | TOU Mira |

## Classic — Impostor Support

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Blackmailer](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/BlackmailerRole.cs) | Silence a player during the next meeting. | Impostor victory. | TOU Mira |
| [Bootlegger](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/BootleggerRole.cs) | Share a drink to roleblock a player. | Impostor victory. | TOU Mira |
| [Hypnotist](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/HypnotistRole.cs) | Hypnotize players, then trigger Mass Hysteria to distort what they see. | Impostor victory. | TOU Mira |
| [Janitor](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/JanitorRole.cs) | Clean bodies so they cannot be reported. | Impostor victory. | TOU Mira |
| [Miner](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/MinerRole.cs) | Place connected vents around the map. | Impostor victory. | TOU Mira |
| [Undertaker](IntegratedTownOfUs/Roles/Classic/Impostor/ImpostorSupport/UndertakerRole.cs) | Drag and drop bodies to hide them. | Impostor victory. | TOU Mira |

## Town of Polus — Neutral

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Serial Killer](IntegratedTownOfUs/Roles/TownOfPolus/Neutral/PolusSerialKillerRole.cs) | Hunt players as an independent killer. | Be the last killer alive. | TOU Mira |

## Classic — Neutral Afterlife

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Spectre](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralAfterlife/SpectreRole.cs) | Complete your tasks as a ghost to earn your own victory. | Complete your ghost tasks. | ToU, reworked |

## Kill Frenzy — Neutral Benign

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [FrenzyGhostRole (mode-specific variant)](IntegratedTownOfUs/Roles/KillFrenzy/FrenzyGhostRole.cs) | Spectate after death and keep your original role's win condition. | Your original role's victory. | TOU Mira |

## Classic — Neutral Outlier

| Role | Ability | Goal | Origin |
|---|---|---|---|
| [Chef](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralOutlier/ChefRole.cs) | Cook bodies and serve enough portions to meet your quota. | Meet the serving quota. | TOU Mira |
| [Inquisitor](IntegratedTownOfUs/Roles/Classic/Neutral/NeutralOutlier/InquisitorRole.cs) | Find the players holding your target roles and kill them. Killing a non-target removes Vanquish. | All Heretics die. | ToU, reworked |

## Base-game roles

The startup registry also contains these base-game roles; their presence is not a promise that every vanilla role is enabled in every MTOU mode: Crewmate, Impostor, Scientist, Guardian Angel, Shapeshifter, Noisemaker, Phantom, Tracker, Viper, Detective, Influencer.
