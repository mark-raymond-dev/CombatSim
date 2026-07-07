# CombatSim — Project Context

## Purpose & Context

Mark is building **CombatSim**, a Pathfinder 2e battle simulator in C# (.NET 10), located at `C:\dev\CS\`. It is a deliberate companion learning project to his existing **BattleReady** project (`C:\dev\BR\`), a deployed Azure API for analytical probability calculations. CombatSim takes a simulation approach (real dice rolls, mutable combat state) contrasting with BattleReady's analytical method. The project is structured as a numbered milestone curriculum, with Claude acting as a guide — providing problem statements, code review, and targeted hints rather than upfront solutions.

Key environment and style preferences:
- VS Code and dotnet CLI exclusively (no Visual Studio, no Azure CLI)
- Traditional constructors over primary constructors (interview clarity rationale)
- Prefers verbose/explicit code over condensed one-liners when clarity is gained
- Wants to understand *why* something works, not just accept syntax on faith
- Prefers working through problems independently with hints, but will explicitly ask to "just show me" when done with the Socratic approach — Claude should comply immediately when that signal is given
- Comfortable pushing back on Claude's architectural reasoning; Claude should treat pushback as a prompt to reassess, not restate
- For pure syntax questions (vs. design/reasoning questions), Mark may explicitly ask Claude to skip the Socratic approach and just provide the answer directly

BattleReady serves as an architectural reference point. Established BattleReady patterns (e.g., separating API-layer annotations from Core models) carry weight in CombatSim design decisions, and are treated as the default unless Mark identifies a specific reason to diverge.

---

## Current State: Milestone 6 — COMPLETE (plus post-milestone refinements)

**Milestone 6 built `CombatSim.Api`, a Web API project exposing simulation logic from `CombatSim.Core` over HTTP, and deployed it to Azure.** The milestone itself closed with code, validation, thread-safety, and live deployment all verified (see prior section below, unchanged). Since then, a follow-on session reworked the output-model design and added response-size controls, described here.

### Solution structure (unchanged):
- `CombatSim.Core` — simulation logic, models
- `CombatSim.Console` — console runner
- `CombatSim.Api` — Web API layer (complete, deployed)

### Output model redesign — `Report` sub-objects replace loose metric properties

Both `CombatOutput` and `CombatOutputCollection` were restructured around the same pattern:

- Neither class inherits from `List<T>` anymore (an earlier design mistake, caught and corrected before it shipped) — each holds its child collection as a named property (`Combats` / `Rounds`) alongside a dedicated `Report` property, matching the shape `RoundOutput` already used.
- **`CombatOutput.Report`** is a new `CombatOutputReport` object (per-combat metrics): `TotalRounds`, `DidHeroesWin`, `StartCacheCount`, `EndCacheCount`, `StartTime`, `EndTime`, `ElapsedMilliseconds`.
- **`CombatOutputCollection.Report`** is `CombatOutputCollectionReport` (batch-level metrics): `TotalCombats`, `HeroWinCount`, `HeroLoseCount`, `HeroWinPercentage`, `HeroLosePercentage`, `AverageRoundsPerCombat`, plus the same cache/time fields as above.
- Both `Report` properties are `{ get; internal set; }` — external code (Api, Console) can read a `Report` but cannot swap it out for a different instance.
- **All individual properties on `CombatOutputReport` and `CombatOutputCollectionReport` are also `internal set`** — closing a gap identified during review, where the outer `Report` reference was locked but its individual fields (e.g. `DidHeroesWin`) were still publicly mutable after the fact. Now the whole report — object and fields — is read-only outside `CombatSim.Core`.
- Population happens via `internal void FinalizeReport(...)` methods (one on `CombatOutput`, one on `CombatOutputCollection`) called only from `SimulatorService` at the natural completion point of a combat / a batch. `GetReport()` as a publicly-callable method no longer exists.
- Both `CombatOutput` and `CombatOutputCollection` now take a `startCacheCount` constructor parameter and stamp `Report.StartTime` at construction, so a report can never exist in a "half-built" state where `Combats`/`Rounds` are populated but the report isn't.

### Elapsed-time measurement — `Stopwatch` + `DateTime.UtcNow`, used for what each is good at

An earlier pass of this redesign computed `ElapsedMilliseconds` as `(EndTime - StartTime).TotalMilliseconds` using two `DateTime.UtcNow` reads — convenient since `StartTime`/`EndTime` were already being captured as useful timestamps, but a real regression: `DateTime.UtcNow` has much coarser resolution (~15ms on Windows) than `Stopwatch`, and is subject to system clock adjustments mid-measurement. This showed up as manufactured precision (rounding a coarse value to 5 decimal places).

Fixed by using both tools for what they're each designed for:
- A `Stopwatch`, started in the constructor and stopped inside `FinalizeReport`, drives `Report.ElapsedMilliseconds` via `_stopwatch.Elapsed.TotalMilliseconds` (a `double` — no truncation, unlike the `long`-returning `ElapsedMilliseconds` property).
- `DateTime.UtcNow` is still used for `StartTime`/`EndTime`, which are legitimate wall-clock timestamps (useful for logging/debugging), not a duration measurement.

### `ReturnType` — response-size control for large `SimulationCount` runs

Added to trim response payload for large Monte Carlo-style runs, where round-by-round attack logs dominate the response size and aren't needed if only aggregate stats matter.

- New enum, `CombatSim.Core.Features.Simulator.Models.ReturnType`: `All`, `Reports`, `SummaryReport`.
- New `CombatInput.ReturnType` property (default `All`), included in `CombatInput.Clone()`.
- `SimulatorService.Simulate()` runs the full simulation exactly as before (every attack, every round, every `CombatOutput.Report` finalized), then trims **after** `combatOutputCollection.FinalizeReport(...)` runs, right before returning:
  - `Reports`: clears `Rounds` on every `CombatOutput` (keeps each combat's `Report`, plus the collection's `Report`)
  - `SummaryReport`: clears `Combats` entirely (only the collection-level `Report` survives)
  - `All`: no trimming (unchanged behavior)
- Trimming happens *after* report finalization, so metrics like `AverageRoundsPerCombat` are computed from full data regardless of `ReturnType` — only the returned payload shrinks, not the math.
- Known characteristic, not a bug: this approach still builds every `AttackResult` and formats every log string before discarding them for `Reports`/`SummaryReport` — it saves response payload size, not simulation compute. (Skipping the record-keeping itself, to save CPU/memory too, was identified as a follow-on option — "Approach B" — but deferred since the current approach already performs well; see benchmark below.)
- **Enum JSON serialization**: enums default to integer values in `System.Text.Json`. Registered `JsonStringEnumConverter` globally in `CombatSim.Api/Program.cs` via `AddControllers().AddJsonOptions(...)`, so `ReturnType` (and any future enum added to the project) serializes/deserializes as its string name (e.g. `"SummaryReport"`) rather than an integer, with case-insensitive binding.

### Benchmark result

`SimulationCount = 10000` with `ReturnType = SummaryReport` completes in **under 3 seconds** end-to-end. Since `SummaryReport` mode still performs full simulation work under the hood before trimming, this number reflects real simulation cost, not payload/network overhead — a useful baseline if "Approach B" (skip building unused records) is ever revisited for performance rather than payload-size reasons.

### Known limitation, accepted as-is

`StartCacheCount`/`EndCacheCount` on both report types are read from the shared singleton `_cache` at start/end of a combat or batch. Under concurrent API requests, the delta between them doesn't cleanly isolate *that* request's contribution to the cache — a concurrent request can add entries in between. Mark's intent for these fields is primarily debugging/observability (e.g., confirming the cache persists and grows across the app's lifetime, since it's a Singleton that may have been running for weeks), not a precise per-request metric, so this was explicitly accepted rather than fixed.

---

## Milestone 6 — original delivered architecture (for reference)

**API design:**
- Single endpoint: `POST /api/simulator/simulate`
- `SimulatorController` depends on `ISimulatorService` (interface, not concrete class)
- `ISimulatorService.Simulate(CombatInput)` returns `Task<CombatOutputCollection>` — unified single-fight and multi-run cases (`SimulationCount` on the input drives the loop; `SimulationCount = 1` is just the trivial case)
- `CombatSim.Api/Models/Requests/CombatRequest.cs` and `CreatureRequest.cs` — dedicated request DTOs with `DataAnnotations` + `IValidatableObject` (for cross-field rules like "at least one hero and one monster required"), mirroring BattleReady's Api/Core separation
- `CombatSim.Api/Mapping/CombatRequestExtensions.cs` and `CreatureRequestExtensions.cs` — `ToInput()` extension methods mapping `CombatRequest` → `CombatInput`
- `[ApiController]` handles automatic 400 responses on validation failure — no manual validation code in the controller

**DI wiring (`Program.cs`):**
- `builder.Services.AddHttpClient();` — registers `IHttpClientFactory` cleanly (not the typed-client form)
- `IDictionary<string, ParseDamageResponse>` cache registered as **Singleton** via explicit factory lambda, backed by `ConcurrentDictionary<string, ParseDamageResponse>` (not `Dictionary`)
- `ISimulatorService` registered as **Singleton** via explicit factory lambda: `provider => new SimulatorService(httpClientFactory.CreateClient(), cache)` — bypasses the DI container's constructor-guessing entirely, avoiding the circular-dependency error that plain `AddSingleton<TInterface, TImpl>()` triggered with `IDictionary`'s copy-constructor ambiguity
- `.AddControllers().AddJsonOptions(...)` now also registers `JsonStringEnumConverter` globally (see above)

**Thread-safety — verified, not just reasoned about:**
- The shared `_cache` (damage-parse cache, keyed on damage expression strings) was the *only* shared mutable state in `SimulatorService`; everything else is per-call locals
- Confirmed via a custom C# console load-test script (`Task.WhenAll` firing concurrent POSTs) that `Dictionary`-backed cache threw `InvalidOperationException: "concurrent update... corrupted its state"` under real concurrent load
- Swapping to `ConcurrentDictionary` (keeping the existing `TryGetValue` + manual-assignment pattern, deliberately *not* using `GetOrAdd`, since the cached operation is an expensive external HTTP call and `GetOrAdd`'s factory can run more than once under contention) fully resolved it — verified clean under repeated hammering

**NuGet vulnerability fix:**
- `Microsoft.OpenApi` transitive dependency (via `Microsoft.AspNetCore.OpenApi`) resolved to a vulnerable `2.0.0` (NU1903, GHSA-v5pm-xwqc-g5wc — stack-overflow DoS via malicious OpenAPI document parsing)
- Fixed via a direct `PackageReference` override to `Microsoft.OpenApi 2.7.5` in `CombatSim.Api.csproj` — NuGet prefers an explicit direct reference over the same package pulled in transitively

**Deployment — live and verified:**
- Azure App Service `combatsim-api` (West US 3, Windows, .NET 10, F1 free tier), GitHub Actions CI/CD from the `main` branch
- Fixed a real deployment bug: the auto-generated GitHub Actions workflow ran `dotnet build`/`dotnet publish` with no project argument, which (since the repo root has a `.slnx`) built/published the **entire solution** into one flat output folder — mixing `CombatSim.Console.exe` in with the API's files and missing a clean, unambiguous entry point
- Fix: scoped both commands to the target project — `dotnet build CombatSim.Api -c Release` / `dotnet publish CombatSim.Api -c Release -o "${{env.DOTNET_ROOT}}/myapp"`
- Verified live at `https://combatsim-api-f6ezbgd7ayhbd4c6.westus3-01.azurewebsites.net/swagger/index.html` — Swagger UI renders all schemas correctly, `POST /api/simulator/simulate` executes successfully against the real BattleReady API dependency

**`elapsedMilliseconds = 0` investigation — resolved as correct behavior, not a bug:**
- Observed `elapsedMilliseconds = 0` for all combats after the first in a batch, and eventually for *all* combats including the first
- Root cause (confirmed via a deliberate Azure App Service restart, which clears the singleton's in-memory cache): the damage-parse cache is genuinely app-lifetime (Singleton), so only a truly cold cache pays the BattleReady HTTP round-trip cost (verified: `179ms`, consistent with an earlier `385ms` cold measurement). Every cache-hit combat thereafter completes in sub-millisecond time, which `Stopwatch.ElapsedMilliseconds` (an integer-truncating property) correctly reports as `0`
- A separate, unrelated symptom (a 6-minute delay and repeated browser "Page Unresponsive" dialogs when re-running large `SimulationCount` values without refreshing Swagger) was isolated as a **Swagger UI / browser-side DOM rendering issue** — stacking large rendered JSON responses in the same tab — not a server-side or API bug. Refreshing the Swagger page before each large test eliminated it entirely (confirmed: `SimulationCount = 100` completed in ~32–34 real seconds on a fresh page load)
- No code changes were needed; this was a successful validation exercise, not a fix
- (Superseded by the `Stopwatch`/`DateTime.UtcNow` split above, which now uses `Elapsed.TotalMilliseconds` — a `double` — so this exact symptom can no longer occur, though the underlying explanation still stands as the reason `0` values were ever seen.)

---

## On the Horizon

**Milestone 7 — not yet started.** Candidate scope, to be confirmed at the start of that session:
- Dependency injection refinement (revisit whether Milestone 7 changes anything about today's factory-based registrations)
- Persistence
- Testing

No open items carried forward from Milestone 6 or the post-milestone refinement session — both are fully closed.

---

## Key Learnings & Principles

- **`new HttpClient()` per-call is a critical anti-pattern** — caused 60–80 second runtimes in Milestone 5; `IHttpClientFactory` is the correct solution. In a Web API without container-driven constructor injection, resolve `IHttpClientFactory` explicitly inside a DI factory lambda and call `.CreateClient()` — do not rely on `AddHttpClient<T>()`'s typed-client side effects when manually constructing `T` yourself.
- **Caching identical inputs matters at scale** — a cache keyed on damage expression strings eliminated redundant API calls to BattleReady's `ParseDamage` endpoint and addressed rate limiting (BattleReady: fixed-window, ~30 req/10s, no Retry-After).
- **DI lifetime mismatches cause real bugs** — `AddScoped` with `IDictionary` caused circular dependency issues; `Singleton` is correct for stateful shared caches that should persist for the app's lifetime.
- **Factory lambdas solve constructor ambiguity** — `provider => new ConcurrentDictionary<...>()` bypasses the container's constructor-guessing for types like `IDictionary`, whose implementing type (`Dictionary<TKey,TValue>`) has a copy-constructor overload that otherwise creates a circular-dependency trap.
- **Plain `Dictionary<TKey,TValue>` is not safe for any concurrent read+write mix** — not just write+write. Verified empirically: concurrent HTTP requests hitting a singleton service's `Dictionary`-backed cache threw `InvalidOperationException` reporting internal state corruption. `ConcurrentDictionary` fixed it. `GetOrAdd`'s factory-may-run-more-than-once behavior was deliberately avoided here because the cached operation is an external HTTP call — explicit `TryGetValue` + manual assignment was the correct choice.
- **`dotnet build`/`dotnet publish` with no project argument, run inside a multi-project solution folder, silently targets the whole `.slnx`** — this produced a flattened, ambiguous publish output mixing an unrelated console app's `.exe` in with the Web API's files. Always scope CI/CD build and publish commands to the specific project.
- **NuGet prefers an explicit direct `PackageReference` over the same package pulled in transitively** — used to resolve a `Microsoft.OpenApi` vulnerability warning by adding a direct reference at a patched version, without touching the transitive parent package.
- **A class inheriting from `List<T>` silently drops any additional properties during JSON serialization** — `System.Text.Json` (and Newtonsoft.Json) serializes such a type as a bare array only. Caught before shipping by noticing `CacheCount`/`ElapsedMilliseconds` would vanish from `CombatOutputCollection`'s JSON output. Fixed by composition (a named `Combats` list property) instead of inheritance — consistent with the .NET Framework Design Guidelines' general advice against inheriting from `List<T>`/`Collection<T>`, and consistent with how `RoundOutput` and `CombatOutput` already modeled "has a list" rather than "is a list."
- **Locking down an outer property's setter (`internal set`) doesn't lock down the object it points to** — restricting `Report`'s setter stops external code from swapping in a different `Report` instance, but each property *on* that `Report` object needs its own `internal set` if you also want the fields themselves to be externally read-only.
- **`Stopwatch` and `DateTime.UtcNow` solve different problems** — `Stopwatch` (high-resolution performance counter, immune to clock adjustments) is the correct tool for measuring elapsed duration, even sub-millisecond; `DateTime.UtcNow` is the correct tool for wall-clock timestamps, but its coarser resolution (~15ms on Windows) makes it a poor substitute for duration measurement via subtraction, especially for fast, cache-hit-driven operations.
- **`Stopwatch.Elapsed.TotalMilliseconds` (double) avoids the truncation `Stopwatch.ElapsedMilliseconds` (long) has** — relevant for very fast operations that would otherwise report `0`.
- **Trimming a response payload after building it isn't the same as skipping the work that built it** — clearing `Rounds`/`Combats` before returning reduces what goes over the wire but not the CPU/memory cost of simulating every attack. A benchmark showing "fast" under this approach measures simulation cost, not the cost of the discarded record-keeping; a genuine performance optimization would need to skip building the discarded objects in the first place.
- **`Clone()` fragility is a known deferred risk** — silent staleness when new fields are added; accepted as a known issue. (`ReturnType` was added to `Clone()` correctly this round — a live example of the risk being managed, not yet a case of it causing a bug.)
- **Mark's pushback is often correct** — mid-round attacker guard, `.slnx` vs `.sln` format, option (b) DataAnnotations framing, and the BattleReady-consistency question on where DataAnnotations belong were all cases where Mark identified errors or gaps in Claude's reasoning that led to a better outcome.

---

## Approach & Patterns

- **Session structure**: Claude produces an updated `CONTEXT.md` at the end of each milestone session; Mark replaces his local copy and opens a new chat attaching the file to begin the next milestone
- **Code review loop**: Mark implements independently, submits as zip files, Claude reviews and flags issues with clear rationale
- **Milestone insertion**: When gaps are identified (e.g., GitHub/Azure setup), Mark prefers inserting them as proper numbered milestones with full renumbering, not side detours
- **Architectural decisions are logged** in `CONTEXT.md` with rationale, so future sessions have continuity without re-litigating settled choices
- **Debugging approach**: When investigating an unexpected result, Claude proposes concrete, falsifiable experiments (e.g., "run SimulationCount=5 and report the exact per-combat values," "restart the App Service and re-test") rather than resolving ambiguity through reasoning alone — several apparent bugs this milestone turned out to be correct behavior once tested directly
- **Design conversations happen before code**: significant model reshaping (e.g., the `Report` sub-object pattern, `ReturnType` filtering) is talked through — intent, trade-offs, alternative approaches — before Mark implements independently and submits for review, rather than Claude generating the implementation directly

---

## Tools & Resources

- **Languages/runtime**: C# / .NET 10
- **Editor**: VS Code + dotnet CLI
- **Source control**: Git / GitHub (`mark-raymond-dev` account), repo: `github.com/mark-raymond-dev/CombatSim` (public)
- **Cloud**: Azure App Service `combatsim-api` (West US 3, F1 free tier, Windows), GitHub Actions CI/CD from `main`
- **Live API**: `https://combatsim-api-f6ezbgd7ayhbd4c6.westus3-01.azurewebsites.net/swagger/index.html`
- **External API**: BattleReady `ParseDamage` endpoint (live, rate-limited, occasional cold-start delay from its database)
- **Project context file**: `CONTEXT.md` (passed between sessions as the continuity mechanism)
