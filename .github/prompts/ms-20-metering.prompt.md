---
name: ms:metering
description: Implement metering2 subscription and JSON/binary decoding with tests.
agent: Mixing Station Protocol Engineer
tools: ["githubRepo", "terminal", "search"]
---
Implement:
- POST /console/metering2/subscribe
- Receive frames at /console/metering2/{id}
- Decode JSON and binary(base64) formats into a stable metering model
Add tests for:
- JSON decode shape
- Binary decode correctness (int16 big-endian, scaled by 100)
