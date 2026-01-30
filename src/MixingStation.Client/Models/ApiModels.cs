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
    /// PLACEHOLDER: /console/data does NOT exist in OpenAPI spec
    /// TODO Phase 2: Replace with real endpoint after OpenAPI spec analysis
    /// This type exists only to allow compilation of out-of-scope interfaces
    /// </summary>
    public record ConsoleDataResponse
    {
        // Placeholder - real schema TBD in Phase 2
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
