# CombatSim — Project Context File
# Keep this file current. Paste it into every new chat window at the start of a session.
# Update the "Active Milestone" and "Decisions Log" sections as work progresses.

---

## Project Identity

- **Project name:** CombatSim
- **Root folder:** C:\dev\CS\
- **Abbreviation:** CS
- **Solution name:** CombatSim (currently CombatSim.Core, CombatSim.Console; will grow further, e.g. CombatSim.Tests, as the project develops)
- **.NET version:** .NET 10
- **Language:** C# — traditional constructors preferred over primary constructors (interview clarity)
- **Editor:** VS Code + dotnet CLI exclusively (not Visual Studio)

---

## What CombatSim Is

A Pathfinder 2e battle simulator. Two parties of combatants face each other. Each combatant
has HP, AC, ToHit, and a damage expression (e.g. "2d8+6 slashing"). The simulation runs
round by round — each combatant takes a turn, attacks someone on the opposing side, damage
is applied, HP drops, dead combatants stop acting — until one side is fully wiped out.
The winner is reported.

Eventually (later milestones) the simulation will be runnable N times to report win
percentages empirically (Monte Carlo style), contrasting with BattleReady's analytical
probability approach. (This part is already built — see Milestone 2.)

**Relationship to BattleReady:**
CombatSim is a deliberate companion project to BattleReady (C:\dev\BR\), built as a
separate learning exercise. It will eventually call BattleReady's live deployed Azure API
over HTTP to parse damage expressions (e.g. "2d8+6 slashing" → structured dice data),
demonstrating service-to-service HTTP calls. It shares the PF2e domain but solves a
fundamentally different problem: mutable state + simulation over time vs. static
probability calculation.

**BattleReady live API base URL:**
https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net

---

## Guiding Principles (agreed before milestone 1)

1. **No copy-pasting from BattleReady.** Mark may look at BattleReady for general ideas,
   but all code is typed fresh. The goal is re-deriving architecture, not cloning it.
2. **Milestones are problem statements, not specs.** Claude hands Mark a situation to
   solve; no field names, class names, or implementation hints are given upfront.
3. **Tight loop with opt-out.** Default is frequent check-ins. Mark can declare "I'm
   taking a bigger chunk" and go quiet — that's explicitly fine. Check-in when done or
   stuck.
4. **Divergence from BattleReady's history is not failure.** If Mark's design choices
   differ from BattleReady's, that's a real decision to compare and discuss — not a
   wrong answer to correct.
5. **One chat window per milestone (roughly).** Split at milestone boundaries. Update
   this file before closing any window. Paste this file at the start of every new window.

---

## Milestone Shape (rough — subject to adjustment as work progresses)

| # | Focus | Status |
|---|-------|--------|
| 1 | Single console app — hardcoded parties, real dice rolling, full round-by-round simulation, termination condition, winner reported | **COMPLETE** |
| 2 | Multiple simulations (run N times), win-percentage reporting | **COMPLETE** |
| 3 | Project split — Core + Console — once logic is rich enough that testing without running the app becomes a felt need | **COMPLETE** |
| 4 | Wire up GitHub repo and free-tier Azure deployment (rehearsing the setup already done for BattleReady) | Not started |
| 5 | Replace stub damage parsing with real HTTP call to BattleReady's ParseDamage API endpoint | Not started |
| 6 | Web API project — expose the simulation as an HTTP endpoint | Not started |
| 7 | Dependency Injection, service interfaces, proper layering | Not started |
| 8 | Input validation, error handling | Not started |
| 9 | Persistence (logging simulation results) | Not started |
| 10 | Testing — unit + integration | Not started |
| 11+ | Further features TBD (JWT, versioning, etc. mirroring BattleReady's later journey) | Not started |

---

## Active Milestone

**Milestone 4 — NOT YET STARTED**

Focus: wire up the GitHub repo and free-tier Azure deployment for CombatSim, rehearsing
the same setup already completed for BattleReady. Unlike the other milestones, this one
is closer to environment/tooling setup than a design problem — Claude will walk Mark
through the steps rather than handing him an open-ended problem statement.

Problem statement / walkthrough: not yet written. Claude will prepare it at the start of
the next chat window, in the new session where Milestone 4 begins.

Milestone 3 wrap-up notes:
- Solution split into two projects: `CombatSim.Core` (all simulation logic — models,
  `SimulatorService`, `DieRoller`, degree-of-success calculation, combat/round/attack
  output types, aggregation/reporting) and `CombatSim.Console` (entry point only —
  builds a hardcoded `CombatInput`, calls `SimulatorService`, prints results).
- `CombatSim.Console` references `CombatSim.Core` via `ProjectReference`, correct
  dependency direction (Console → Core, not reverse).
- Solution file is `CS.slnx` (the newer XML-based solution format, not legacy `.sln`),
  referencing both project `.csproj` files.
- Folder structure under Core uses a `Features/Simulator/Models/...` and
  `Features/Simulator/Services/...` shape, with subfolders per concern (`Attack`,
  `Combat`, `Creature`, `Round`, `Shared`). Namespaces are currently flat
  (`CombatSim.Core.Features.Simulator.Models` for all model files regardless of
  subfolder) rather than mirroring the subfolder structure — a known, accepted
  inconsistency, not a priority to fix right now.
- Visibility: everything left `public` for now. No internal implementation detail in
  Core currently needs hiding from Console, so no `internal` was introduced solely for
  the sake of the project boundary. Revisit if/when Core grows encapsulated internals.
- Cleaned up two stub/leftover artifacts found during the split: the default
  `Class1.cs` generated by `dotnet new classlib` (unused, deleted), and a dead
  `CreatureInitializer.Setup()` helper that had drifted out of sync with the roster
  hardcoded directly in `Program.cs` (Trolls at 100 HP vs. 150 HP) — removed rather
  than reconciled, since nothing referenced it.
- Verification approach (no automated tests yet — that's now Milestone 10): ran the
  Console app manually for both code paths (`doMonteCarlo = true` and
  `doMonteCarlo = false`) and confirmed output looked correct. Acknowledged limitation:
  manual smoke-testing catches gross breakage but not subtle regressions (e.g. a
  silently dropped stat), especially with RNG making every run different. This is
  expected to improve once the testing milestone introduces real tests.

---

## Decisions Log

| Decision | Reasoning |
|----------|-----------|
| CombatSim chosen as project name | Clean abbreviation (CS), scales well to CombatSim.Core etc., concise |
| Root folder C:\dev\CS\ | Parallel to BattleReady's C:\dev\BR\ — clean separation |
| Damage parsing via HTTP call to BattleReady API (not referenced as a library) | Demonstrates real service-to-service HTTP client pattern; more portfolio-relevant than a DLL reference |
| Dice rolls are real random rolls (not probability calculations) | Simulation approach — empirical vs. analytical; deliberate contrast with BattleReady |
| Milestone 1 uses stub/hardcoded damage parsing (2d6+3) | Avoid debugging simulation loop AND HTTP client simultaneously; clean it up once real HTTP parsing is added (now Milestone 5, after GitHub/Azure setup was inserted as Milestone 4) |
| Inserted GitHub/Azure wiring as its own Milestone 4 (renumbering damage-parsing HTTP and everything after it) | Deployment tooling deserves visibility in the milestone narrative for portfolio purposes, but is a mechanical/rehearsal task rather than an open-ended design problem — different in character from the other milestones |
| Milestone 1 is deliberately ugly/hardcoded | Same philosophy as BattleReady's own origin — get something end-to-end before adding architecture |
| Focus fire targeting (always attack the first living enemy) | Realistic default strategy; produces faster kills and more decisive outcomes than random selection |
| `attacker.HP > 0` guard kept inside the round loop | Creatures can die mid-round; the alive list is built once at round start so the guard is necessary |
| Shared static `Random` instance in `DieRoller` | Avoids re-seeding issue from instantiating `new Random()` per call |
| Winner stored as `DidHeroesWin` bool on `CombatOutput` | Simple, sufficient for Milestone 1; can be enriched later |
| Clone-before-run pattern for Monte Carlo (Milestone 2) | `Fight()` mutates `Creature.HP` in place; running N times requires fresh state each run. `CombatInput.Clone()` delegates to a `Clone()` method on `Creature` itself, keeping copy logic next to the fields it copies rather than hand-copied by a calling type. |
| Aggregation/reporting given its own type (`CombatOutputCollectionReport`) | Keeps win%, lose%, avg rounds, etc. out of `Program.cs`; produced via `CombatOutputCollection.GetReport()` |
| Two-project split: `CombatSim.Core` + `CombatSim.Console` (Milestone 3) | Enables testing simulation logic in isolation ahead of the testing milestone (now Milestone 10); Console reduced to wiring + printing only |
| `.slnx` used instead of legacy `.sln` | Newer XML-based solution format; both projects referenced from it |
| Flat namespace across Core `Models` subfolders (not mirroring folder structure) | Accepted inconsistency for now — folders organize by concern, namespace doesn't subdivide further; not a current priority |
| Everything left `public` after the split | No encapsulated internals in Core yet that need hiding from Console; avoids premature visibility restrictions |
| Removed dead `CreatureInitializer.Setup()` rather than reconciling it with `Program.cs`'s hardcoded roster | It was unreferenced and had drifted out of sync (100 HP vs. 150 HP trolls) — simplest correct fix was deletion, not merging |

---

## How to Use This File

**At the end of a session:**
- Update "Active Milestone" with current status and any mid-milestone notes
- Add any new decisions to the Decisions Log
- Update the milestone table status column

**At the start of a new session:**
- Paste this entire file into the new chat window
- Tell Claude which milestone you're on and whether you're starting fresh or resuming
- Claude will read this and be immediately up to speed

---

## Related Projects & Resources

- **BattleReady repo:** https://github.com/mark-raymond-dev/BattleReady
- **BattleReady live Swagger:** https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net/swagger
- **BattleReady Study Guide v4:** generated document covering 125 concepts across the BattleReady build journey — primary study reference
- **BattleReady root folder:** C:\dev\BR\
