---
name: api-client-tdd
description: Use when building integration/contract tests against a reachable API server, including environment-based configuration and deterministic test patterns.
---
# API Client TDD Skill

## Setup pattern
- Base URL from env var (e.g., API_BASE_URL)
- Auth token/credentials from env var (e.g., API_TOKEN)
- Fail fast if missing.

## Test pattern (per endpoint slice)
1) Arrange: prepare unique resource identifiers.
2) Act: call client method.
3) Assert: status-independent assertions (fields, invariants).
4) Cleanup: delete created resources if applicable.

## Negative-path coverage
At least one of:
- Missing/invalid auth
- Invalid input
- Not found
- Conflict
