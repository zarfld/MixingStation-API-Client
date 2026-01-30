# Public API Interfaces

**Standard**: ISO/IEC/IEEE 42010:2011 (Interface Specification)  
**Policy**: "Public Interface Mirrors REST" (ADR-002)  
**Phase**: 03 - Architecture  
**Date**: 2026-01-30  

## Overview

This document defines the public API contracts for MixingStation API Client. All public interfaces follow the strict naming policy: **method names mirror REST endpoint paths and verbs**.

---

## Naming Policy (Enforced - ADR-002)

> **All public client members that directly represent REST endpoints MUST use the same nouns and verbs as the REST API path, and MUST preserve the REST resource grouping.**
>
> This policy is **mechanically enforced** via CI (`scripts/validate-api-naming.py`).

### Naming Derivation Algorithm

**Pattern**: `{Verb}{PathSegments}{Async}`

Where:
- **Verb**: `Get` (GET), `Post` (POST), `Put` (PUT), `Delete` (DELETE)
- **PathSegments**: Path parts after group, converted to PascalCase
- **Async**: Suffix for async methods

**Formula**:
```
/{group}/{segment1}/{segment2}  →  {Verb}{Segment1}{Segment2}Async()
```

### Exact Mapping Table (CI Validated)

---

## Namespace Separation (CRITICAL)

### 🔒 Strict REST Mirror (NO Invented Names)

**Namespace**: `MixingStation.Client.*`

**Rules**:
- ✅ Methods MUST match REST endpoints exactly (CI enforced)
- ✅ DTOs MUST match OpenAPI schemas (or document mapping if obfuscated)
- ❌ NO domain-driven naming allowed
- ❌ NO convenience methods allowed

**Purpose**: Perfect traceability between code and API documentation.

### 🎨 Workflow Helpers (Ergonomic Layer)

**Namespace**: `MixingStation.Workflows.*`

**Rules**:
- ✅ Ergonomic method names allowed (`ExportMixerToExcelAsync()`)
- ✅ Composite operations allowed (call multiple REST endpoints)
- ✅ Domain-driven naming encouraged
- ❌ MUST call `MixingStation.Client.*` internally (no direct HTTP calls)
- ❌ MUST document which REST methods are called

**Purpose**: High-level, developer-friendly API for common tasks.

---

## Core Interfaces

### MixingStation.Client.App Namespace (REST Mirror)xersCurrentAsync()` | `GetCurrentMixer()` | App |
| `/app/connect` | POST | `ConnectAsync()` | `EstablishConnection()` | App |
| `/app/disconnect` | POST | `DisconnectAsync()` | `Close()` | App |
| `/app/status` | GET | `GetStatusAsync()` | `GetConnectionStatus()` | App |
| `/console/information` | GET | `GetInformationAsync()` | `GetConsoleInfo()` | Console |
| `/console/data/get/{path}` | GET | `GetDataGetAsync(path)` | `ReadData(path)` | Console |
| `/console/data/set/{path}` | POST | `PostDataSetAsync(path, value)` | `WriteData(path)` | Console |
| `/console/data/subscribe` | POST | `PostDataSubscribeAsync(paths)` | `Subscribe(paths)` | Console |

**Exception for Single-Verb Endpoints**:
- `/app/connect` → `ConnectAsync()` (not `PostConnectAsync()`)
- `/app/disconnect` → `DisconnectAsync()` (not `PostDisconnectAsync()`)
- Rationale: Reads naturally, verb IS the action

### Rules Summary

1. **Namespace = First path segment**: `/app/*` → `MixingStation.Client.App`, `/console/*` → `MixingStation.Client.Console`
2. **Method = Verb + PathSegments + Async**: `/app/mixers/current` → `GetMixersCurrentAsync()`
3. **No invented names**: Enforced by CI (build fails on mismatch)
4. **Async suffix**: All async methods end with `Async`
5. **Path parameters removed**: `/data/get/{path}` → `GetDataGetAsync(string path)` (parameter in signature, not name)

---

## Core Interfaces

### MixingStation.Client.App Namespace

#### IAppClient

**Purpose**: Access `/app/*` endpoints for application-level operations (connect, disconnect, mixer discovery).

**REST Mapping**: `/app/*`

```csharp
namespace MixingStation.Client.App
{
    /// <summary>
    /// Client for MixingStation /app/* endpoints.
    /// Mirrors REST API structure (ADR-002).
    /// </summary>
    public interface IAppClient
    {
        /// <summary>
        /// GET /app/mixers/current
        /// Retrieves currently connected mixer information.
        /// </summary>
        /// <returns>Mixer metadata (series, model, IP, firmware)</returns>
        /// <exception cref="MixingStationException">If not connected or API error</exception>
        Task<MixerInfo> GetMixersCurrentAsync(CancellationToken ct = default);
        
        /// <summary>
        /// POST /app/connect
        /// Establishes connection to MixingStation console.
        /// </summary>
        /// <param name="request">Connection parameters (IP, port, timeout)</param>
        /// <returns>Connection result with status</returns>
        /// <exception cref="MixingStationException">If connection fails</exception>
        Task<ConnectResponse> ConnectAsync(ConnectRequest request, CancellationToken ct = default);
        
        /// <summary>
        /// POST /app/disconnect
        /// Gracefully disconnects from console.
        /// </summary>
        /// <returns>Disconnect confirmation</returns>
        Task<DisconnectResponse> DisconnectAsync(CancellationToken ct = default);
        
        /// <summary>
        /// GET /app/status
        /// Retrieves current connection status and session info.
        /// </summary>
        /// <returns>Connection status (connected/disconnected), session ID, uptime</returns>
        Task<StatusResponse> GetStatusAsync(CancellationToken ct = default);
    }
}
```

**DTOs (Data Transfer Objects)**:

```csharp
public record MixerInfo
{
    public string ConsoleSeries { get; init; }    // "Behringer X32"
    public string Model { get; init; }            // "X32 Compact"
    public string FirmwareVersion { get; init; }
    public string IpAddress { get; init; }
---

### MixingStation.Client.Console Namespace (REST Mirror)

#### IConsoleClient

**Purpose**: Access `/console/*` endpoints for console-specific data (channel configs, values, subscriptions).

**REST Mapping**: `/console/*`  
**Naming Policy**: Methods mirror REST paths exactly (ADR-002)get; init; } = TimeSpan.FromSeconds(10);
}

public record ConnectResponse
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public string SessionId { get; init; }
}

public record DisconnectResponse
{
    public bool Success { get; init; }
}

public record StatusResponse
{
    public bool IsConnected { get; init; }
    public string SessionId { get; init; }
    public TimeSpan Uptime { get; init; }
}
```

---

### MixingStation.Client.Console Namespace

#### IConsoleClient

**Purpose**: Access `/console/*` endpoints for console-specific data (channel configs, values, subscriptions).

**REST Mapping**: `/console/*`

```csharp
namespace MixingStation.Client.Console
{
    /// <summary>
    /// Client for MixingStation /console/* endpoints.
    /// Mirrors REST API structure (ADR-002).
    /// </summary>
    public interface IConsoleClient
    {
        /// <summary>
        /// GET /console/information
        /// Retrieves console metadata (type, channel counts, capabilities).
        /// </summary>
        /// <returns>Console type, total channels, aux counts, bus counts</returns>
        /// <exception cref="MixingStationException">If not connected or API error</exception>
        Task<ConsoleInformation> GetInformationAsync(CancellationToken ct = default);
        
        /// <summary>
        /// GET /console/data/get/{path}/val
        /// Retrieves specific console data path value.
        /// Example: path = "ch.01.config.name" returns channel 1 name.
        /// </summary>
        /// <param name="path">Console data path (e.g., "ch.01.config.name")</param>
        /// <returns>Value at path (type varies: string, float, bool)</returns>
        /// <exception cref="MixingStationException">If path not found or API error</exception>
        Task<DataValue> GetDataAsync(string path, CancellationToken ct = default);
        
        /// <summary>
        /// POST /console/data/set/{path}/val
        /// Sets specific console data path value.
        /// </summary>
        /// <param name="path">Console data path</param>
        /// <param name="value">New value (type must match path type)</param>
        /// <returns>Set confirmation</returns>
        /// <exception cref="MixingStationException">If path invalid or value rejected</exception>
        Task<SetDataResponse> SetDataAsync(string path, object value, CancellationToken ct = default);
        
        /// <summary>
        /// POST /console/data/subscribe
        /// Subscribes to console data path changes (WebSocket).
        /// NOTE: Phase 1 uses HTTP polling; Phase 2 adds WebSocket support.
        /// </summary>
        /// <param name="paths">Paths to subscribe to</param>
        /// <returns>Subscription handle</returns>
        Task<SubscriptionResponse> SubscribeAsync(string[] paths, CancellationToken ct = default);
    }
}
```

**DTOs**:

```csharp
public record ConsoleInformation
{
    public string ConsoleType { get; init; }      // "X32", "M32", "Wing"
    public int TotalChannels { get; init; }
    public int InputChannels { get; init; }
    public int AuxChannels { get; init; }
    public int BusChannels { get; init; }
    public int DcaCount { get; init; }
    public string FirmwareVersion { get; init; }
}

public record DataValue
{
    public string Path { get; init; }
    public object Value { get; init; }            // string, float, bool, int
    public string Type { get; init; }             // "string", "float", "bool", "int"
}

public record SetDataResponse
{
    public bool Success { get; init; }
    public string Path { get; init; }
    public object NewValue { get; init; }
}

public record SubscriptionResponse
{
    public string SubscriptionId { get; init; }
    public string[] SubscribedPaths { get; init; }
}
```

---

---

## Domain Layer Interfaces (Not REST Clients)

### MixingStation.Normalizer Namespace

#### INormalizer

**Purpose**: Transform raw API responses into console-agnostic CanonicalModel.

**Note**: This is NOT a REST client; naming policy does not apply here.

```csharp
namespace MixingStation.Normalizer
{
    /// <summary>
    /// Transforms API responses into CanonicalModel (console-agnostic).
    /// Implements ADR-003 normalization rules.
    /// </summary>
    public interface INormalizer
    {
        /// <summary>
        /// Normalizes API responses into CanonicalModel v0.
        /// </summary>
        /// <param name="mixerInfo">From /app/mixers/current</param>
        /// <param name="consoleInfo">From /console/information</param>
        /// <param name="channelData">Channel configs and values</param>
        /// <returns>Normalized CanonicalModel</returns>
        /// <exception cref="NormalizationException">If required data missing</exception>
        Task<CanonicalModel> NormalizeAsync(
            MixerInfo mixerInfo,
            ConsoleInformation consoleInfo,
            Dictionary<string, DataValue> channelData,
            CancellationToken ct = default);
    }
}
```

---

---

### MixingStation.Workflows.Export Namespace (Ergonomic Layer)

#### IExcelExporter

**Purpose**: Export CanonicalModel to Excel files.

**Note**: This is a workflow helper; ergonomic naming allowed (NOT a REST client).

```csharp
namespace MixingStation.Workflows.Export
{
    /// <summary>
    /// Exports CanonicalModel to Excel format.
    /// Implements ADR-004 formatting standards.
    /// </summary>
    public interface IExcelExporter
    {
        /// <summary>
        /// Generates Excel workbook as byte array.
        /// </summary>
        /// <param name="model">Normalized mixer data</param>
        /// <returns>Excel file bytes (.xlsx)</returns>
        Task<byte[]> GenerateAsync(CanonicalModel model, CancellationToken ct = default);
        
        /// <summary>
        /// Exports Excel workbook to file path.
        /// </summary>
        /// <param name="model">Normalized mixer data</param>
        /// <param name="filePath">Output file path (e.g., "InputList.xlsx")</param>
        Task ExportToFileAsync(CanonicalModel model, string filePath, CancellationToken ct = default);
    }
    
    public class ExcelExporterOptions
    {
        public bool IncludeMetadata { get; set; } = true;
        public bool AutoSizeColumns { get; set; } = true;
        public bool FreezeHeaderRow { get; set; } = true;
        public bool EnableAutoFilter { get; set; } = true;
    }
}
```

---

## Error Handling Interfaces

### MixingStation.Client.Exceptions Namespace

```csharp
namespace MixingStation.Client.Exceptions
{
    /// <summary>
    /// Base exception for all MixingStation API errors.
    /// </summary>
    public class MixingStationException : Exception
    {
        public string ErrorCode { get; }
        public int? HttpStatusCode { get; }
        
        public MixingStationException(string message, string errorCode, int? httpStatusCode = null)
            : base(message)
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
        }
    }
    
    public class ConnectionException : MixingStationException
    {
        public ConnectionException(string message)
            : base(message, "CONNECTION_FAILED") { }
    }
    
    public class ApiException : MixingStationException
    {
        public ApiException(string message, int httpStatusCode)
            : base(message, "API_ERROR", httpStatusCode) { }
    }
    
    public class NormalizationException : MixingStationException
    {
        public NormalizationException(string message)
            : base(message, "NORMALIZATION_FAILED") { }
    }
}
```

---

## Configuration Interfaces

### MixingStation.Client.Configuration Namespace

```csharp
namespace MixingStation.Client.Configuration
{
    /// <summary>
    /// Configuration options for MixingStation clients.
    /// </summary>
    public class MixingStationClientOptions
    {
        public Uri BaseAddress { get; set; } = new Uri("http://localhost:8045/");
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public int MaxRetries { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);
        
        public JsonSerializerOptions JsonOptions { get; set; } = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
}
```

---

## Dependency Injection Setup

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using MixingStation.Client.App;
using MixingStation.Client.Console;
using MixingStation.Normalizer;
using MixingStation.Workflows.Export;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMixingStationClient(
        this IServiceCollection services,
        Action<MixingStationClientOptions> configure = null)
    {
        var options = new MixingStationClientOptions();
        configure?.Invoke(options);
        
        // HTTP Clients (ADR-002)
        services.AddHttpClient<IAppClient, AppClient>(client =>
        {
            client.BaseAddress = options.BaseAddress;
            client.Timeout = options.Timeout;
        });
        
        services.AddHttpClient<IConsoleClient, ConsoleClient>(client =>
        {
            client.BaseAddress = options.BaseAddress;
            client.Timeout = options.Timeout;
        });
        
        // Domain Services
        services.AddSingleton<INormalizer, Normalizer>();
        services.AddSingleton<IExcelExporter, ExcelExporter>();
        
        return services;
    }
}
```

### CLI Usage Example

```csharp
// Program.cs (exportlists CLI)
var services = new ServiceCollection();

services.AddMixingStationClient(options =>
{
    options.BaseAddress = new Uri($"http://{ipAddress}:8045/");
    options.Timeout = TimeSpan.FromSeconds(30);
});

var provider = services.BuildServiceProvider();
var appClient = provider.GetRequiredService<IAppClient>();
var consoleClient = provider.GetRequiredService<IConsoleClient>();
var normalizer = provider.GetRequiredService<INormalizer>();
var exporter = provider.GetRequiredService<IExcelExporter>();

// Use clients...
await appClient.ConnectAsync(new ConnectRequest { IpAddress = ipAddress });
```

---

## Traceability Matrix

| Interface | Implements Requirement | Verified By |
|-----------|----------------------|-------------|
| `IAppClient` | REQ-F-001, REQ-F-002 | TEST-APP-001 |
| `IConsoleClient` | REQ-F-001, REQ-F-003 | TEST-CONSOLE-001 |
| `INormalizer` | REQ-F-004 | TEST-NORMALIZE-001 |
| `IExcelExporter` | REQ-F-005 | TEST-EXPORT-001 |

---

## Design Patterns Applied

| Pattern | Where | Purpose |
|---------|-------|---------|
| **Interface Segregation** | `IAppClient`, `IConsoleClient` | Separate concerns by REST namespace |
| **Dependency Injection** | All interfaces | Testability, loose coupling |
| **Factory** | `IHttpClientFactory` | Connection pooling, lifecycle management |
| **Adapter** | `INormalizer` | Transform API → CanonicalModel |
| **Strategy** | `IExcelExporter` | Future: CSV, Google Sheets exporters |

---

---

## CI Enforcement

### Naming Guard Script

**Location**: `scripts/validate-api-naming.py`

**Purpose**: Fail CI if public methods in `MixingStation.Client.*` don't match REST endpoints.

**Usage**:
```bash
python scripts/validate-api-naming.py \
  --openapi openapi.json \
  --assembly bin/Release/net8.0/MixingStation.Client.dll
```

**Example Output**:
```
✅ AppClient.GetMixersCurrentAsync() → Matches GET /app/mixers/current
✅ AppClient.ConnectAsync() → Matches POST /app/connect
❌ AppClient.GetCurrentMixer() → VIOLATION: Should be GetMixersCurrentAsync()

❌ CI FAILED: 1 naming violation found
```

**CI Integration** (`.github/workflows/ci.yml`):
```yaml
- name: Build library
  run: dotnet build --configuration Release

- name: Validate API Naming Policy (ADR-002)
  run: |
    python scripts/validate-api-naming.py \
      --openapi openapi.json \
      --assembly src/MixingStation.Client/bin/Release/net8.0/MixingStation.Client.dll
```

---

## Future Extensions (Phase 2+)

### IWebSocketClient (Phase 2) - REST Mirror

```csharp
namespace MixingStation.Client.Console
{
    /// <summary>
    /// WebSocket client for real-time console data subscriptions.
    /// Phase 2: Replaces HTTP polling with persistent connection.
    /// Naming policy: Mirrors WebSocket endpoint paths.
    /// </summary>
    public interface IWebSocketClient
    {
        // WebSocket /console/data/subscribe (persistent connection)
        Task PostDataSubscribeWebSocketAsync(string[] paths, Action<DataValue> onUpdate);
        Task DisconnectAsync();
    }
}
```

### IWorkflowOrchestrator (Phase 2+) - Ergonomic Layer

```csharp
namespace MixingStation.Workflows
{
    /// <summary>
    /// High-level workflow orchestration (ergonomic helpers).
    /// Naming policy: Ergonomic names allowed (NOT REST mirror).
    /// Internally calls AppClient + ConsoleClient.
    /// </summary>
    public interface IWorkflowOrchestrator
    {
        /// <summary>
        /// Export mixer to Excel (composite workflow).
        /// Calls: ConnectAsync() → GetMixersCurrentAsync() → GetInformationAsync() → ExportToFileAsync()
        /// </summary>
        Task<string> ExportMixerToExcelAsync(string ipAddress, string outputPath);
        
        /// <summary>
        /// Capture mixer snapshot (composite workflow).
        /// Calls: ConnectAsync() → GetMixersCurrentAsync() → GetInformationAsync() → NormalizeAsync()
        /// </summary>
        Task<CanonicalModel> CaptureMixerSnapshotAsync(string ipAddress);
    }
}
```

---

## Standards Compliance

- ✅ **ISO/IEC/IEEE 42010:2011**: Interface specifications with clear semantics
- ✅ **ADR-002 Enforcement**: Public methods mirror REST endpoints (CI validated)
- ✅ **IEEE 1016-2009**: Interface design descriptions

---

**Next Phase**: Phase 04 - Detailed Design (sequence diagrams, class designs)
