# Reviewed mechanics and runtime limits

These notes describe the supplied implementation and the publication copy. Native registration/startup and deterministic rule tests do not establish that each complete interaction succeeds in multiplayer. All below need full host/client regression matches.

## Dirtek (`SoulguardRole`)

The host automatically binds one random living target through AssignPermanentTargets/DirtekBondRuntime. The target is permanent; no ability cooldown or change-target option is required. FindLivingGuard and the shared protection pipeline cancel attacks on the bound target while the guard lives. Protection feedback uses Dirtek's color with visibility rules handled by ProtectionRuntime.

Dirtek himself bypasses defensive shields in the reviewed protection paths; receiving a marker is not intended to confer survival. When Dirtek dies, the bond runtime kills the target using a forced bond-death marker/bypass, with a processed-state guard against repeats. Meeting, exile and disconnect handling exist, but the complete cross-role death matrix was not replayed for this release. DidWin requires both Dirtek and target to be alive. Registered category: Neutral Benign, not a newly invented Neutral Protective menu.

## Doctor

Marks are unique per target and restricted by cooldown/per-round cap. Living Crewmates other than the Doctor form the reveal denominator; only living marked Crewmates count. The host's mark that crosses the threshold grants no shield; other marks add DoctorShieldModifier. Shields are one-hit and removed if their Doctor dies/ceases being Doctor. Private blue plus markers are drawn only for the local Doctor.

Reveal is requested from a meeting and evaluated by the host. With at most two living players, CanRevealConditions permits reveal regardless of marks; no living eligible Crewmates also permits it. A failed early attempt locks marking, with reset/conditional unlock logic in DoctorRuntime. Successful reveal disables marking and exposes the identity to other players. Immortality requires the option and at least three living players; exile is separately configurable. The guessing pipeline contains a revealed-Doctor rejection, independently of an ordinary attack shield.

Presentation now uses a stationary native medical character, two original cyan pulses and a drawn cross, with synthesized audio. No Mayor reveal clip or old Doctor WAV is used. The Doctor medical outfit is retained for the match. The original profile outfit is restored when match state is cleared. It does not dress the player as Mayor. The source contains placement and cleanup logic; full-match visibility to every peer remains unverified.

Sources: `Roles/DoctorRole.cs`, `Modules/Doctor/`, `Modules/ProtectionRuntime.cs`, meeting guessing patches and `Options/NewCrewOptions.cs`.

## One More Chance

Valid roles are Sheriff, Officer, Jailor and Furas; AuditRules excludes Vigilante and other roles. The consuming action/host reserves the use, records the owner/victim pair and synchronizes removal. RpcSheriffMisfire, OfficerShootButton, JailorEvents and FurasRuntime consult the forgiven result to cancel the owner penalty while keeping the victim elimination. It is removed from active modifiers rather than merely changing a description; subsequent mistakes use normal role rules. Relevant state is cleared between games. Full attack/latency combinations remain untested.

## Executioner and Amnesiac

Both actions are available **during the initial walking round, before the first meeting starts**, when their host options are enabled. They are not first-meeting buttons. Change Target and Renounce share one Used reservation: choosing one consumes the one-time choice and prevents the other.

The host validates a living Executioner in-round with no completed win, then chooses uniformly from eligible living connected Crewmates. It excludes the owner, current target, spectators, other Executioner targets, Guardian Angel targets and alliance-marked players. It does not use "any living player" eligibility. No alternative candidate means no used reservation. Accepted target application is replicated without re-running time-sensitive local eligibility on clients; target markers, voters, task header and HUD are refreshed.

Renounce removes old target markers and changes the actual role to Amnesiac via ChangeRole(RoleId.Get<AmnesiacRole>()), rather than adding a modifier. Its new role governs abilities and victory. Original Executioner target-ejection rules otherwise remain in place. Deterministic eligibility/first-round tests passed; actual button clicks, target assignment and transformation across multiple clients remain unverified.

## Jester and Spectre

JesterExileState captures ballot resolution with the role at that moment and confirms a qualifying ejection. JesterExileRuntime supplies the preserved outcome to Jester/neutral winner checks even if the player's current instance becomes Spectre. The host setting determines immediate ending versus other configured outcomes. Non-vote deaths, skipped votes and ties do not qualify. This does not disable Spectre transformations globally.

The 231 state assertions passed again for this candidate. Earlier local native fixtures are historical evidence, not a replay of a real vote or proof of the final winner screen in this release. Multiplayer result replication needs a real match.

## Deputy

Deputy camps another player. If that player dies, the killer becomes the candidate for a shot at the following meeting. Success kills that killer; an incorrect shot has the original behavior. RevealDeputyUponShot controls no-reveal/role-announcement/player-reveal presentation; WarnKiller controls warning behavior.

The supplied DeputyShotPresentation uses a dedicated effect/lifecycle object, guarded timing and cleanup instead of leaving a borrowed overlay actor active indefinitely. The release preparation preserved cooldowns, hit rules, shot limits and penalties. The current native startup test does not exercise the shot animation; resolution, interruption, repeated RPC and cleanup cases require regression tests.

## Plumber

The role retains Flush and vent-related access but its current special placement drops a **nail into a vent**, not a vent wall. PlumberNailRuntime validates placement and triggers through replicated VentSpikeState. Each trap is single-use: the first accepted eligible entry removes it and increments that player's hit counter. Reaching HitsRequiredToKill makes that entry lethal and resets the player's accumulated hits. Hits can therefore span several traps. Duplicate same player/vent entries are suppressed until exit; active trap count is limited per owner. The replicated state tests cover these rules; they do not simulate live network packets or every vent map.

Settings include flush cooldown, required hits, maximum trapped vents and trap cooldown. The one-nail image is `Resources/Icons/PlumberNail.png`; embedded logical resource naming is preserved.

## Exorcist

Drag/Drop tracks corpse carriers. RequiredBodies is based on **all unreported bodies on the map**, clamped to 1–3, rather than always three or only nearby bodies. BodyGroupRules searches a valid subset near the Exorcist and requires every pair within 1.5 units. Prayer's host validation uses GameHistory.KilledPlayers for those victims; forced Dirtek bond deaths are excluded. Only qualifying Impostor/Neutral Killing killers are selectable, with one random valid history entry.

A valid prayer consumes a use and queues the actual killer name/role for the next meeting on the Exorcist's client only, with green flash and sound. Missing grouping/history conditions return red feedback without consuming a use; button cooldown is reset after the button's click processing. The inherited English description's "three bodies" shorthand was stale relative to runtime. Dead/disconnected killer and all drag/report/meeting combinations still require match testing.

## Sweets

Normal Impostor Kill remains available. One Flower/Pollinate button changes mode after the quota. Recipients must be living, distinct, non-Impostor-aligned players; already flowered targets cannot receive another in the same cycle. RequiredFlowers scales the cap down to living eligible players. Flower holder information appears at a meeting without naming Sweets.

Pollinate runs the percentage independently for each living holder, issues protected special multi-murder, then consumes flowers and resets counters/cooldown, allowing further cycles. The inherited "once per game" wiki sentence was stale. **Flowers, FlowersGiven and ReadyAt are static shared state rather than per-Sweets state**; multi-Sweets interactions are not certified and should be tested before enabling multiple copies.

## Other review scope

Furas control/attack, Bandit's deferred role loan, Gambler's next-round mark activation, Hugger's immediate meeting murder, Big Brain's meeting-only report and Time Lord risk are implemented in the supplied code and cataloged. No full match was run for them in this release preparation. The source-wide inventory, compilation and rule/RPC checks are not a line-by-line correctness proof for all 1,128 source files.
