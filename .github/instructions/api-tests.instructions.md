---
applyTo: "**/tests/**,**/*test*/**,**/*spec*/**"
---
# API client test rules

- Each endpoint group requires:
  - One happy-path integration test
  - One negative-path test
- Prefer contract assertions: schema shape + required fields.
- Keep tests hermetic where possible (unique resource names; cleanup).
- Avoid sleeps; poll with bounded timeout if eventual consistency is expected.
