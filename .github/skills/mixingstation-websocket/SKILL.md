---
name: mixingstation-websocket
description: Use for implementing REST-over-WebSocket envelope, request/reply correlation, and push updates.
---
# Mixing Station WebSocket Skill

## Implement
- Client connects to Mixing Station WS endpoint
- Send requests as { path, method, body }
- Correlate replies (path+method+optional requestId if you add one client-side)
- Route unsolicited push updates:
  - /app/state updates
  - /console/metering2/{id}
  - subscribed console values

## Reliability
- Reconnect on socket close
- Resubscribe after reconnect
- Bound retries and total reconnect time (configurable)
