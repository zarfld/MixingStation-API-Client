---
applyTo: "**/*"
---
# General coding standards

- Prefer small, composable functions; avoid “god clients”.
- Keep side effects isolated (HTTP, time, randomness).
- Validate inputs at the boundary; fail fast with actionable messages.
- Prefer explicit types for public APIs; avoid ambiguous return values.
- Write code that is friendly to test harnesses (dependency injection where relevant).
