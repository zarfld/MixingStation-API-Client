# MixingStation API Client (.NET 8)

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-TBD-green.svg)](LICENSE)
[![Phase](https://img.shields.io/badge/phase-05%20Implementation-yellow.svg)](05-implementation/README.md)

C# client library and CLI tool for MixingStation REST API. Automates export of console configurations to Excel.

## Quick Start

```powershell
# Build solution
dotnet build

# Run tests (after implementation)
dotnet test

# Run CLI tool (after implementation)
dotnet run --project examples/exportlists -- --output ./output
```

## Project Status

### ✅ Completed Phases

- **Phase 01**: Stakeholder Requirements ([Issues #1-3](https://github.com/zarfld/MixingStation-API-Client/issues))
- **Phase 02**: Functional Requirements ([Issues #4-9](https://github.com/zarfld/MixingStation-API-Client/issues))
- **Phase 03**: Architecture ([Issues #10-13](https://github.com/zarfld/MixingStation-API-Client/issues), [C4 Diagrams](03-architecture/views/c4.md))
- **Phase 04**: Detailed Design ([Design Doc](04-design/phase-1-detailed-design.md))
- **Phase 05**: Scaffolding ✅ **CURRENT** (see [05-implementation/README.md](05-implementation/README.md))

### ⏳ Next Steps (TDD Required)

1. Implement HTTP transport layer → `AppClient.cs`
2. Implement console client → `ConsoleClient.cs`
3. Implement normalizer → `Normalizer.cs`
4. Implement Excel exporter → `ExcelExporter.cs`
5. Implement CLI tool → `Program.cs`

## Architecture

### Design Principle: "Public Interface Mirrors REST"

Per [ADR-002](https://github.com/zarfld/MixingStation-API-Client/issues/11), all public client methods mirror REST endpoint names exactly:

| REST Endpoint | Derived Method | Interface |
|---------------|----------------|-----------|
| `POST /app/connect` | `ConnectAsync()` | IAppClient |
| `GET /app/mixers/current` | `MixersCurrentAsync()` | IAppClient |
| `GET /console/data` | `DataAsync()` | IConsoleClient |

**CI-enforced** via `scripts/validate-api-naming.py`

### CanonicalModel v0

Per [ADR-003](https://github.com/zarfld/MixingStation-API-Client/issues/12):
- 1-based channel numbering (user-friendly)
- Explicit "N/A" for unset values
- Bidirectional stereo pair links
- Color mapping: API integers → hex codes

## Requirements Traceability

All code references GitHub Issues for full traceability:

**Stakeholder Requirements**:
- [#1](https://github.com/zarfld/MixingStation-API-Client/issues/1) - Epic 1: Ingest & Normalize

**Functional Requirements**:
- [#4](https://github.com/zarfld/MixingStation-API-Client/issues/4) - HTTP Client Transport
- [#5](https://github.com/zarfld/MixingStation-API-Client/issues/5) - Application State Reading
- [#6](https://github.com/zarfld/MixingStation-API-Client/issues/6) - Console Data Reading
- [#7](https://github.com/zarfld/MixingStation-API-Client/issues/7) - Data Normalization
- [#8](https://github.com/zarfld/MixingStation-API-Client/issues/8) - Excel Export
- [#9](https://github.com/zarfld/MixingStation-API-Client/issues/9) - CLI Tool

**Architecture Decisions**:
- [#10](https://github.com/zarfld/MixingStation-API-Client/issues/10) - .NET 8 Runtime
- [#11](https://github.com/zarfld/MixingStation-API-Client/issues/11) - HTTP Transport + REST Mirror Naming
- [#12](https://github.com/zarfld/MixingStation-API-Client/issues/12) - CanonicalModel v0
- [#13](https://github.com/zarfld/MixingStation-API-Client/issues/13) - Excel Export Library

## Development Workflow (XP Practices)

### Test-Driven Development (TDD)

**CRITICAL**: Write tests BEFORE implementation

```
🔴 RED: Write failing test
🟢 GREEN: Write minimal code to pass
🔵 REFACTOR: Improve design (tests stay green)
```

### Quality Gates

- ✅ All tests pass
- ✅ Coverage ≥ 70% (target 80%)
- ✅ REST naming compliance (CI)
- ✅ Code reviewed

## Standards Compliance

Following IEEE/ISO/IEC standards and XP practices:

- **ISO/IEC/IEEE 12207:2017**: Software lifecycle processes
- **ISO/IEC/IEEE 29148:2018**: Requirements engineering
- **ISO/IEC/IEEE 42010:2011**: Architecture description
- **IEEE 1016-2009**: Software design descriptions
- **XP Practices**: TDD, Continuous Integration, Simple Design, Refactoring

## Resources

- **[Phase 04 Design](04-design/phase-1-detailed-design.md)** - Complete design specification
- **[Phase 05 Status](05-implementation/README.md)** - Implementation progress
- **[Architecture Decisions](03-architecture/decisions/)** - ADRs
- **[GitHub Issues](https://github.com/zarfld/MixingStation-API-Client/issues)** - Requirements tracking

## License

TBD - See [LICENSE](LICENSE) file

---

**Philosophy**: "Slow is Fast" + "No Excuses" + "No Shortcuts" + "Clarify First"

- Write tests first (TDD) - prevents rework
- REST naming policy CI-enforced
- Phase gates prevent technical debt
- Full traceability: requirements → design → code → tests
