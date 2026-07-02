# CombatSim — Project Context File
# Keep this file current. Paste it into every new chat window at the start of a session.
# Update the "Active Milestone" and "Decisions Log" sections as work progresses.
# (test change)

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
| 4 | Wire up GitHub repo and free-tier Azure App Service (rehearsing the setup already done for BattleReady) | **COMPLETE** |
| 5 | Replace stub damage parsing with real HTTP call to BattleReady's ParseDamage API endpoint | Not started |
| 6 | Web API project — expose the simulation as an HTTP endpoint; deploy to the Azure App Service provisioned in Milestone 4 | Not started |
| 7 | Dependency Injection, service interfaces, proper layering | Not started |
| 8 | Input validation, error handling | Not started |
| 9 | Persistence (logging simulation results) | Not started |
| 10 | Testing — unit + integration | Not started |
| 11+ | Further features TBD (JWT, versioning, etc. mirroring BattleReady's later journey) | Not started |

---

## Active Milestone

**Milestone 5 — NOT YET STARTED**

Focus: replace the hardcoded/stub damage parsing (currently `2d6+3`) with a real HTTP
call to BattleReady's live `ParseDamage` API endpoint
(https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net). This is the
first milestone requiring an HTTP client from CombatSim.Core out to an external service.

Problem statement / walkthrough: not yet written. Claude will prepare it at the start of
the next chat window, in the new session where Milestone 5 begins.

---

## Milestone 4 wrap-up notes (GitHub + Azure setup — COMPLETE)

**GitHub:**
- Local git repo initialized from zero in `C:\dev\CS\`.
- `.gitignore` generated fresh via `dotnet new gitignore`.
- Initial commit made ("Initial commit: Milestone 3 complete") and pushed to
  `main` on `github.com/mark-raymond-dev/CombatSim` (public repo).
- Repo confirmed live with all Milestone 3 contents: `CombatSim.Console/`,
  `CombatSim.Core/`, `.gitignore`, `CONTEXT.md`, `CS.slnx`.

**Azure App Service:**
- Provisioned via the Azure Portal (Azure CLI not installed on Mark's machine —
  Portal used for the entire Milestone 4 Azure flow, not CLI).
- **Resource Group:** CombatSim-rg
- **App Service name:** `combatsim-api` (no uniqueness suffix needed)
- **Default hostname:** `combatsim-api-f6ezbgd7ayhbd4c6.westus3-01.azurewebsites.net`
- **Region:** West US 3 (matches BattleReady)
- **OS:** Windows (matches BattleReady)
- **Runtime stack:** .NET 10
- **Pricing tier:** F1 (Free) — found under the **Dev/Test** tab in the pricing
  plan selector, separate from the Basic/Standard/Premium production tiers.
  Confirmed on F1 after creation.
- **Status:** resource shell only — no code deployed yet. Visiting the hostname
  currently shows Azure's default placeholder page. Real deployment of
  `CombatSim.Api` (once it exists) is deferred to Milestone 6.

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
| Azure setup done via Portal, not CLI (Milestone 4) | Azure CLI (`az`) not installed on Mark's machine; rather than pausing to install/configure it, the Portal UI was used directly, matching how BattleReady's tier discovery (F1 under Dev/Test) was already found interactively |
| Azure App Service provisioned as an empty resource shell in Milestone 4, ahead of `CombatSim.Api` existing | Rehearses environment/deployment tooling on its own timeline, decoupled from the Web API project build-out; real deployment happens in Milestone 6 once there's code to push |
| App Service OS set to Windows, runtime .NET 10 | Matches BattleReady's own environment for a consistent comparison between the two projects |

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
- **CombatSim repo:** https://github.com/mark-raymond-dev/CombatSim
- **CombatSim Azure App Service (empty shell, awaiting Milestone 6 deployment):** https://combatsim-api-f6ezbgd7ayhbd4c6.westus3-01.azurewebsites.net
