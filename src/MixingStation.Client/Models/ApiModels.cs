/**
 * API DTOs - Request/Response models for REST endpoints
 * 
 * SOURCE OF TRUTH: docs/openapi.json (http://localhost:8045/openapi.json)
 * Public API mirrors REST endpoints exactly (ADR-002: #11)
 * DTOs map 1:1 to OpenAPI schemas (blob-* obfuscated names)
 * 
 * Architecture:
 * - #11 ADR-002: HTTP Transport + REST Mirror Naming Policy
 * 
 * Design: Phase 1 Detailed Design (04-design/phase-1-detailed-design.md)
 * Reference: docs/openapi-spec-usage.md
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/11
 */

namespace MixingStation.Client.Models
{
    // ========================================
    // /app/* Endpoints (Application Lifecycle)
    // ========================================

    /// <summary>
    /// POST /app/mixers/connect - Request
    /// OpenAPI Schema: blob-ca-b
    /// </summary>
    public record AppMixersConnectRequest
    {
        /// <summary>Mixer series ID which should be started</summary>
        public int ConsoleId { get; init; }
        
        /// <summary>IP address or hostname</summary>
        public string Ip { get; init; } = string.Empty;
    }

    // NOTE: POST /app/mixers/connect returns 204 No Content (NO RESPONSE BODY!)

    /// <summary>
    /// GET /app/mixers/current - Response
    /// OpenAPI Schema: blob-ca-d$b
    /// </summary>
    public record AppMixersCurrentResponse
    {
        /// <summary>ID of this mixer series</summary>
        public int ConsoleId { get; init; }
        
        /// <summary>Current model ID</summary>
        public int CurrentModelId { get; init; }
        
        /// <summary>Mixer IP address</summary>
        public string IpAddress { get; init; } = string.Empty;
        
        /// <summary>Mixer series name (e.g., "Behringer X32")</summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>Firmware version</summary>
        public string FirmwareVersion { get; init; } = string.Empty;
        
        /// <summary>Current model name</summary>
        public string CurrentModel { get; init; } = string.Empty;
        
        /// <summary>Manufacturer name</summary>
        public string Manufacturer { get; init; } = string.Empty;
        
        /// <summary>Available models</summary>
        public string[] Models { get; init; } = Array.Empty<string>();
        
        /// <summary>Human-readable list of supported mixer models</summary>
        public string[] SupportedHardwareModels { get; init; } = Array.Empty<string>();
        
        /// <summary>Can search for mixers</summary>
        public bool CanSearch { get; init; }
        
        /// <summary>Manufacturer ID</summary>
        public int ManufacturerId { get; init; }
    }

    /// <summary>
    /// GET /app/state - Response
    /// OpenAPI Schema: blob-bw-a
    /// </summary>
    public record AppStateResponse
    {
        /// <summary>State message</summary>
        public string Msg { get; init; } = string.Empty;
        
        /// <summary>Progress (0-100)</summary>
        public int Progress { get; init; }
        
        /// <summary>Current state</summary>
        public string State { get; init; } = string.Empty;
        
        /// <summary>Top-level state</summary>
        public string TopState { get; init; } = string.Empty;
    }

    // ========================================
    // Phase 3: Mixer Lifecycle Endpoints
    // ========================================

    /// <summary>
    /// GET /app/mixers/available - Response
    /// OpenAPI Schema: blob-ca-d
    /// </summary>
    public record AppMixersAvailableResponse
    {
        /// <summary>List of available mixer console series</summary>
        public MixerConsole[] Consoles { get; init; } = Array.Empty<MixerConsole>();
    }

    /// <summary>
    /// Mixer console series details
    /// OpenAPI Schema: blob-ca-d$a
    /// </summary>
    public record MixerConsole
    {
        /// <summary>ID of this mixer series</summary>
        public int ConsoleId { get; init; }
        
        /// <summary>Available models for this series</summary>
        public string[] Models { get; init; } = Array.Empty<string>();
        
        /// <summary>Human-readable list of supported mixer models</summary>
        public string[] SupportedHardwareModels { get; init; } = Array.Empty<string>();
        
        /// <summary>Manufacturer ID</summary>
        public int ManufacturerId { get; init; }
        
        /// <summary>Mixer series name</summary>
        public string Name { get; init; } = string.Empty;
    }

    /// <summary>
    /// POST /app/mixers/search - Request
    /// OpenAPI Schema: blob-ca-f
    /// </summary>
    public record AppMixersSearchRequest
    {
        /// <summary>Mixer series ID to search for</summary>
        public int ConsoleId { get; init; }
    }

    // NOTE: POST /app/mixers/search returns 204 No Content (NO RESPONSE BODY!)

    /// <summary>
    /// GET /app/mixers/searchResults - Response
    /// OpenAPI Schema: blob-ca-e
    /// </summary>
    public record AppMixersSearchResultsResponse
    {
        /// <summary>List of found mixers</summary>
        public MixerSearchResult[] Results { get; init; } = Array.Empty<MixerSearchResult>();
    }

    /// <summary>
    /// Individual mixer search result
    /// OpenAPI Schema: blob-ca-e$a
    /// </summary>
    public record MixerSearchResult
    {
        /// <summary>Model ID</summary>
        public int ModelId { get; init; }
        
        /// <summary>IP address</summary>
        public string Ip { get; init; } = string.Empty;
        
        /// <summary>Mixer name</summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>Model name</summary>
        public string Model { get; init; } = string.Empty;
        
        /// <summary>Firmware version</summary>
        public string Version { get; init; } = string.Empty;
    }

    // NOTE: POST /app/mixers/disconnect returns 204 No Content (NO REQUEST/RESPONSE BODY!)

    /// <summary>
    /// POST /app/mixers/offline - Request
    /// OpenAPI Schema: blob-ca-g
    /// </summary>
    public record AppMixersOfflineRequest
    {
        /// <summary>Mixer series ID for offline mode</summary>
        public int ConsoleId { get; init; }
        
        /// <summary>Model ID for offline mode</summary>
        public int ModelId { get; init; }
        
        /// <summary>Model name</summary>
        public string Model { get; init; } = string.Empty;
    }

    // NOTE: POST /app/mixers/offline returns 204 No Content (NO RESPONSE BODY!)

    // ========================================
    // /console/* Endpoints (Console Data)
    // ========================================

    /// <summary>
    /// GET /console/information - Response
    /// OpenAPI Schema: blob-bx-b
    /// </summary>
    public record ConsoleInformationResponse
    {
        /// <summary>Total number of channels</summary>
        public int TotalChannels { get; init; }
        
        /// <summary>Channel color definitions</summary>
        public ChannelColor[] ChannelColors { get; init; } = Array.Empty<ChannelColor>();
        
        /// <summary>Channel type definitions</summary>
        public ChannelType[] ChannelTypes { get; init; } = Array.Empty<ChannelType>();
        
        /// <summary>RTA frequency points</summary>
        public float[] RtaFrequencies { get; init; } = Array.Empty<float>();
        
        /// <summary>dBFS offset for level meters</summary>
        public double DbfsOffset { get; init; }
    }

    /// <summary>
    /// Channel color definition (blob-bx-b$a sub-schema)
    /// </summary>
    public record ChannelColor
    {
        /// <summary>Color index/ID</summary>
        public int Id { get; init; }
        
        /// <summary>Color name</summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>Hex color code (e.g., "#FF5733")</summary>
        public string Hex { get; init; } = string.Empty;
    }

    /// <summary>
    /// Channel type definition (blob-bx-b$c sub-schema)
    /// </summary>
    public record ChannelType
    {
        /// <summary>Type index/ID</summary>
        public int Id { get; init; }
        
        /// <summary>Type name (e.g., "Input", "Aux", "Bus", "Matrix")</summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>Number of channels of this type</summary>
        public int Count { get; init; }
    }

    /// <summary>
    /// GET /console/data/get/{path}/{format} - Response
    /// OpenAPI Schema: blob-bx-m
    /// </summary>
    public record ConsoleDataValue
    {
        /// <summary>Format: "val" (actual value) or "norm" (normalized 0-1)</summary>
        public string Format { get; init; } = string.Empty;
        
        /// <summary>Value (type varies by path)</summary>
        public object? Value { get; init; }
    }

    /// <summary>
    /// POST /console/data/subscribe - Request
    /// POST /console/data/unsubscribe - Request
    /// OpenAPI Schema: blob-bw-j
    /// </summary>
    public record ConsoleDataSubscribeRequest
    {
        /// <summary>Console data path pattern (e.g., "ch.*.name", "ch.0.*")</summary>
        public string Path { get; init; } = string.Empty;
        
        /// <summary>Value format: "val" (actual value) or "norm" (normalized 0-1)</summary>
        public string Format { get; init; } = string.Empty;
    }

    /// <summary>
    /// LEGACY PLACEHOLDER - DO NOT USE
    /// Originally created for out-of-scope endpoint /console/data (does not exist in OpenAPI)
    /// Phase 2 implemented real endpoints: /console/data/subscribe, /console/data/set
    /// This type exists only for backward compatibility
    /// </summary>
    [Obsolete("Use ConsoleDataValue or ConsoleDataSubscribeRequest instead")]
    public record ConsoleDataResponse
    {
        // Placeholder - not used in actual implementation
    }

    // ========================================
    // Domain Models (Not REST DTOs)
    // ========================================

    /// <summary>
    /// Excel export result (domain model, NOT a REST response)
    /// </summary>
    public record ExportResult
    {
        public bool Success { get; init; }
        public string InputListPath { get; init; } = string.Empty;
        public string PatchingListPath { get; init; } = string.Empty;
        public List<string> Warnings { get; init; } = new();
        public string? ErrorMessage { get; init; }
    }
}
