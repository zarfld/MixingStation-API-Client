---
name: mixingstation-console-values
description: Use for /console/data/subscribe and /con/[vn] value semantics (plain vs normalized).
---
# Mixing Station Console Values Skill

## Core concepts
- Parameter paths like: "ch.0.mix.lvl" or wildcard "ch.*.mix.lvl"
- Two formats:
  - Plain values (e.g. dB/Hz)
  - Normalized values in [0..1]

## Implement
- Subscribe endpoint: POST /console/data/subscribe with { path, format }
- Provide get/set helpers for both formats
- Expose an observable stream of updates:
  - (path, value, format, timestamp)
