# Integration Test Findings & Issues

**Date**: 2025-01-31  
**Server**: http://localhost:8045  
**Mixer**: StudioLive 32SC (ConsoleId: 18, IP: 157.247.3.12)

## Overview

This document tracks real bugs and issues discovered during integration testing against the live MixingStation API server.

## Key Findings

### 1. ConsoleDataCategoriesResponse DTO Mismatch ✅ FIXED

**Issue**: DTO used `Dictionary<string, object?>` but API returns hierarchical array structure.

**API Response**:
```json
{
  "categories": [
    {
      "id": 1,
      "name": "Channel",
      "description": "All channels",
      "children": [
        { "id": 10, "name": "Gain", "description": "...", "children": [] },
        ...
      ]
    },
    ...
  ]
}
```

**Root Cause**: OpenAPI spec blob-bx-c marked as "empty object - dynamic content" was incomplete/misleading.

**Fix Applied**:
```csharp
// OLD (WRONG):
public record ConsoleDataCategoriesResponse
{
    public Dictionary<string, object?> Categories { get; init; }
}

// NEW (CORRECT):
public record ConsoleDataCategoriesResponse
{
    public DataCategory[] Categories { get; init; }
}

public record DataCategory
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public DataCategory[] Children { get; init; } // Recursive
}
```

**Status**: ✅ FIXED - Test `GetDataCategoriesAsync_RequiresMixer` now PASSES

---

### 2. Empty Response from /app/mixers/current When No Mixer Connected

**Issue**: Endpoint returns **empty body (204 No Content-style)** when no mixer is connected, but C# tries to deserialize it.

**Error**:
```
TransportException: JSON deserialization error: The input does not contain any JSON tokens.
Expected the input to start with a valid JSON token, when isFinalBlock is true.
Path: $ | LineNumber: 0 | BytePositionInLine: 0
```

**Root Cause**: 
- When mixer disconnected or not yet connected, `/app/mixers/current` returns **empty response**
- AppClient.GetMixersCurrentAsync() expects JSON body always
- No null/empty body handling

**Expected Behavior**:
- When connected: Returns AppMixersCurrentResponse JSON
- When idle/disconnected: Should return HTTP 404 or nullable response?

**Investigation Needed**:
- What should OpenAPI spec say for this case?
- Should DTO be `AppMixersCurrentResponse?` (nullable)?
- Should we catch empty response and return null?

**Workaround**: 
```csharp
// In integration tests, check state first:
var state = await _appClient.GetStateAsync();
if (state.TopState != "connected") {
    // Skip or handle empty response
}
```

**Status**: ⚠️ NEEDS FIX - Need to handle empty response gracefully

---

### 3. Mixer Connection State is Transient

**Observation**: Mixer connection can disconnect between test runs.

**Evidence**:
- First test run: Mixer connected, `TopState = "connected"`
- Later test run: Mixer disconnected, `TopState = "idle"`
- Manual reconnect: `TopState = "connecting"` (takes >5 seconds)

**Implications**:
- Integration tests cannot assume stable mixer connection
- Tests need to handle:
  1. Server running but mixer disconnected
  2. Connecting state (temporary)
  3. Connection failures (mixer unreachable)

**Test Strategy**:
- **Option A**: Skip tests when mixer not connected (current approach)
- **Option B**: Auto-reconnect before test suite (add setup fixture)
- **Option C**: Each test checks/reconnects (slow but robust)

**Current Approach**: Option A with `RequireMixerAsync()` - skips tests when `TopState != "connected"`

**Status**: ⚠️ DESIGN DECISION NEEDED

---

### 4. Test Results Summary (Latest Run)

**Total Tests**: 39 integration tests  
**Passed**: 14 (35.9%)  
**Failed**: 25 (64.1%)  

**Passing Tests** (14):
- ✅ GetStateAsync_ReturnsState
- ✅ GetMixersAvailableAsync_ReturnsMixers
- ✅ PostMixersOfflineAsync_AddsOfflineMixer
- ✅ GetPersistenceAsync_ReturnsSettings
- ✅ GetSettingsCurrentFolderAsync_ReturnsFolder
- ✅ GetSettingsFilesystemEntriesAsync_ReturnsEntries
- ✅ GetPresetsChannelAsync_ReturnsChannelPresets
- ✅ GetPresetsScenesAsync_ReturnsScenePresets
- ✅ PostMixersDcaMapGetAsync_ReturnsDcaMapping
- ✅ GetIdcasAsync_ReturnsIdcas
- ✅ GetDataCategoriesAsync_RequiresMixer ✅ **FIXED DTO**
- ✅ (3 more Phase 9/Console tests - need to rerun for exact list)

**Failing Tests** (25):
Most failures fall into categories:

**Category 1: Empty Response Issues** (like Finding #2):
- GetMixersCurrentAsync_ReturnsCurrentMixer
- GetUiSelectedChannelAsync_ReturnsChannel
- GetUiSelectedChannelAsync_WithParam_RequiresMixer
- PostMixersConnectAsync_WithValidData_Succeeds (edge case: might return empty?)

**Category 2: Requires Mixer Connection**:
- PostDataSetAsync_RequiresMixer
- GetInformationAsync_RequiresMixer
- PostDataSubscribeAsync_RequiresMixer
- GetDataPathsAsync_RequiresMixer
- PostDataUnsubscribeAsync_RequiresMixer
- GetDataDefinitionsAsync_RequiresMixer
- GetDataDefinitions2Async_RequiresMixer
- GetDataPathsByPathAsync_RequiresMixer
- GetAuthInfoAsync_RequiresMixer
- GetMixTargetsAsync_RequiresMixer
- PostAuthLoginAsync_RequiresMixer
- PostMeteringSubscribeAsync_RequiresMixer
- PostMetering2SubscribeAsync_RequiresMixer
- PostMeteringUnsubscribeAsync_RequiresMixer
- PostPresetsChannelCreateAsync_RequiresMixer
- PostPresetsChannelApplyAsync_RequiresMixer
- PostPresetsScenesCreateAsync_RequiresMixer
- PostPresetsScenesApplyAsync_RequiresMixer
- PostIdcasAsync_Create_RequiresMixer
- PostIdcasAsync_Modify_RequiresMixer
- PostIdcasDeleteAsync_RequiresMixer
- PostIdcasRearrangeAsync_RequiresMixer

**Category 3: WebSocket/Advanced**:
- GetOnConfigChangedAsync_WebSocketEndpoint
- PostMixersSearchAsync_StartsSearch
- PostSaveAsync_SavesSettings

**Status**: ⚠️ NEED INDIVIDUAL INVESTIGATION (per user's direction: "don't skip based on assumptions, investigate analyze and proof first!!!")

---

## Next Steps

### Immediate Actions (CRITICAL)

1. **Fix Empty Response Handling** (Finding #2)
   - Modify AppClient.GetMixersCurrentAsync() to handle empty responses
   - Return `null` or throw specific exception when no mixer connected
   - Update tests to expect null when mixer disconnected

2. **Investigate Individual Test Failures** (25 tests)
   - For each failing test:
     1. Run with `--logger "console;verbosity=detailed"`
     2. Check actual HTTP response using `Invoke-RestMethod`
     3. Compare actual response to expected DTO structure
     4. Fix any DTO mismatches found (like #1)
     5. Document actual API behavior
   - **DO NOT assume causes** - must investigate each failure

3. **Document Mixer Connection Requirements**
   - Update test documentation with connection stability findings
   - Add setup instructions for stable test environment
   - Consider adding test setup fixture to auto-reconnect mixer

### Investigation Questions

- [ ] What does OpenAPI spec say about /app/mixers/current when no mixer?
- [ ] Should other endpoints return empty responses too?
- [ ] How should C# client handle null/empty responses?
- [ ] Can we make mixer connection more stable for testing?
- [ ] Are there other DTO mismatches like #1?

---

## Lessons Learned

### ✅ DO
- Investigate actual API responses (use `Invoke-RestMethod`)
- Compare actual JSON to DTO structure
- Fix DTOs when they don't match reality
- Check preconditions explicitly (mixer state, server running)
- Document transient states (connecting, disconnecting)

### ❌ DON'T
- Assume test failures are due to missing preconditions
- Skip tests without investigating actual cause
- Trust incomplete OpenAPI specs blindly
- Assume mixer connection is stable
- Ignore empty responses (need explicit handling)

---

**Last Updated**: 2025-01-31  
**Maintainer**: Integration Test Team  
**Next Review**: After fixing Finding #2 and investigating remaining 25 failures

