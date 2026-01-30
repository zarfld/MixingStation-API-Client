---
name: api:discover
description: Analyze the API contract and produce a coverage matrix + implementation plan.
argument-hint: "Provide OpenAPI path(s) or link(s), plus target language/runtime."
agent: api-architect
tools: ["search", "fetch", "githubRepo"]
---
Use the available API contract (OpenAPI/Swagger or equivalent) and produce:

1) Endpoint inventory grouped by resource
2) Coverage matrix: endpoint -> client method -> tests (integration + negative)
3) Key risks/gaps to validate with the test server (auth, pagination, errors, rate limits)
4) Proposed module layout for the client (transport/auth/serialization/errors/policies/endpoints)

Output as Markdown. Keep it concrete and actionable.
