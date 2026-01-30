# Copilot Instructions (Repository-wide)

You are helping implement a REST API client against an existing API contract.

## Primary goals
- Correctness against the API contract (spec + observed server behavior).
- Maintainable architecture: transport/auth/serialization/error/policy are separate concerns.
- Strong tests: contract + integration + unit, with deterministic assertions.

## Non-negotiables
- Always add timeouts and cancellation support for HTTP calls.
- Centralize error handling: map status + error payload to typed errors/exceptions.
- Never log secrets (tokens, auth headers, passwords).
- Avoid leaking generated DTOs in the public API surface.

## Implementation approach
- Prefer a “thin generated layer + handwritten façade”:
  - Handwritten: public client API, domain naming, convenience overloads.
  - Internal: low-level request builder, raw DTOs if generated.
- Add endpoints in slices: implement + tests + docs together.

## Testing
- Use the reachable test server for integration tests.
- Tests must not depend on ordering; clean up created resources.
- If the server is flaky/rate-limited, use retry only in the client (not in assertions).

## Documentation
- Every public method must document:
  - required auth scopes/roles (if known),
  - expected errors,
  - pagination/rate-limit behavior,
  - idempotency expectations.
