---
name: ms:bootstrap
description: Bootstrap a Mixing Station client with HTTP + WebSocket, state tracking, and minimal smoke tests.
agent: Mixing Station Protocol Engineer
tools: ["githubRepo", "terminal", "search", "fetch"]
---
Create a client skeleton for Mixing Station:
- HTTP transport for REST endpoints
- WebSocket transport using the {path, method, body} envelope
- App state watcher for /app/state
- Minimal smoke tests:
  - GET /app/state
  - GET /app/mixers/current (or /app/mixers/available then connect)
Use env vars for base URL/ports. No secrets committed.
