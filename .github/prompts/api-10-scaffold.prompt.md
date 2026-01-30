---
name: api:scaffold
description: Scaffold the client architecture (transport/auth/serialization/errors/policies) with placeholders wired for tests.
argument-hint: "Target language/runtime, HTTP library preference (if any)."
agent: api-implementer
tools: ["githubRepo", "terminal", "search"]
---
Scaffold the API client architecture with these modules:
- Transport (base URL, headers, timeouts, cancellation)
- Auth provider abstraction
- Serializer abstraction (central JSON options)
- Error mapping (status + payload -> typed errors)
- Policy pipeline hooks (retry/rate-limit classification; implementation may be basic)

Add a minimal smoke test harness that can call a ping/health endpoint (or equivalent).
Do not hardcode secrets; use env vars.
