# MixingStation.Client Integration Tests

Integration tests that test the **REAL** MixingStation API at http://localhost:8045.

## Requirements

1. **MixingStation app must be running** on http://localhost:8045
2. For full test coverage, a **mixer must be connected**
3. Some tests are skipped if mixer is not connected (graceful degradation)

## Running Integration Tests

### Run ALL integration tests:
```powershell
dotnet test --filter "Category=Integration"
```

### Run by phase:
```powershell
# Phase 1 only
dotnet test --filter "Category=Integration&Phase=1"

# Phase 9 (network tests - work without mixer)
dotnet test --filter "Category=Integration&Phase=9"
```

### Run unit tests ONLY (exclude integration):
```powershell
dotnet test --filter "Category!=Integration"
```

### Run ALL tests (unit + integration):
```powershell
dotnet test
```

## Test Categories

### ✅ Tests that work WITHOUT mixer:
- **Phase 1**: App state, current mixer (limited data)
- **Phase 3**: Mixer discovery, search (may return empty results)
- **Phase 9**: Network interfaces, save settings

### ⚠️ Tests that REQUIRE connected mixer:
- **Phase 2**: Console data subscription
- **Phase 4**: Data discovery (categories, paths, definitions)
- **Phase 5**: Authentication, mix targets
- **Phase 6**: Metering
- **Phase 7**: Presets
- **Phase 8**: IDCA operations
- **Phase 10**: Config change events

## Test Structure

```
IntegrationTests/
├── IntegrationTestBase.cs          # Base class with real HttpClient
├── App/
│   ├── AppClientPhase1IntegrationTests.cs    # 3 endpoints
│   ├── AppClientPhase3IntegrationTests.cs    # 5 endpoints
│   ├── AppClientPhase7IntegrationTests.cs    # 6 endpoints
│   ├── AppClientPhase8IntegrationTests.cs    # 6 endpoints
│   └── AppClientPhase9IntegrationTests.cs    # 3 endpoints
└── Console/
    ├── ConsoleClientPhase2IntegrationTests.cs   # 3 endpoints
    ├── ConsoleClientPhase4IntegrationTests.cs   # 6 endpoints
    ├── ConsoleClientPhase5IntegrationTests.cs   # 3 endpoints
    ├── ConsoleClientPhase6IntegrationTests.cs   # 3 endpoints
    └── ConsoleClientPhase10IntegrationTests.cs  # 1 endpoint
```

## Expected Results

### Without Mixer Connected:
- Phase 1: 1-2 tests pass, 1 skipped
- Phase 3: 3-4 tests pass (search returns empty)
- Phase 9: All 3 tests pass
- Phases 2,4,5,6,7,8,10: Most tests skipped

### With Mixer Connected:
- All phases should have passing tests
- Some operations may still require specific mixer features

## Troubleshooting

### "MixingStation server is not running"
```powershell
# Start MixingStation app
# Verify it's running:
curl http://localhost:8045/app/state
```

### "No mixer connected"
```powershell
# Connect a mixer via MixingStation app UI
# Or run tests without mixer (some will be skipped)
```

### HTTP 400 errors
- Most endpoints require an active mixer connection
- Check MixingStation app UI - is a mixer connected?

## CI/CD Integration

Add to CI pipeline:
```yaml
# Only run if MixingStation is available
- name: Integration Tests
  run: dotnet test --filter "Category=Integration"
  continue-on-error: true  # Don't fail build if server not available
```

## Comparison: Unit vs Integration Tests

| Aspect | Unit Tests | Integration Tests |
|--------|-----------|-------------------|
| HTTP calls | Mocked | Real |
| Server required | No | Yes |
| Mixer required | No | For most tests |
| Speed | Fast (~1s) | Slower (~10-30s) |
| Coverage | 100% code paths | Real API behavior |
| Run frequency | Every commit | Before release |
| Test count | 141 tests | ~30-40 tests |

## Best Practices

1. **Run unit tests during development** (fast feedback)
2. **Run integration tests before commits** (verify real API)
3. **Run full suite before releases** (complete validation)
4. **Use `Skip.If()` for graceful degradation** (tests adapt to available resources)
5. **Document mixer requirements** in test comments

---

**Total Endpoints**: 40  
**Unit Tests**: 141  
**Integration Tests**: ~30-40 (some conditionally skipped)  
**Coverage**: 100% of API surface area
