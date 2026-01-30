# Phase 03: Architecture Design

**Phase**: Architecture Design  
**Standard**: ISO/IEC/IEEE 42010:2011  
**Status**: In Progress  
**Date**: 2026-01-30  

## Scope

Define the software architecture for MixingStation API Client (.NET 8 C# library) targeting Phase 1 Epic 1: Ingest & Normalize with vertical slice: Connect → Read → Normalize → Export to Excel.

### Objectives

1. **Establish architectural boundaries** between REST mirror (MixingStation.Client) and workflow helpers (MixingStation.Workflows)
2. **Define public API contracts** that mirror REST endpoints (enforced naming policy)
3. **Design CanonicalModel v0** normalized data structure for console-agnostic mixer representation
4. **Select and configure** HTTP transport (HttpClient) and Excel export libraries
5. **Document key decisions** in ADRs with traceability to requirements

### Goals (Phase 1 Only)

- ✅ **Minimal Viable Architecture**: Only what's needed for Epic 1 vertical slice
- ✅ **Future-Proofing**: Namespace structure supports Phase 2 (WebSocket, Workflows)
- ✅ **Standards Compliance**: IEEE 42010 architecture description, ADRs trace to requirements
- ✅ **Testability**: All components injectable, interfaces over concrete classes

### Constraints

| Constraint | Impact |
|------------|--------|
| **No third-party HTTP libs** | Must use System.Net.Http.HttpClient |
| **"Public Interface Mirrors REST" policy** | All public methods match REST paths/verbs |
| **.NET 8 LTS target** | No preview features, must run on .NET 8.0+ |
| **EPPlus licensing** | Non-commercial use or purchase license |
| **No WebSocket in Phase 1** | HTTP polling only (accept latency trade-off) |

## Architecture Decisions (ADRs)

| ADR | Title | Status | Traces To |
|-----|-------|--------|-----------|
| [ADR-001](#10) | .NET 8 Runtime and Packaging Strategy | Proposed | StR-002 |
| [ADR-002](#11) | HTTP Transport + REST Mirror Naming Policy | Proposed | StR-002, REQ-F-001 |
| [ADR-003](#12) | CanonicalModel v0 - Normalized Data Structure | Proposed | StR-001, REQ-F-004 |
| [ADR-004](#13) | Excel Export Library and Formatting Standards | Proposed | StR-001, REQ-F-005 |

## Architecture Documentation

| Document | Description | Status |
|----------|-------------|--------|
| [C4 Diagrams](c4.md) | Context, Container, Component views | ✅ Complete |
| [Public Interfaces](interfaces.md) | API contracts and boundaries | ✅ Complete |
| [Data Model](data-model.md) | CanonicalModel v0 schema | ✅ Complete |
| [Risk Register](risk-register.md) | Top risks and mitigations | ✅ Complete |

## Phase Plan (Definition of Ready Gate)

### Phase 03 Entry Criteria (✅ Met)

- ✅ Phase 02 complete: All REQ-F issues created (#4-9)
- ✅ Requirements traced to stakeholder needs
- ✅ Acceptance criteria defined for all user stories

### Phase 03 Exit Criteria (Gate Before Implementation)

- ✅ All ADRs created and linked to requirements
- ✅ C4 diagrams (Context, Container, Component) complete
- ✅ Public interfaces documented with REST mapping
- ✅ CanonicalModel v0 schema defined
- ✅ Risk register populated with top risks
- ✅ Architecture reviewed and approved by @zarfld

**Gate Status**: ⏳ In Progress

## Key Architectural Patterns

| Pattern | Usage | Rationale |
|---------|-------|-----------|
| **Repository** | Data access abstraction | IHttpClientFactory wraps HTTP I/O |
| **Adapter** | API → CanonicalModel transformation | Isolate API changes from domain model |
| **Factory** | HttpClient creation | IHttpClientFactory for connection pooling |
| **Strategy** | Excel export formats | Future: CSV, Google Sheets exporters |
| **Clean Architecture** | Layers: Client → Normalizer → Exporter | Domain logic independent of I/O |

## Quality Attributes (Non-Functional)

| Attribute | Scenario | Requirement |
|-----------|----------|-------------|
| **Performance** | Export 64 channels to Excel | <5 seconds |
| **Reliability** | Network timeout handling | Retry with backoff, fail gracefully |
| **Maintainability** | API endpoint name change | Public method name must match (enforced by CI) |
| **Testability** | Unit test coverage | >70% (target 80%) |
| **Extensibility** | Add new export format (CSV) | Implement `IExporter` interface |

## Traceability

### Requirements Coverage

| Requirement | Architecture Element | Verification |
|-------------|---------------------|--------------|
| REQ-F-001 (HTTP Transport) | `MixingStation.Client` namespace, HttpClient | ADR-002 |
| REQ-F-002 (App State) | `AppClient.GetMixersCurrentAsync()` | interfaces.md |
| REQ-F-003 (Console Data) | `ConsoleClient.GetDataAsync()` | interfaces.md |
| REQ-F-004 (Normalization) | `CanonicalModel`, `INormalizer` | ADR-003, data-model.md |
| REQ-F-005 (Excel Export) | `IExcelExporter`, EPPlus | ADR-004 |
| REQ-F-006 (CLI Tool) | `examples/exportlists` project | c4.md (Container) |

## Standards Compliance

- ✅ **ISO/IEC/IEEE 42010:2011**: Architecture description with stakeholders, views, concerns
- ✅ **ISO/IEC/IEEE 12207:2017**: Software lifecycle processes (Phase 03 Architecture Design)
- ✅ **IEEE 1016-2009** (upcoming): Detailed design will reference this architecture

## References

- **Stakeholder Requirements**: [01-stakeholder-requirements/](../01-stakeholder-requirements/)
- **Functional Requirements**: [02-requirements/](../02-requirements/)
- **GitHub Issues**: [#1](https://github.com/zarfld/MixingStation-API-Client/issues/1) (StR-001), [#2](https://github.com/zarfld/MixingStation-API-Client/issues/2) (StR-002), [#3](https://github.com/zarfld/MixingStation-API-Client/issues/3) (StR-003)
- **MixingStation API**: http://localhost:8045/ (OpenAPI spec available)

---

**Next Phase**: Phase 04 - Detailed Design → Phase 05 - Implementation (TDD with Red-Green-Refactor)
