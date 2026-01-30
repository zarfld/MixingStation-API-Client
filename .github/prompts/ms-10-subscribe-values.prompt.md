---
name: ms:subscribe-values
description: Implement console value subscriptions and set/get helpers for plain vs normalized values.
agent: Mixing Station Protocol Engineer
tools: ["githubRepo", "terminal", "usages"]
---
Implement:
- POST /console/data/subscribe
- A typed subscription API that emits updates and includes initial values
- Helpers:
  - Subscribe(pathPattern, format=val|norm)
  - SetPlain(path, value)
  - SetNormalized(path, value01)
Add integration tests using a known parameter path (e.g. ch.0.mix.lvl) and assert updates.
