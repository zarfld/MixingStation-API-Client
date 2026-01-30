---
name: auth-and-secrets
description: Use when implementing authentication flows (bearer tokens, OAuth, API keys) and handling secrets safely in code/tests/CI.
---
# Auth and Secrets Skill

## Non-negotiables
- Never commit secrets.
- Never print secrets in logs/test output.
- Prefer short-lived tokens when possible.

## Implementation
- Centralize auth header injection in the transport layer.
- If refresh tokens exist: implement refresh with locking to avoid stampede.
- Provide a redaction utility for logs.

## Testing
- Use env var injection locally and in CI.
- Provide a “no-auth” negative test if the API supports it.
