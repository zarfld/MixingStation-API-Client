# Phase 05: Implementation

**Status**: In Progress  
**Started**: 2026-01-30  
**Standards**: ISO/IEC/IEEE 12207:2017 (Implementation Process)  
**XP Practices**: Test-Driven Development (TDD), Continuous Integration, Refactoring

## Overview

Implementation of Phase 1 (Epic 1) vertical slice: Connect → Read → Normalize → Export.

## Traceability

**Design**: [Phase 1 Detailed Design](../04-design/phase-1-detailed-design.md)

**Requirements**:
- #4 REQ-F-001: HTTP Client Transport
- #5 REQ-F-002: Application State Reading
- #6 REQ-F-003: Console Data Reading
- #7 REQ-F-004: Data Normalization
- #8 REQ-F-005: Excel Export
- #9 REQ-F-006: CLI Tool

**Architecture**:
- #10 ADR-001: .NET 8 Runtime
- #11 ADR-002: HTTP Transport + REST Mirror Naming
- #12 ADR-003: CanonicalModel v0
- #13 ADR-004: Excel Export Library

## Project Structure

```
src/MixingStation.Client/          # Class library (.NET 8)
├── App/
│   ├── IAppClient.cs              # ✅ Created (interface)
│   └── AppClient.cs               # ⏳ TODO: TDD implementation
├── Console/
│   ├── IConsoleClient.cs          # ✅ Created (interface)
│   └── ConsoleClient.cs           # ⏳ TODO: TDD implementation
├── Models/
│   ├── CanonicalModel.cs          # ✅ Created (v0 schema)
│   └── ApiModels.cs               # ✅ Created (DTOs)
├── Normalization/
│   ├── INormalizer.cs             # ✅ Created (interface)
│   └── Normalizer.cs              # ⏳ TODO: TDD implementation
├── Export/
│   ├── IExcelExporter.cs          # ✅ Created (interface)
│   └── ExcelExporter.cs           # ⏳ TODO: TDD implementation
└── Exceptions/
    └── MixingStationExceptions.cs # ✅ Created (hierarchy)

examples/exportlists/              # Console app (.NET 8)
└── Program.cs                     # ⏳ TODO: CLI implementation

tests/MixingStation.Client.Tests/  # xUnit test project
├── App/
│   └── AppClientTests.cs          # ⏳ TODO: TDD (write first!)
├── Console/
│   └── ConsoleClientTests.cs      # ⏳ TODO: TDD (write first!)
├── Normalization/
│   └── NormalizerTests.cs         # ⏳ TODO: TDD (write first!)
├── Export/
│   └── ExcelExporterTests.cs      # ⏳ TODO: TDD (write first!)
└── Integration/
    └── EndToEndTests.cs           # ⏳ TODO: After unit tests
```

## Implementation Status

### ✅ Phase 05 Entry (Scaffolding Complete)

- [x] Solution structure created (`MixingStation.sln`)
- [x] Class library project (`src/MixingStation.Client`)
- [x] CLI project (`examples/exportlists`)
- [x] Test project (`tests/MixingStation.Client.Tests`)
- [x] NuGet packages added:
  - Microsoft.Extensions.Http (IHttpClientFactory)
  - EPPlus 8.4.1 (Excel export)
  - xUnit (testing)
  - Moq (mocking)
  - FluentAssertions (readable assertions)
- [x] Folder structure per design
- [x] All interfaces defined with traceability headers
- [x] Model classes (CanonicalModel, DTOs)
- [x] Exception hierarchy

### ⏳ Phase 05 Implementation (TDD Required)

**Next Steps** (Red-Green-Refactor):

1. **HTTP Transport Layer** (#4 REQ-F-001)
   - [ ] Write `AppClientTests.cs` (RED)
   - [ ] Implement `AppClient.cs` (GREEN)
   - [ ] Refactor (BLUE)

2. **Console Client** (#6 REQ-F-003)
   - [ ] Write `ConsoleClientTests.cs` (RED)
   - [ ] Implement `ConsoleClient.cs` (GREEN)
   - [ ] Refactor (BLUE)

3. **Normalizer** (#7 REQ-F-004)
   - [ ] Write `NormalizerTests.cs` (RED)
   - [ ] Implement `Normalizer.cs` (GREEN)
   - [ ] Refactor (BLUE)

4. **Excel Exporter** (#8 REQ-F-005)
   - [ ] Write `ExcelExporterTests.cs` (RED)
   - [ ] Implement `ExcelExporter.cs` (GREEN)
   - [ ] Refactor (BLUE)

5. **CLI Tool** (#9 REQ-F-006)
   - [ ] Write `EndToEndTests.cs` (RED)
   - [ ] Implement `Program.cs` (GREEN)
   - [ ] Refactor (BLUE)

## XP Practices

### Test-Driven Development (TDD)

**CRITICAL RULE**: Write tests BEFORE implementation (no exceptions)

**Red-Green-Refactor Cycle**:
```
🔴 RED: Write failing test
  ↓
🟢 GREEN: Write minimal code to pass
  ↓
🔵 REFACTOR: Improve design (tests stay green)
  ↓
Repeat
```

### Continuous Integration

- Integrate multiple times per day
- Run all tests before committing
- Fix broken builds immediately (<10 minutes)
- CI validates:
  - All tests pass
  - Coverage ≥ 70% (target 80%)
  - REST API naming compliance (via `scripts/validate-api-naming.py`)

### Coding Standards

- Follow C# conventions (PascalCase, camelCase)
- Use nullable reference types
- XML documentation for public APIs
- Async/await for I/O operations
- Structured logging

## Quality Gates

**Exit Criteria** (Phase 05 → Phase 06):
- [ ] All code implemented per design
- [ ] Unit tests written for all code (TDD)
- [ ] Test coverage ≥ 70% (target 80%)
- [ ] All tests passing
- [ ] Code reviewed and approved
- [ ] No critical bugs
- [ ] Documentation updated
- [ ] REST naming compliance verified (CI)
- [ ] Traceability established (code → design → requirements)

## Commands

### Build Solution
```powershell
dotnet build
```

### Run Tests
```powershell
dotnet test
```

### Run Tests with Coverage
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Run CLI Tool (after implementation)
```powershell
dotnet run --project examples/exportlists -- --output ./output
```

### Validate REST Naming (CI check)
```powershell
python scripts/validate-api-naming.py
```

---

**Next Action**: Start TDD implementation (write first test for AppClient)

**Remember**: "Slow is fast" - Tests first prevent rework later!
