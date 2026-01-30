---
name: api:docs
description: Generate or update developer documentation for the client (setup/auth/errors/pagination/examples).
argument-hint: "Which modules/endpoints changed?"
agent: api-docs
tools: ["githubRepo", "search"]
---
Update documentation for the API client:
- Quickstart (install/config/base URL/auth)
- Examples for the changed endpoints
- Error handling guide
- Pagination/rate limits notes
- Version compatibility notes
