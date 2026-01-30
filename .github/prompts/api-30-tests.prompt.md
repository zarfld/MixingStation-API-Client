---
name: api:tests
description: Strengthen contract/integration tests and stabilize flaky behavior (429/5xx/timeouts).
argument-hint: "Paste failing test output or describe instability."
agent: api-test-engineer
tools: ["githubRepo", "terminal", "search"]
---
Given the current client and failing/flaky tests:
1) Diagnose the cause (rate limits, timeouts, retries, eventual consistency).
2) Propose and implement test fixes that preserve meaningful assertions.
3) If needed, propose client resilience improvements (but keep tests deterministic).

Output: root cause + concrete changes.
