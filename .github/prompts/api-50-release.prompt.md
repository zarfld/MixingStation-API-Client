---
name: api:release
description: Prepare release checklist (versioning, changelog, compatibility notes, CI gates).
argument-hint: "Target release version and supported API versions."
agent: api-architect
tools: ["githubRepo", "search"]
---
Create a release checklist:
- SemVer decision + breaking change notes
- Compatibility statement (API versions)
- Required CI checks
- Changelog entries
- Known limitations and follow-ups
