---
applyTo: "**/src/**"
---
# API client implementation rules

## Public surface
- Public methods must be stable, named by domain intent, not by raw endpoint paths.
- Do not expose raw HTTP primitives (headers, status codes) unless explicitly intended.

## Transport layer
- Centralize: base URL, default headers, auth injection, timeouts, retry policy.
- Provide hooks for logging/tracing without forcing a logging framework.

## Serialization
- Centralize JSON options (date/time, enums, null handling).
- Ensure forward compatibility: tolerate unknown fields where possible.

## Errors
- Map errors deterministically:
  - 4xx: typed client errors (validation/auth/permission/not found/conflict).
  - 5xx/429/timeouts: transient errors suitable for retry (policy-driven).
