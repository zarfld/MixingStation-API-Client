---
applyTo: "**/*"
---

# OpenAPI Specification - Single Source of Truth

**File**: `docs/openapi.json`  
**Source**: http://localhost:8045/openapi.json  
**Policy**: **ADR-002: Public Interface Mirrors REST**  
**Last Updated**: 2026-01-30

---

## 🎯 Purpose

`docs/openapi.json` is the **SINGLE SOURCE OF TRUTH** for all REST API contracts in this repository. All C# interfaces, DTOs, and tests MUST align with this specification.

**Critical Rule**: If code doesn't match `openapi.json`, the code is wrong (not the spec).

---

## 📦 OpenAPI Schema Reference (blob-* schemas)

The API uses obfuscated schema names (`blob-ca-b`, `blob-bx-m`, etc.). Below are the key schemas for Phase 1:

### App Endpoints (`/app/*`)

#### POST `/app/mixers/connect`

**Request Schema**: `blob-ca-b`
```json
{
  "consoleId": integer,  // Mixer series ID which should be started
  "ip": string           // IP address or hostname
}
```

**Response**: `204 No Content` (NO RESPONSE BODY!)

**C# Mapping**:
```csharp
public record AppMixersConnectRequest
{
    public int ConsoleId { get; init; }
    public string Ip { get; init; }
}

// Method signature
Task PostMixersConnectAsync(AppMixersConnectRequest request, CancellationToken ct = default);
```

---

#### GET `/app/mixers/current`

**Response Schema**: `blob-ca-d$b`
```json
{
  "consoleId": integer,
  "currentModelId": integer,
  "ipAddress": string,
  "name": string,                      // Mixer series name
  "firmwareVersion": string,
  "currentModel": string,
  "manufacturer": string,
  "models": string[],                  // Available models
  "supportedHardwareModels": string[], // Human-readable list
  "canSearch": boolean,
  "manufacturerId": integer,
  "modelEnums": blob-bg-b[]            // App-internal mixer models
}
```

**C# Mapping**:
```csharp
public record AppMixersCurrentResponse
{
    public int ConsoleId { get; init; }
    public int CurrentModelId { get; init; }
    public string IpAddress { get; init; }
    public string Name { get; init; }
    public string FirmwareVersion { get; init; }
    public string CurrentModel { get; init; }
    public string Manufacturer { get; init; }
    public string[] Models { get; init; }
    public string[] SupportedHardwareModels { get; init; }
    public bool CanSearch { get; init; }
    public int ManufacturerId { get; init; }
}

// Method signature
Task<AppMixersCurrentResponse> GetMixersCurrentAsync(CancellationToken ct = default);
```

---

#### GET `/app/state`

**Response Schema**: `blob-bw-a`
```json
{
  "msg": string,
  "progress": integer,  // 0-100
  "state": string,
  "topState": string
}
```

**C# Mapping**:
```csharp
public record AppStateResponse
{
    public string Msg { get; init; }
    public int Progress { get; init; }
    public string State { get; init; }
    public string TopState { get; init; }
}

Task<AppStateResponse> GetStateAsync(CancellationToken ct = default);
```

---

### Console Endpoints (`/console/*`)

#### GET `/console/information`

**Response Schema**: `blob-bx-b`
```json
{
  "totalChannels": integer,
  "channelColors": blob-bx-b$a[],    // Color definitions
  "channelTypes": blob-bx-b$c[],     // Channel type definitions
  "rtaFrequencies": float[],
  "dbfsOffset": number
}
```

**Purpose**: Returns channel architecture details (channel types, colors, total count).

**C# Mapping**:
```csharp
public record ConsoleInformationResponse
{
    public int TotalChannels { get; init; }
    public ChannelColor[] ChannelColors { get; init; }
    public ChannelType[] ChannelTypes { get; init; }
    public float[] RtaFrequencies { get; init; }
    public double DbfsOffset { get; init; }
}

Task<ConsoleInformationResponse> GetInformationAsync(CancellationToken ct = default);
```

---

#### GET `/console/data/get/{path}/{format}`

**Path Parameters**:
- `path`: Console data path (e.g., `ch.0.name`)
- `format`: `val` (actual value) or `norm` (normalized 0-1)

**Response Schema**: `blob-bx-m`
```json
{
  "format": string,  // "val" or "norm"
  "value": object    // Type varies by path
}
```

**C# Mapping**:
```csharp
public record ConsoleDataValue
{
    public string Format { get; init; }
    public object Value { get; init; }
}

Task<ConsoleDataValue> GetDataGetAsync(string path, string format, CancellationToken ct = default);
```

---

## 🚨 Critical Naming Rules (ADR-002)

### 1. Method Names Mirror REST Paths

**Formula**: `{HttpVerb}{PathSegmentsPascalCase}Async()`

**Examples**:
- `POST /app/mixers/connect` → `PostMixersConnectAsync()`
- `GET /app/mixers/current` → `GetMixersCurrentAsync()`
- `GET /console/information` → `GetInformationAsync()`
- `GET /console/data/get/{path}/{format}` → `GetDataGetAsync(path, format)`

### 2. Property Names Use PascalCase (C# Convention)

API uses camelCase, C# uses PascalCase:

**API (JSON)**:
```json
{
  "consoleId": 1,
  "firmwareVersion": "1.0.0"
}
```

**C# DTO**:
```csharp
public record AppMixersCurrentResponse
{
    public int ConsoleId { get; init; }         // Maps from "consoleId"
    public string FirmwareVersion { get; init; } // Maps from "firmwareVersion"
}
```

### 3. No Invented Properties

❌ **WRONG** (invented properties not in OpenAPI):
```csharp
public record AppConnectRequest
{
    public string Host { get; init; }  // DOESN'T EXIST IN API!
    public int Port { get; init; }     // DOESN'T EXIST IN API!
}
```

✅ **CORRECT** (matches blob-ca-b schema):
```csharp
public record AppMixersConnectRequest
{
    public int ConsoleId { get; init; }  // ✅ Exists in blob-ca-b
    public string Ip { get; init; }      // ✅ Exists in blob-ca-b
}
```

---

## 🔄 Update Workflow

When MixingStation API changes:

1. **Fetch Latest OpenAPI Spec**:
   ```powershell
   Invoke-WebRequest -Uri http://localhost:8045/openapi.json -OutFile docs/openapi.json
   ```

2. **Compare Changes**:
   ```powershell
   git diff docs/openapi.json
   ```

3. **Update C# Code**:
   - Update `src/MixingStation.Client/Models/ApiModels.cs` (DTOs)
   - Update `src/MixingStation.Client/App/IAppClient.cs` (interfaces)
   - Update `src/MixingStation.Client/Console/IConsoleClient.cs` (interfaces)

4. **Update Tests**:
   - Update test fixtures to match new schemas
   - Regenerate mock responses

5. **Update Documentation**:
   - Update `03-architecture/interfaces.md`
   - Update `04-design/phase-1-detailed-design.md`

---

## 🧪 Validation

### CI Validation (Planned)

Script: `scripts/validate-api-naming.py`

**Checks**:
- All interface methods match OpenAPI paths
- All DTOs match OpenAPI schemas
- No invented properties or methods

**Run Manually**:
```powershell
python scripts/validate-api-naming.py --openapi docs/openapi.json --assembly bin/Debug/net8.0/MixingStation.Client.dll
```

### Manual Validation

**Check Method Names**:
```powershell
# List all public methods
dotnet build
Get-Content src/MixingStation.Client/App/IAppClient.cs | Select-String "Task"
```

**Check Against OpenAPI**:
```powershell
# Pretty-print OpenAPI paths
Get-Content docs/openapi.json | ConvertFrom-Json | Select -ExpandProperty paths | ConvertTo-Json -Depth 5
```

---

## 📚 Key OpenAPI Paths (Phase 1)

| Endpoint | Method | Request Schema | Response Schema | Status |
|----------|--------|----------------|-----------------|--------|
| `/app/mixers/connect` | POST | blob-ca-b | 204 No Content | ✅ Phase 1 |
| `/app/mixers/current` | GET | - | blob-ca-d$b | ✅ Phase 1 |
| `/app/state` | GET | - | blob-bw-a | ✅ Phase 1 |
| `/console/information` | GET | - | blob-bx-b | ✅ Phase 1 |
| `/console/data/get/{path}/{format}` | GET | - | blob-bx-m | ✅ Phase 1 |
| `/console/data/subscribe` | POST | blob-bw-j | 204 No Content | ⏳ Phase 2 |
| `/console/data/set/{path}/{format}` | POST | blob-bx-m | blob-bx-m | ⏳ Phase 2 |

---

## ⚠️ Common Mistakes

### ❌ Mistake 1: Inventing Response Objects for 204 No Content

**Wrong**:
```csharp
Task<ConnectResponse> PostMixersConnectAsync(...);
```

**Correct**:
```csharp
Task PostMixersConnectAsync(...);  // Void Task (204 No Content)
```

### ❌ Mistake 2: Wrong Endpoint Path

**Wrong**:
```csharp
// Method for /app/connect (doesn't exist!)
Task ConnectAsync(...);
```

**Correct**:
```csharp
// Method for /app/mixers/connect (real endpoint)
Task PostMixersConnectAsync(...);
```

### ❌ Mistake 3: Invented Property Names

**Wrong**:
```csharp
public record AppMixersCurrentResponse
{
    public string FirmwareVersion { get; init; }  // ❌ Too generic
    public int ChannelCount { get; init; }        // ❌ Doesn't exist in API!
}
```

**Correct**:
```csharp
public record AppMixersCurrentResponse
{
    public string FirmwareVersion { get; init; }  // ✅ Maps from "firmwareVersion"
    // ChannelCount doesn't exist - get it from /console/information instead
}
```

---

## 🔗 References

- **OpenAPI Spec**: `docs/openapi.json`
- **ADR-002**: `03-architecture/decisions/ADR-002-http-transport-rest-mirror-naming.md` (issue #11)
- **Interface Contracts**: `03-architecture/interfaces.md`
- **Phase 1 Design**: `04-design/phase-1-detailed-design.md`
- **Live API**: http://localhost:8045/openapi.json

---

**Version**: 1.0  
**Last Updated**: 2026-01-30  
**Maintained By**: Standards Compliance Team
