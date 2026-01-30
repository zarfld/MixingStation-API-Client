🔍 Critical Questions Before We Start
1. Project Scope & Priority:

Which ONE epic should we implement first? (I recommend starting small with a vertical slice)
Suggested: Start with Epic 1: Ingest & Normalize as foundation for everything else?

-> Epic 1: Ingest & Normalize — but only as far as required to generate one real artifact.

Vertical slice for Phase 1

Connect to Mixing Station (HTTP only)

Read: current mixer metadata + a minimal set of channel + patch info (whatever is available via /app/* and relevant /console/* paths)

Normalize into a CanonicalModel v0

Export Input List + Patching List to Excel

This yields a concrete deliverable and forces the core abstractions (transport, mapping, model) without overbuilding.

2. Technical Stack (C#):

What .NET version should we target? (.NET 6? .NET 8? .NET Framework 4.8?)
Deployment target? (Desktop app, ASP.NET service, library/NuGet package, all of above?)
HTTP library preference? (HttpClient, RestSharp, Flurl, or let me recommend?)

- Target: .NET 8 (LTS, best modern runtime, good perf, best TLS/HTTP stack)

- Output type: library-first (NuGet-ready) + one CLI example

    - src/YourLib (library)
    - examples/venuectl or examples/exportlists (CLI)

- HTTP library: HttpClient (+ IHttpClientFactory where applicable)
    - Keeps dependencies minimal, works everywhere, easy DI/test.

(WebSocket support comes in Phase 2; still use built-in ClientWebSocket.)


3. MixingStation API Access:

Do you have access to a running MixingStation instance for testing?
Can you share the OpenAPI/Swagger spec file from http://localhost:8045/#/rest-api?
Is the WebSocket endpoint available for testing?
api exploere is available at http://localhost:8045/#/rest-api
data explorer can be found at http://localhost:8045/#/data-explorer

4. Success Criteria:

How will we know Phase 1 is complete? (Example: "Can read mixer config and export input list to Excel")
Test coverage target? (I recommend >80% following XP practices)
Phase 1 is complete when:
- exportlists CLI can:
    1. connect (or detect already connected),
    2. read enough data to build CanonicalModel,
    3. export InputList.xlsx + PatchingList.xlsx,
    4. run integration tests against a local Mixing Station instance (env-var configured).

Test target for Phase 1

Aim: ~70–80% on the library core, but prioritize meaningful tests:
- parsing/normalization rules
- mapping logic
- export formatting
- one integration “smoke” test set (skippable if no server)


5. Stakeholders:

Who approves requirements? (You, or someone else?)
Who will use this first? (Yourself, your team, external users?)

-> For now: I approve requirements; first user is me (and later external users). That implies we optimize for:
- reproducible examples
- predictable CLI behavior
- strict logging and reports