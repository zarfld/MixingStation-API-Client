---
name: mixingstation-metering
description: Use for /console/metering2/subscribe and decoding JSON vs base64/binary.
---
# Mixing Station Metering Skill

## Implement
- POST /console/metering2/subscribe with { id, interval, binary, params[] }
- Receive frames on /console/metering2/{id}

## Decode
- JSON: body.v is nested arrays; preserve channel ordering.
- Binary: body.b is base64 (non-padded); decode to int16 big-endian; scale/100 -> dB float.
