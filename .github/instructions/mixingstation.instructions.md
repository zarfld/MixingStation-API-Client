---
applyTo: "**/src/**"
---
# Mixing Station API client rules

## Protocols
Mixing Station exposes:
- REST over HTTP (OpenAPI / Swagger available from the built-in API explorer)
- REST over WebSocket using a JSON envelope: { path, method, body }
- Console parameter model with value subscriptions
- Metering subscriptions with JSON or base64/binary payloads

## Required capabilities
- Support both HTTP and WebSocket transports.
- Implement value subscriptions (subscribe sends initial values).
- Implement reconnect + resubscribe (idempotent, bounded backoff).
- Treat normalized (0..1) vs plain values as explicit API surface.

## WebSocket envelope
Requests MUST be sent as:
{ "path": "...", "method": "GET|POST|...", "body": ... }
Responses include:
{ "path": "...", "method": "...", "body": ..., "error": ... }

## Console values
- Use "/console/data/subscribe" for value subscriptions.
- Provide helpers for paths like "ch.*.mix.lvl".
- Do not clamp silently unless the caller opts in.

## Metering
- Implement "/console/metering2/subscribe" and decode both JSON and base64/binary responses.
- Binary decoding: int16 big-endian scaled by 100.

## App state
- Track "/app/state" and use it to drive connection lifecycle.
