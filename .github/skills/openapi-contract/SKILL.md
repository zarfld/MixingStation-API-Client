---
name: openapi-contract
description: Use when you need to translate an OpenAPI/Swagger contract into a coverage matrix, typed models, and endpoint slices.
---
# OpenAPI Contract Skill

## When to use
Use this skill when:
- The API spec exists and you need implementation structure.
- You need a coverage matrix (endpoints -> tests -> methods).
- You need to identify spec gaps (auth, errors, pagination, rate limits).

## Procedure
1) Identify the spec source (OpenAPI JSON/YAML or equivalent).
2) Extract:
   - auth schemes
   - base paths and versions
   - endpoints grouped by tags/resources
   - request/response schemas and error schemas
3) Produce:
   - Coverage matrix template
   - DTO/model mapping strategy
   - “vertical slice” implementation order (start with read-only + auth)

## Output format
- Markdown tables for coverage matrix
- A prioritized endpoint slice list
- A list of spec ambiguities to validate via integration tests
