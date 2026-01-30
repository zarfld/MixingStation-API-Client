“**public interface mirrors REST**” is a core policy, make it an explicit **architecture decision** and then enforce it everywhere (naming, codegen boundaries, examples).

There are two viable variants; both satisfy your rule, but with different tradeoffs.

---

## 1) Decide the policy precisely (no ambiguity)

### Policy statement (recommended)

> **All public client members that directly represent REST endpoints MUST use the same nouns and verbs as the REST API path/operationId, and MUST preserve the REST resource grouping.**
> Additional “convenience” APIs are allowed only if they are **clearly separated** (namespace/module), and they must reference the underlying REST name in docs.

This avoids “random names”, but still allows a higher-level façade if you ever need it—without polluting your primary surface.

---

## 2) Structure the public surface to prevent drift

### Option A (strictest): *One-to-one REST client*

* Public API is **exactly**:

  * `App.*` methods for `/app/*`
  * `Console.*` methods for `/console/*`
  * `Con.*` methods for `/con/*` (value set/get)
* Method names derived from `operationId` if present; else derived deterministically from `HTTP method + path`.

**Pros:** maximum traceability, no invented semantics
**Cons:** less ergonomic; callers see raw REST concepts (paths, DTOs)

### Option B (recommended): *Public REST client + separate “extras”*

* Primary package: `MixingStation.Rest.*` (mirrors REST names)
* Secondary package/namespace: `MixingStation.Workflow.*` (your harmonized workflows: export lists, validate config, venue modes)

**Pros:** strict REST mirror for core; workflows don’t contaminate the contract surface
**Cons:** slightly more structure

Given your repo goals (input lists, validation, venue automation), Option B is typically best.

---

## 3) Make it an ADR (so Copilot and contributors must follow)

Create: `03-architecture/adr/000X-public-api-mirrors-rest.md`

**Content outline**

* **Decision:** public interface naming mirrors REST API names and grouping
* **Rationale:** traceability, avoids invented semantics, aligns with docs/swagger
* **Consequences:**

  * no “friendly renames” in core client
  * if ergonomic wrappers exist, they go in a separate namespace/module and are explicitly documented as wrappers

---

## 4) Enforce it mechanically (not by hope)

### 4.1 Namespace + folder conventions

Example layout:

```
src/
  MixingStation.Client/                 # strict REST mirror
    App/
    Console/
    Con/
    Models/                             # DTOs generated or mapped 1:1
  MixingStation.Workflows/              # harmonized workflow layer
    Export/
    Validation/
    VenueAutomation/
```

### 4.2 Naming rules you can automate

* **Group name** = first path segment (`app`, `console`, `con`)
* **Method base name** = `operationId` (preferred)
* Fallback: derive name from path:

  * `/app/mixers/current` → `App.MixersCurrentGet()` (or `AppMixersCurrentGet()` if no grouping class)
  * `/app/mixers/connect` → `App.MixersConnectPost()`
  * `/console/data/subscribe` → `Console.DataSubscribePost()`
* **DTO names**: derived from schema names as-is; no domain renames.

### 4.3 Add a CI check (“API naming guard”)

A simple CI rule can prevent drift:

* Parse `openapi.json`
* Build an expected list of public methods (name + group)
* Reflect over compiled assembly and compare
* Fail CI on mismatch

This is the real solution to “no random names”.

---

## 5) How to handle “workflows” without violating the rule

Your workflows (export lists, validate config) are not REST endpoints; so they would naturally violate “names must match REST” if placed in the same surface.

So:

* Keep workflows in `MixingStation.Workflows.*`
* Workflows accept/return canonical models and call the strict REST client internally
* Documentation states clearly:

  * “Workflows are higher-level helpers; REST client is contract-mirror.”

This keeps your public REST interface pure and traceable.

---

## 6) Update Copilot instructions accordingly

Add to `.github/copilot-instructions.md`:

* “Do not invent method/property names for REST endpoints.”
* “Use operationId/path-derived naming rules exactly.”
* “Workflow helpers must live under `MixingStation.Workflows` and must call the REST-mirror client.”

.
