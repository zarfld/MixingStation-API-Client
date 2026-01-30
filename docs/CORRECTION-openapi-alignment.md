# OpenAPI Alignment Correction - 2026-01-30

**Status**: ✅ COMPLETED  
**Priority**: CRITICAL (Build Blocker)  
**Root Cause**: Tests and design documents invented API contracts instead of using REAL OpenAPI spec

---

## 🚨 Problem Summary

**Issue**: Build failed with 28 compilation errors because tests referenced properties/methods that don't exist in the REAL MixingStation API.

**Root Cause**: Violation of **ADR-002: Public Interface Mirrors REST** - I invented API contracts instead of fetching the actual OpenAPI spec from http://localhost:8045/openapi.json.

---

## ✅ Corrections Made

### 1. Saved OpenAPI Spec as Single Source of Truth

**File**: `docs/openapi.json`  
**Source**: http://localhost:8045/openapi.json  
**Purpose**: All code, tests, and documentation MUST align with this file.

**Command**:
```powershell
Invoke-WebRequest -Uri http://localhost:8045/openapi.json -OutFile docs/openapi.json
```

---

### 2. Updated Design Documents

**Updated Files**:
- `03-architecture/interfaces.md` - Corrected IAppClient interface and DTOs
- `04-design/phase-1-detailed-design.md` - Updated endpoint mapping table
- `docs/openapi-spec-usage.md` - NEW: Complete OpenAPI reference guide

**Key Changes**:

| Aspect | WRONG (Invented) | CORRECT (From OpenAPI) |
|--------|------------------|------------------------|
| **Connect Endpoint** | `/app/connect` | `/app/mixers/connect` |
| **Connect Request** | `{appId, appVersion, host, port}` | `{consoleId, ip}` (blob-ca-b) |
| **Connect Response** | `{success, sessionId, isConnected}` | `204 No Content` (NO BODY!) |
| **Mixer Info** | `MixerInfo` | `AppMixersCurrentResponse` (blob-ca-d$b) |
| **Properties** | `ChannelCount, BusCount` | DOESN'T EXIST (use /console/information) |
| **Property Casing** | `Host, Port` (invented) | `ConsoleId, Ip` (from schema) |
| **Method Names** | `ConnectAsync()` | `PostMixersConnectAsync()` (REST mirror) |

---

### 3. Identified Test Violations

**File**: `tests/MixingStation.Client.Tests/App/AppClientTests.cs`

**Violations Found** (28 compilation errors):

#### ❌ Violation 1: Wrong Request Properties (12 errors)

**Test Code** (WRONG):
```csharp
var request = new AppConnectRequest
{
    AppId = "test-app",
    AppVersion = "1.0.0",
    Host = "localhost",  // ❌ DOESN'T EXIST IN API!
    Port = 8045          // ❌ DOESN'T EXIST IN API!
};
```

**OpenAPI Schema `blob-ca-b`** (CORRECT):
```json
{
  "consoleId": integer,  // Mixer series ID
  "ip": string           // IP address or hostname
}
```

**Correct C# DTO**:
```csharp
public record AppMixersConnectRequest
{
    public int ConsoleId { get; init; }
    public string Ip { get; init; }
}
```

---

#### ❌ Violation 2: Wrong Response Properties (2 errors)

**Test Code** (WRONG):
```csharp
response.IsConnected.Should().BeTrue();      // ❌ NO RESPONSE!
response.ServerVersion.Should().NotBeNull(); // ❌ NO RESPONSE!
```

**OpenAPI Response**: `204 No Content` (NO RESPONSE BODY!)

**Correct Test**:
```csharp
await appClient.PostMixersConnectAsync(request);  // Void Task
// No response to assert - just check no exception thrown
```

---

#### ❌ Violation 3: Wrong Mixer Info Properties (6 errors)

**Test Code** (WRONG):
```csharp
response.FirmwareVersion.Should().NotBeNull();  // ✅ Exists but wrong casing in my mind
response.ChannelCount.Should().BeGreaterThan(0);  // ❌ DOESN'T EXIST!
response.BusCount.Should().BeGreaterThan(0);      // ❌ DOESN'T EXIST!
```

**OpenAPI Schema `blob-ca-d$b`** (CORRECT):
```json
{
  "consoleId": integer,
  "currentModelId": integer,
  "firmwareVersion": string,  // ✅ Exists
  "currentModel": string,
  "ipAddress": string,
  "name": string,
  // ❌ NO ChannelCount or BusCount here!
}
```

**Note**: Channel/bus counts come from `/console/information` (blob-bx-b), not `/app/mixers/current`!

---

#### ❌ Violation 4: AppClient Class Doesn't Exist (10 errors)

**Test Code**:
```csharp
var appClient = new AppClient(mockHttpClientFactory.Object);  // ❌ Class not created yet
```

**Fix**: Create stub class in next step (after models fixed).

---

#### ❌ Violation 5: StatusCode Type Mismatch (4 errors)

**Test Code**:
```csharp
exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);  // int? vs HttpStatusCode
```

**Fix**: Change `TransportException.StatusCode` from `int?` to `HttpStatusCode`.

---

## 📋 Next Steps (In Order)

### ✅ Step 1: Save OpenAPI Spec
**Status**: COMPLETED  
**File**: `docs/openapi.json`

### ✅ Step 2: Update Design Documents
**Status**: COMPLETED  
**Files**: 
- `03-architecture/interfaces.md`
- `04-design/phase-1-detailed-design.md`
- `docs/openapi-spec-usage.md` (NEW reference guide)

### ⏳ Step 3: Update ApiModels.cs
**Status**: PENDING  
**Changes**:
- Rename `AppConnectRequest` → `AppMixersConnectRequest`
- Update properties: `{ConsoleId, Ip}` (remove `AppId, AppVersion, Host, Port`)
- Delete `AppConnectResponse` (204 No Content = no response)
- Rename `AppMixersCurrentResponse` properties to match blob-ca-d$b
- Remove `ChannelCount, BusCount` (doesn't exist in /app/mixers/current)

### ⏳ Step 4: Update TransportException
**Status**: PENDING  
**Change**: `StatusCode` from `int?` to `HttpStatusCode`

### ⏳ Step 5: Create AppClient Stub
**Status**: PENDING  
**File**: `src/MixingStation.Client/App/AppClient.cs`  
**Implementation**: Stub class implementing `IAppClient` (can throw `NotImplementedException`)

### ⏳ Step 6: Verify Build
**Status**: PENDING  
**Command**: `dotnet build`  
**Expected**: Build succeeds (0 errors)

### ⏳ Step 7: Achieve RED Phase
**Status**: PENDING  
**Command**: `dotnet test`  
**Expected**: All 13 tests FAIL (because AppClient throws NotImplementedException)

### ⏳ Step 8: Rewrite Tests
**Status**: PENDING  
**Changes**: Update tests to use REAL OpenAPI contracts

---

## 📊 Impact Analysis

### Files to Update

| File | Status | Changes Required |
|------|--------|------------------|
| `docs/openapi.json` | ✅ DONE | Saved from API |
| `docs/openapi-spec-usage.md` | ✅ DONE | NEW reference guide |
| `03-architecture/interfaces.md` | ✅ DONE | Updated IAppClient, DTOs |
| `04-design/phase-1-detailed-design.md` | ✅ DONE | Updated endpoint table |
| `src/MixingStation.Client/Models/ApiModels.cs` | ⏳ PENDING | Fix DTOs |
| `src/MixingStation.Client/Exceptions/MixingStationExceptions.cs` | ⏳ PENDING | Fix StatusCode type |
| `src/MixingStation.Client/App/IAppClient.cs` | ⏳ PENDING | Update interface |
| `src/MixingStation.Client/App/AppClient.cs` | ⏳ PENDING | Create stub |
| `tests/MixingStation.Client.Tests/App/AppClientTests.cs` | ⏳ PENDING | Rewrite tests |

### Build Status

**Before Correction**: ❌ 28 compilation errors  
**After Step 2**: ⏳ Still 28 errors (models not updated yet)  
**After Step 5**: ✅ Expected: 0 errors (build succeeds)  
**After Step 7**: ✅ Expected: 13 test failures (RED phase achieved)

---

## 🎓 Lessons Learned

### ❌ What Went Wrong

1. **Didn't fetch OpenAPI spec first** - Started coding based on assumptions
2. **Invented API contracts** - Created properties/methods that don't exist
3. **Violated ADR-002** - "Public Interface Mirrors REST" policy ignored
4. **No source of truth** - Design docs contradicted actual API

### ✅ Correct Workflow

1. **Fetch OpenAPI spec FIRST**: `Invoke-WebRequest -Uri http://localhost:8045/openapi.json -OutFile docs/openapi.json`
2. **Study schemas**: Identify request/response schemas (blob-* names)
3. **Map to C# DTOs**: Use PascalCase, match exact properties
4. **Design interfaces**: Methods mirror REST paths exactly
5. **Write tests**: Use REAL schemas (no invented properties)
6. **Implement code**: Match tests and schemas

---

## 🔗 References

- **OpenAPI Spec**: `docs/openapi.json`
- **Usage Guide**: `docs/openapi-spec-usage.md`
- **ADR-002**: `03-architecture/decisions/ADR-002-http-transport-rest-mirror-naming.md` (issue #11)
- **REST Mirror Policy**: `docs/public interface mirrors REST_policy.md`

---

**Corrected By**: Standards Compliance Advisor  
**Date**: 2026-01-30  
**Next Action**: Update `src/MixingStation.Client/Models/ApiModels.cs` to match OpenAPI schemas
