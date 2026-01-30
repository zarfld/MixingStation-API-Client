# Risk Register - Phase 1

**Standard**: ISO/IEC/IEEE 42010:2011 (Concerns and Risks)  
**Phase**: 03 - Architecture  
**Date**: 2026-01-30  

## Overview

This risk register identifies potential issues that could impact the successful delivery of Phase 1 (Epic 1: Ingest & Normalize). Risks are categorized by probability and impact, with mitigations defined.

---

## Risk Assessment Matrix

| Probability | Impact | Priority |
|------------|--------|----------|
| High (>50%) | High | 🔴 Critical |
| High | Medium | 🟠 Major |
| High | Low | 🟡 Minor |
| Medium (20-50%) | High | 🟠 Major |
| Medium | Medium | 🟡 Minor |
| Low (<20%) | High | 🟡 Minor |

---

## Top Risks (Phase 1)

### R-001: API Drift (OpenAPI Schema Changes)

**Category**: Technical  
**Probability**: Medium (30%)  
**Impact**: High  
**Priority**: 🟠 Major  

**Description**:
MixingStation API has obfuscated schema names (`blob-*` in OpenAPI spec), which may change across firmware updates. This could break our DTOs and normalization logic.

**Indicators**:
- New firmware released for X32/M32/Wing
- API endpoints return unexpected JSON structures
- Schema validation errors during normalization

**Mitigation**:
1. **API Version Pinning**: Document tested firmware versions (e.g., "Tested with X32 FW 4.06")
2. **Graceful Degradation**: Use "N/A" for unknown fields instead of crashing
3. **Integration Tests**: Run against real console to detect schema changes
4. **CI Smoke Test**: Automated test hitting real MixingStation instance (if available)
5. **ADR-002 Policy**: "Public Interface Mirrors REST" isolates API changes to client layer

**Contingency**:
- If breaking change detected, update DTOs and normalization logic
- Document breaking changes in CHANGELOG with migration guide
- Consider creating API version adapter pattern for multi-version support

---

### R-002: Console Variant Gaps (Unsupported Models)

**Category**: Requirements  
**Probability**: Medium (40%)  
**Impact**: Medium  
**Priority**: 🟡 Minor  

**Description**:
CanonicalModel v0 assumes X32/M32/Wing share similar data structures. Newer consoles (e.g., Wing X, XR18) may have different channel types, routing options, or missing features.

**Indicators**:
- User reports "N/A" for all fields on new console model
- Channel counts mismatch expectations
- Unknown channel types returned by API

**Mitigation**:
1. **Explicit Model Support**: Document supported consoles in README (X32, M32, Wing for Phase 1)
2. **Fallback Defaults**: Use "N/A" for unknown/unsupported features
3. **Extensibility**: CanonicalModel v1 can add console-specific fields
4. **User Feedback**: Collect logs/API responses from unsupported consoles for future support

**Contingency**:
- Add console-specific normalizers (e.g., `X32Normalizer`, `WingNormalizer`)
- Create abstraction: `INormalizerFactory.Create(consoleType)`
- Document unsupported models in README with workaround (manual export)

---

### R-003: Network Reliability (Connection Timeouts)

**Category**: Infrastructure  
**Probability**: Medium (30%)  
**Impact**: Medium  
**Priority**: 🟡 Minor  

**Description**:
MixingStation API is accessed over local network (Wi-Fi or Ethernet). Network issues, console sleep mode, or firewall rules could cause connection failures or timeouts.

**Indicators**:
- Timeout exceptions during API calls
- Intermittent connection failures
- Slow response times (>5 seconds)

**Mitigation**:
1. **Retry Policy**: 3 retries with exponential backoff (2s, 4s, 8s)
2. **Configurable Timeout**: Default 30s, user-configurable via CLI args
3. **Connection Test**: Pre-flight check (`GET /app/status`) before heavy operations
4. **Clear Error Messages**: "Cannot connect to 192.168.1.100:8045 - check console is powered on and network is reachable"
5. **HTTP Client Configuration**: Use IHttpClientFactory with connection pooling

**Contingency**:
- If retries fail, exit with code 1 and clear error message
- Suggest troubleshooting steps (ping console, check firewall, verify MixingStation app running)
- Add `--verbose` flag for detailed HTTP logs

---

### R-004: Excel File Size/Performance (Large Consoles)

**Category**: Performance  
**Probability**: Low (20%)  
**Impact**: Low  
**Priority**: 🟢 Low  

**Description**:
Large consoles (Wing with 128 channels) could generate slow exports (>10s) or large .xlsx files (>5MB).

**Indicators**:
- Export takes >5 seconds for 64 channels
- Excel file size >5MB
- Memory usage spikes during export

**Mitigation**:
1. **Streaming Export**: EPPlus supports streaming writes (avoid loading entire workbook in memory)
2. **Batch Processing**: Write channels in batches (16 at a time)
3. **Performance Test**: CI benchmark against 128-channel mock data (target <5s)
4. **Lazy Loading**: Only fetch required API data (skip unused fields like FX routing)

**Contingency**:
- If >5s, add progress indicator to CLI (`Exporting channel 32/128...`)
- If >10s, optimize by removing auto-column sizing (expensive in EPPlus)
- Consider CSV export as faster alternative for large consoles

---

### R-005: EPPlus Licensing (Commercial Use)

**Category**: Legal  
**Probability**: Low (10%)  
**Impact**: Medium  
**Priority**: 🟡 Minor  

**Description**:
EPPlus 7.x uses Polyform Noncommercial 1.0.0 license. Commercial use requires purchasing a license ($799/developer). Users may unknowingly violate license terms.

**Indicators**:
- User reports using library in commercial project
- Company requests invoice/license compliance audit

**Mitigation**:
1. **License Documentation**: README clearly states EPPlus license restrictions
2. **Commercial License Link**: Provide link to purchase EPPlus commercial license
3. **ClosedXML Alternative**: Keep ClosedXML (MIT, fully open-source) as backup option
4. **ADR-004**: Document licensing decision with trade-offs

**Contingency**:
- If commercial use detected, guide user to purchase EPPlus license
- If license issue becomes blocker, switch to ClosedXML (minor code changes)
- Create `IExcelExporter` abstraction to support multiple backends

---

### R-006: Incomplete Data Normalization (Missing Fields)

**Category**: Requirements  
**Probability**: Medium (40%)  
**Impact**: Low  
**Priority**: 🟡 Minor  

**Description**:
Phase 1 defers some fields (phantom power, gain, aux routing) to "N/A". Users may expect complete data and be confused by missing values.

**Indicators**:
- User reports: "Why is phantom power always N/A?"
- Excel columns filled with "N/A" instead of real data
- Feature requests for missing fields

**Mitigation**:
1. **Documentation**: README explains Phase 1 scope (minimal data, input list + patching only)
2. **Excel Metadata**: Include disclaimer in Row 1: "Phase 1 Export - Some fields show 'N/A' (coming in Phase 2)"
3. **ADR-003**: Document which fields are Phase 1 vs. Phase 2
4. **User Expectations**: Explicitly state in user stories: "N/A for unavailable data"

**Contingency**:
- If demand is high, prioritize Phase 2 features (phantom, gain, aux routing)
- Add `--verbose` flag to show which API calls are being made (transparency)
- Provide roadmap in README (Phase 2: Full channel data)

---

### R-007: WebSocket Support Delay (Phase 2 Dependency)

**Category**: Requirements  
**Probability**: High (60%)  
**Impact**: Low  
**Priority**: 🟡 Minor  

**Description**:
Phase 1 uses HTTP polling (slower, higher latency). Users may expect real-time updates via WebSocket, which is deferred to Phase 2.

**Indicators**:
- User asks: "Why isn't data updating in real-time?"
- Feature request: "Add live monitoring of channel changes"

**Mitigation**:
1. **Documentation**: README clearly states Phase 1 is snapshot-based (not real-time)
2. **Phase Roadmap**: Outline Phase 2 WebSocket support timeline
3. **Polling Option**: Future: Add `--poll` flag for periodic snapshots (every 5s)
4. **ADR-002**: Document HTTP-only Phase 1, WebSocket in Phase 2

**Contingency**:
- If demand is high, prioritize Phase 2 WebSocket implementation
- Add experimental `--watch` mode using HTTP polling (simple refresh loop)

---

### R-008: Stereo Pair Detection Accuracy

**Category**: Technical  
**Probability**: Medium (30%)  
**Impact**: Low  
**Priority**: 🟡 Minor  

**Description**:
Stereo pair detection relies on API flags or naming conventions (e.g., "Bass L", "Bass R"). May incorrectly identify stereo channels or miss valid pairs.

**Indicators**:
- Excel shows incorrect stereo grouping (e.g., "Ch 1" and "Ch 2" marked as stereo but are independent)
- Missed stereo pairs (e.g., "Kick L" and "Kick R" not detected)

**Mitigation**:
1. **Heuristic Fallback**: Check both API flag (`config.link`) AND naming pattern (`(L)`, `(R)`)
2. **User Override**: Future: Add `--ignore-stereo` CLI flag to disable detection
3. **Test Cases**: Unit tests with various stereo naming conventions
4. **Documentation**: Explain stereo detection logic in data-model.md

**Contingency**:
- If detection is unreliable, make it opt-in via `--detect-stereo` flag (default off)
- Provide manual correction workflow (edit Excel after export)

---

### R-009: CI/CD Pipeline Dependency on Live Console

**Category**: Infrastructure  
**Probability**: Medium (40%)  
**Impact**: Medium  
**Priority**: 🟡 Minor  

**Description**:
Integration tests require a live MixingStation console (or mock server). CI/CD may not have access to physical hardware, limiting automated testing.

**Indicators**:
- Integration tests skipped in CI (no console available)
- Unable to validate API contract changes automatically

**Mitigation**:
1. **Mock Server**: Create mock MixingStation API server for CI (returns canned responses)
2. **Docker Container**: Package mock server as Docker image (run in CI)
3. **Optional Tests**: Mark integration tests as `[Trait("Category", "Integration")]` (skipped by default)
4. **Manual Testing**: Provide manual test checklist for pre-release validation

**Contingency**:
- If CI tests blocked, prioritize mock server development
- Use Postman collections to document API contract (manual verification)
- Set up nightly builds against real console (if available in office/studio)

---

### R-010: Data Privacy (Exporting Sensitive Channel Names)

**Category**: Security  
**Probability**: Low (10%)  
**Impact**: Low  
**Priority**: 🟢 Low  

**Description**:
Excel exports may contain sensitive information (e.g., artist names, venue details in channel labels). Users may inadvertently share files with unauthorized parties.

**Indicators**:
- User reports: "Can I anonymize channel names?"
- Privacy compliance concerns (GDPR, venue confidentiality)

**Mitigation**:
1. **Documentation**: README warns users to review exports before sharing
2. **Redaction Option**: Future: Add `--redact-names` flag (replaces names with "Ch 1", "Ch 2", etc.)
3. **Disclaimer**: Excel metadata includes: "For internal use only - do not distribute"

**Contingency**:
- If demand arises, add privacy features (redact names, strip metadata)
- Provide guidance on secure file handling

---

## Risk Monitoring and Review

### Review Cadence

| Phase | Review Frequency |
|-------|-----------------|
| Phase 1 (Implementation) | Weekly |
| Phase 2+ | Monthly |
| Post-Release | Quarterly |

### Risk Owners

| Risk ID | Owner | Review Date |
|---------|-------|-------------|
| R-001 (API Drift) | @zarfld | 2026-02-15 |
| R-002 (Variant Gaps) | @zarfld | 2026-02-15 |
| R-003 (Network Reliability) | @zarfld | 2026-02-15 |
| R-005 (EPPlus Licensing) | @zarfld | 2026-02-15 |

---

## Emerging Risks (Watch List)

| Risk | Probability | Impact | Monitor |
|------|------------|--------|---------|
| MixingStation API deprecation | Low (5%) | High | Check MixingStation forum monthly |
| .NET 8 EOL (Nov 2026) | Low (5%) | Medium | Plan .NET 9 migration by Q3 2026 |
| Breaking changes in EPPlus 8.x | Low (10%) | Low | Pin EPPlus version in .csproj |

---

## Traceability

| Risk | Mitigated By | Verified By |
|------|-------------|-------------|
| R-001 (API Drift) | ADR-002 (REST Mirror Policy), Integration Tests | TEST-INTEGRATION-001 |
| R-002 (Variant Gaps) | ADR-003 (CanonicalModel v0), "N/A" handling | TEST-NORMALIZE-VARIANTS |
| R-003 (Network Reliability) | IHttpClientFactory retry policy | TEST-RESILIENCE-001 |
| R-005 (EPPlus Licensing) | ADR-004 (Excel Export Library), README | License documentation |

---

## Standards Compliance

- ✅ **ISO/IEC/IEEE 42010:2011**: Risk concerns and architecture trade-offs
- ✅ **ISO/IEC/IEEE 12207:2017**: Risk management in software lifecycle

---

**Next Phase**: Phase 05 - Implementation (TDD with risk-driven test cases)
