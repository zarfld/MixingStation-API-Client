---
name: api:implement-endpoints
description: Implement one endpoint group as a vertical slice (methods + errors + tests + docs updates).
argument-hint: "Which tag/resource group? e.g., Users, Projects, Measurements..."
agent: api-implementer
tools: ["githubRepo", "terminal", "search", "usages"]
---
Implement a single endpoint group in a vertical slice:
- Add public methods (domain naming)
- Wire transport/auth/serialization/errors consistently
- Add integration tests: happy path + negative path
- Update docs/examples

Stop after one group. Keep changes reviewable.
