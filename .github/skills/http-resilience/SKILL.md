---
name: http-resilience
description: Use when implementing timeouts, retries, backoff/jitter, rate-limit handling (429), and transient error classification in the client.
---
# HTTP Resilience Skill

## When to use
- You see flaky integration tests due to 429/5xx/timeouts.
- You are implementing the policy pipeline (retry/backoff/circuit breaker).

## Rules of thumb
- Always apply a request timeout (and allow caller override).
- Retry only idempotent requests by default (GET/HEAD/PUT with idempotency key if supported).
- Respect Retry-After headers when present.
- Bound retries by max attempts and total elapsed time.

## Deliverables
- A single policy module implementing:
  - transient classification (timeouts, network errors, 5xx, 429)
  - backoff with jitter
  - rate-limit compliance
  - observability hooks (events/counters)
