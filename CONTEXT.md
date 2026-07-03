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

The simulation can be run N times to report win percentages empirically (Monte Carlo
style), contrasting with BattleReady's analytical probability approach.

Damage parsing is no longer a stub — as of Milestone 5, it's a real HTTP call to
BattleReady's live `ParseDamage` endpoint, with in-memory caching to avoid redundant
network calls for repeated damage expressions.

**Relationship to BattleReady:**
CombatSim is a deliberate companion project to BattleReady (C:\dev\BR\), built as a
separate learning exercise. It shares the PF2e domain but solves a fundamentally
different problem: mutable state + simulation over time vs. static probability
calculation. As of Milestone 5, CombatSim.Core makes real HTTP calls out to BattleReady's
deployed Azure API to parse damage expressions (e.g. "2d8+6 slashing" → structured dice
data), demonstrating real service-to-service HTTP integration.

**BattleReady live API base URL:**
https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net

**BattleReady ParseDamage endpoint (as consumed by CombatSim):**
`GET /api/v1/ParseDamage/calculate?Expression={escaped damage expression}`
Returns a JSON body deserialized into `ParseDamageResponse` (DamageDieBase,
DamageDieCount, DamageModifier — hand-coded model class, not generated from Swagger).

**BattleReady rate limiting (confirmed from BattleReady's own Program.cs):**
Fixed window policy — 30 requests per 10-second window, `QueueLimit = 0` (no queueing;
excess requests immediately receive HTTP 429), `QueueProcessingOrder.OldestFirst`.
No `Retry-After` or custom rate-limit headers are emitted (standard ASP.NET Core
rate limiter middleware, not custom-configured to add them).

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
| 5 | Replace stub damage parsing with real HTTP call to BattleReady's ParseDamage API endpoint | **COMPLETE** |
| 6 | Web API project — expose the simulation as an HTTP endpoint; deploy to the Azure App Service provisioned in Milestone 4 | Not started |
| 7 | Dependency Injection, service interfaces, proper layering | Not started |
| 8 | Input validation, error handling | Not started |
| 9 | Persistence (logging simulation results) | Not started |
| 10 | Testing — unit + integration | Not started |
| 11+ | Further features TBD (JWT, versioning, etc. mirroring BattleReady's later journey) | Not started |

---

## Active Milestone

**Milestone 6 — NOT YET STARTED**

Focus: add a `CombatSim.Api` Web API project exposing the simulation as an HTTP endpoint,
and deploy it to the Azure App Service (`combatsim-api`) provisioned as an empty shell
back in Milestone 4.

Problem statement / walkthrough: not yet written. Claude will prepare it at the start of
the next chat window, in the new session where Milestone 6 begins.

---

## Milestone 5 wrap-up notes (Real damage parsing via BattleReady HTTP call — COMPLETE)

**What was built:**
- `SimulatorService` now takes constructor-injected `HttpClient` and
  `Dictionary<string, ParseDamageResponse>` (cache), both created once in
  `Program.cs` and passed in — correct lifetime management, avoiding the
  well-known "new HttpClient() per call" anti-pattern that caused an early
  60-80 second single-combat runtime.
- `ParseDamage(string damageExpression)` — new private method. Checks cache first
  (`TryGetValue`), returns cached result immediately on hit. On miss, calls
  BattleReady's `GET /api/v1/ParseDamage/calculate` endpoint via `HttpClient.GetAsync`
  (not `GetFromJsonAsync`, deliberately — raw `HttpResponseMessage` needed to branch
  on status code without throwing). On success, deserializes, caches, returns.
- **429 (Too Many Requests) handling:** on a non-429 failure, throws immediately.
  On a 429, retries up to 3 times with exponential backoff (`300ms * retry`),
  breaking out early on first success; throws only if all retries are exhausted.
  Retry delay of 300ms was chosen with BattleReady's actual limiter numbers in mind
  (30 req / 10 sec ≈ 333ms average sustainable spacing).
- `GetApiCallExceptionSuffix(HttpResponseMessage)` — small helper consolidating
  exception-message formatting (URL, status code, reason phrase) used at both throw
  sites.
- `Uri.EscapeDataString` used to properly encode the damage expression (handles
  spaces, `+`, etc.) into the query string — `Uri.EscapeUriString` was considered
  and correctly rejected as obsolete/wrong for this purpose.
- `RollDamage` simplified to just call `ParseDamage` and use the result — HTTP/cache
  concerns fully separated from damage-roll math.
- `Thread.Sleep` (used for the optional inter-round delay) replaced with
  `await Task.Delay` — correct async citizenship, avoiding a blocked thread even
  though the current single-fight, sequential-loop structure means there's no
  observable speed difference yet. Delay parameter now defaults to `0`
  (no delay) since caching made a blanket per-round delay largely unnecessary.
- `CacheCount` surfaced on both `CombatOutput` (per-fight snapshot of cache size —
  will read identically across most fights once the small set of distinct damage
  expressions has been seen) and `CombatOutputCollection` / its report (final
  cumulative cache size after a full `FightMultiple` batch, set once at the end).
  Confirmed via actual run: 5 distinct damage expressions in the test roster → 
  `CacheCount == 5`. This was a deliberate "verify, don't just assume no-exception-
  means-correct" check.
- `ParseDamageResponse.cs` hand-coded (not generated from Swagger/OpenAPI) — reasonable
  given only 3 primitive properties. Placed under `Models/Shared/` (moved there from
  an initial `Models/ParseDamage/` location) to match the precedent of `DieRoller` and
  `DegreeOfSuccessCalculator`, which are also cross-cutting rather than domain-model
  types.

**Known deferred / non-priority items (tracked, not resolved):**
- `HttpClient` and the cache `Dictionary` are manually constructed and passed via
  plain constructor injection in `Program.cs` — no DI container yet. This is
  explicitly correct for now; a real DI container (and likely `IHttpClientFactory`
  in place of a raw long-lived `HttpClient`) is Milestone 7's concern.
- Cache is a plain `Dictionary`, not `IMemoryCache` — deliberately: no expiration
  need (parsed damage expressions never go stale), no memory pressure at this scale,
  no concurrency need given the current single-threaded sequential fight loop.
  Worth revisiting only if either of those assumptions changes.
  Investigated OpenAPI/NSwag-style client generation as an alternative to hand-coding
- response models for more complex APIs — not used here (too small a payload to
  justify it) but noted as a real tool for future, larger integrations.
- Retry/backoff logic is intentionally basic (fixed 3 retries, simple linear-scaled
  backoff) — comprehensive resilience patterns explicitly out of scope per the
  Milestone 5 problem statement; Milestone 8 (validation/error handling) is the
  more natural home for anything more sophisticated if it's ever needed.
- No exploration yet of whether Monte Carlo runs (1000 fights) could be parallelized
  for speed — discussed conceptually (Task.Delay vs. Thread.Sleep does NOT by itself
  produce parallelism; that would require restructuring FightMultiple's loop, e.g.
  via Task.WhenAll) but not attempted. Flagged as a bigger, separate architectural
  question, not a Milestone 5 concern.

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
| `HttpClient` constructor-injected into `SimulatorService`, created once in `Program.cs` (Milestone 5) | Avoids the "new HttpClient() per call" socket-exhaustion anti-pattern; a full DI container / `IHttpClientFactory` deferred to Milestone 7 as the more complete solution |
| In-memory `Dictionary<string, ParseDamageResponse>` cache, constructor-injected alongside `HttpClient`, keyed on the raw damage expression string | Damage expressions are few in number and never change meaning once parsed — no expiration/eviction/concurrency need justifies `IMemoryCache`'s extra complexity at this scale; plain `Dictionary` is honest to the actual requirements |
| `HttpClient.GetAsync` used instead of `GetFromJsonAsync` for the ParseDamage call | Needed the raw `HttpResponseMessage` to branch on status code (specifically 429) without an automatic throw/deserialize hiding that information |
| 429 (Too Many Requests) retried up to 3x with exponential backoff (300ms × retry); all other failures throw immediately | Matches BattleReady's actual confirmed rate limiter (30 req/10s fixed window, no queueing, no Retry-After header) — retrying only makes sense for the specific transient case a rate limiter represents, not for genuine errors |
| `ParseDamageResponse` hand-coded rather than generated via NSwag/OpenAPI tooling | Reasonable at 3 simple primitive properties; codegen tooling noted as the right call for larger/more complex response contracts in the future |
| `ParseDamageResponse.cs` placed in `Models/Shared/` (moved from an initial `Models/ParseDamage/` location) | Matches the precedent set by `DieRoller` and `DegreeOfSuccessCalculator` — cross-cutting utility-adjacent types, not domain models like `Creature`/`CombatInput` |
| `Thread.Sleep` replaced with `await Task.Delay` for the optional inter-round pacing delay | Correct async citizenship — doesn't block a pooled thread — even though the current sequential single-fight-at-a-time structure means no observable speed difference yet; sets up good habits ahead of any future parallelization |
| `CacheCount` added to both `CombatOutput` (per-fight) and `CombatOutputCollection`/its report (final cumulative, set once after all fights complete) | Different questions answered at each level; also served as a deliberate correctness check (confirmed 5 distinct damage expressions → CacheCount == 5) rather than trusting "no exception thrown" as proof the cache worked |

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
