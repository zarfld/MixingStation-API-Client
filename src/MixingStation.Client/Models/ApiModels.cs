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

    // ========================================
    // Phase 4: Console Data Discovery
    // ========================================

    /// <summary>
    /// GET /console/data/categories - Response
    /// OpenAPI Schema: blob-bx-c (empty object - dynamic content)
    /// </summary>
    public record ConsoleDataCategoriesResponse
    {
        /// <summary>Dynamic categories dictionary (key=category name, value=category data)</summary>
        public Dictionary<string, object?> Categories { get; init; } = new();
    }

    /// <summary>
    /// GET /console/data/paths - Response
    /// GET /console/data/paths/{path} - Response
    /// OpenAPI Schema: blob-bx-f
    /// </summary>
    public record ConsoleDataPathsResponse
    {
        /// <summary>Values at this path level</summary>
        public string[] Val { get; init; } = Array.Empty<string>();
        
        /// <summary>Child paths (recursive structure)</summary>
        public Dictionary<string, ConsoleDataPathsResponse> Child { get; init; } = new();
    }

    /// <summary>
    /// GET /console/data/definitions/{path} - Response (DEPRECATED)
    /// OpenAPI Schema: blob-bx-d
    /// </summary>
    [Obsolete("Use GetDataDefinitions2Async instead - /console/data/definitions2/{path}")]
    public record ConsoleDataDefinitionsResponse
    {
        /// <summary>Data definitions dictionary</summary>
        public Dictionary<string, ConsoleDataPathDefinition> Definitions { get; init; } = new();
    }

    /// <summary>
    /// Individual path definition (blob-bx-l sub-schema)
    /// </summary>
    public record ConsoleDataPathDefinition
    {
        /// <summary>Data path</summary>
        public string Path { get; init; } = string.Empty;
        
        /// <summary>Value definition details</summary>
        public DataValueDefinition? Definition { get; init; }
        
        /// <summary>String constraints (if value is string type)</summary>
        public string[] Constraints { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Value definition details (blob-bx-l$definition sub-schema)
    /// </summary>
    public record DataValueDefinition
    {
        /// <summary>Accepted enums (for enum type)</summary>
        public EnumValue[] Enums { get; init; } = Array.Empty<EnumValue>();
        
        /// <summary>Value unit (e.g., "dB", "Hz")</summary>
        public string Unit { get; init; } = string.Empty;
        
        /// <summary>True if value is tappable</summary>
        public bool Tap { get; init; }
        
        /// <summary>Minimum value</summary>
        public double Min { get; init; }
        
        /// <summary>Maximum value</summary>
        public double Max { get; init; }
        
        /// <summary>Minimum delta value accepted (normalized)</summary>
        public double Delta { get; init; }
        
        /// <summary>Value type (float/integer/boolean/string/enum)</summary>
        public string Type { get; init; } = string.Empty;
    }

    /// <summary>
    /// Enum value (blob-bg-b sub-schema)
    /// </summary>
    public record EnumValue
    {
        /// <summary>Enum ID</summary>
        public int Id { get; init; }
        
        /// <summary>Enum name/label</summary>
        public string Name { get; init; } = string.Empty;
    }

    /// <summary>
    /// GET /console/data/definitions2/{path} - Response
    /// OpenAPI Schema: blob-bx-e
    /// </summary>
    public record ConsoleDataDefinitions2Response
    {
        /// <summary>Node details</summary>
        public DataNodeDetails? Node { get; init; }
        
        /// <summary>Value details</summary>
        public DataValueDetails? Value { get; init; }
    }

    /// <summary>
    /// Node details (blob-bx-e$node sub-schema)
    /// </summary>
    public record DataNodeDetails
    {
        /// <summary>Default filter type</summary>
        public int DefaultFilterType { get; init; }
    }

    /// <summary>
    /// Value details (blob-bx-e$value sub-schema)
    /// </summary>
    public record DataValueDetails
    {
        /// <summary>Accepted enums (for enum type)</summary>
        public EnumValue[] Enums { get; init; } = Array.Empty<EnumValue>();
        
        /// <summary>Value unit (e.g., "dB", "Hz")</summary>
        public string Unit { get; init; } = string.Empty;
        
        /// <summary>True if value is tappable</summary>
        public bool Tap { get; init; }
        
        /// <summary>Minimum value</summary>
        public double Min { get; init; }
        
        /// <summary>Maximum value</summary>
        public double Max { get; init; }
        
        /// <summary>Minimum delta value accepted (normalized)</summary>
        public double Delta { get; init; }
        
        /// <summary>Value title</summary>
        public string Title { get; init; } = string.Empty;
        
        /// <summary>Value type (float/integer/boolean/string/enum)</summary>
        public string Type { get; init; } = string.Empty;
        
        /// <summary>String constraints (if value is string type)</summary>
        public string[] Constraints { get; init; } = Array.Empty<string>();
    }

    // NOTE: POST /console/data/unsubscribe uses ConsoleDataSubscribeRequest (blob-bw-j) - already defined in Phase 2

    // ========================================
    // Phase 5: Console Authentication & Mix Targets
    // ========================================

    /// <summary>
    /// GET /console/auth/info - Response
    /// OpenAPI Schema: blob-bz-c
    /// Returns the security details about this mixer
    /// </summary>
    public record ConsoleAuthInfoResponse
    {
        /// <summary>Array of valid usernames for authentication</summary>
        public string[] Users { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// POST /console/auth/login - Request
    /// OpenAPI Schema: blob-bz-b
    /// Logs in to the mixer using the given credentials
    /// </summary>
    public record ConsoleAuthLoginRequest
    {
        /// <summary>Username</summary>
        public string User { get; init; } = string.Empty;
        
        /// <summary>Password</summary>
        public string Password { get; init; } = string.Empty;
    }

    /// <summary>
    /// POST /console/auth/login - Response
    /// OpenAPI Schema: blob-bz-a
    /// </summary>
    public record ConsoleAuthLoginResponse
    {
        /// <summary>Whether login was successful</summary>
        public bool Success { get; init; }
    }

    /// <summary>
    /// GET /console/mixTargets - Response
    /// OpenAPI Schema: blob-bw-i
    /// Returns all signal sinks which can be used as mix target for the channels
    /// </summary>
    public record ConsoleMixTargetsResponse
    {
        /// <summary>List of available mix targets</summary>
        public MixTarget[] Targets { get; init; } = Array.Empty<MixTarget>();
    }

    /// <summary>
    /// Mix target details
    /// OpenAPI Schema: blob-bw-i$a
    /// </summary>
    public record MixTarget
    {
        /// <summary>Whether this target is a channel</summary>
        public bool IsChannel { get; init; }
        
        /// <summary>Target name</summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>Channel type details (if IsChannel is true)</summary>
        public MixTargetChannelType? ChannelType { get; init; }
        
        /// <summary>Target ID</summary>
        public int Id { get; init; }
        
        /// <summary>Channel index (if IsChannel is true)</summary>
        public int ChannelIndex { get; init; }
    }

    /// <summary>
    /// Mix target channel type details
    /// Nested object within blob-bw-i$a
    /// </summary>
    public record MixTargetChannelType
    {
        /// <summary>Channel offset</summary>
        public int Offset { get; init; }
        
        /// <summary>Whether this is a stereo channel</summary>
        public bool Stereo { get; init; }
        
        /// <summary>Channel type name</summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>Number of channels of this type</summary>
        public int Count { get; init; }
        
        /// <summary>Short name for this channel type</summary>
        public string ShortName { get; init; } = string.Empty;
        
        /// <summary>Channel type ID</summary>
        public int Type { get; init; }
    }

    // ========================================
    // Phase 6: Console Metering
    // ========================================

    /// <summary>
    /// POST /console/metering/subscribe - Request
    /// OpenAPI Schema: blob-bw-k
    /// DEPRECATED: Use ConsoleMeteringSubscribe2Request instead
    /// Subscribes to metering values of channels (level meters)
    /// </summary>
    [Obsolete("Use ConsoleMeteringSubscribe2Request instead - /console/metering2/subscribe")]
    public record ConsoleMeteringSubscribeRequest
    {
        /// <summary>Subscription ID</summary>
        public int Id { get; init; }
        
        /// <summary>Update interval in milliseconds</summary>
        public int Interval { get; init; }
        
        /// <summary>Use binary format (base64 encoded int16 fixed-point)</summary>
        public bool Binary { get; init; }
        
        /// <summary>Channel indices to monitor</summary>
        public int[] ChannelIndices { get; init; } = Array.Empty<int>();
    }

    /// <summary>
    /// POST /console/metering/unsubscribe - Request
    /// OpenAPI Schema: blob-bw-b
    /// Unsubscribes the metering request with the given id
    /// </summary>
    public record ConsoleMeteringUnsubscribeRequest
    {
        /// <summary>Subscription ID to unsubscribe</summary>
        public int Id { get; init; }
    }

    /// <summary>
    /// POST /console/metering2/subscribe - Request
    /// OpenAPI Schema: blob-bw-l
    /// Subscribes to metering values (improved API)
    /// </summary>
    public record ConsoleMeteringSubscribe2Request
    {
        /// <summary>Subscription ID</summary>
        public int Id { get; init; }
        
        /// <summary>Update interval in milliseconds</summary>
        public int Interval { get; init; }
        
        /// <summary>Use binary format (base64 encoded int16 fixed-point)</summary>
        public bool Binary { get; init; }
        
        /// <summary>Metering parameters (channel index and type)</summary>
        public MeteringParam[] Params { get; init; } = Array.Empty<MeteringParam>();
    }

    /// <summary>
    /// Metering parameter for metering2 subscription
    /// OpenAPI Schema: blob-bw-d
    /// </summary>
    public record MeteringParam
    {
        /// <summary>Channel or bus index</summary>
        public int Index { get; init; }
        
        /// <summary>Metering type (pre/post fader, etc.)</summary>
        public int Type { get; init; }
    }

    // NOTE: All metering endpoints return 204 No Content (NO RESPONSE BODY!)

    // ========================================
    // Phase 7: App Presets (/app/presets/*)
    // ========================================

    /// <summary>
    /// Channel or bus reference
    /// OpenAPI Schema: blob-wr-a
    /// Used in: preset channel operations
    /// </summary>
    public record ChannelReference
    {
        /// <summary>Channel index offset</summary>
        public int Offset { get; init; }
        
        /// <summary>Channel type (input, aux, etc.)</summary>
        public int Type { get; init; }
    }

    /// <summary>
    /// Preset scope definition
    /// OpenAPI Schema: blob-by-e
    /// Used in: GET /app/presets/scopes response
    /// </summary>
    public record AppPresetsScope
    {
        /// <summary>Scope name (e.g., "EQ", "Gate", "Compressor")</summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>Bit position for this scope</summary>
        public int BitPos { get; init; }
    }

    /// <summary>
    /// GET /app/presets/scopes - Response
    /// OpenAPI Schema: blob-by-f
    /// Returns available preset scopes (channel and global)
    /// </summary>
    public record AppPresetsScopesResponse
    {
        /// <summary>Channel-specific scopes (e.g., channel EQ, gate)</summary>
        public AppPresetsScope[] Channel { get; init; } = Array.Empty<AppPresetsScope>();
        
        /// <summary>Global scopes (e.g., main mix settings)</summary>
        public AppPresetsScope[] Global { get; init; } = Array.Empty<AppPresetsScope>();
    }

    /// <summary>
    /// POST /app/presets/channel/apply - Request
    /// OpenAPI Schema: blob-by-b
    /// Recalls the given MS Preset data to a channel
    /// </summary>
    public record AppPresetsChannelApplyRequest
    {
        /// <summary>Preset data (freeform object)</summary>
        public object Data { get; init; } = new();
        
        /// <summary>Scope bitmask (which settings to apply)</summary>
        public int Scope { get; init; }
        
        /// <summary>Target channel reference</summary>
        public ChannelReference Channel { get; init; } = new();
    }

    // NOTE: POST /app/presets/channel/apply returns 204 No Content (NO RESPONSE BODY!)

    /// <summary>
    /// POST /app/presets/channel/create - Request
    /// OpenAPI Schema: blob-by-c
    /// Returns the state of a single channel as MS Preset
    /// </summary>
    public record AppPresetsChannelCreateRequest
    {
        /// <summary>Source channel reference</summary>
        public ChannelReference Src { get; init; } = new();
        
        /// <summary>Scope bitmask (which settings to capture)</summary>
        public int Scope { get; init; }
    }

    /// <summary>
    /// POST /app/presets/channel/create - Response
    /// OpenAPI Schema: blob-by-b (same as apply request)
    /// Returns channel preset data
    /// </summary>
    public record AppPresetsChannelCreateResponse
    {
        /// <summary>Preset data (freeform object)</summary>
        public object Data { get; init; } = new();
        
        /// <summary>Scope bitmask</summary>
        public int Scope { get; init; }
        
        /// <summary>Source channel reference</summary>
        public ChannelReference Channel { get; init; } = new();
    }

    /// <summary>
    /// Channel scope mapping for scene presets
    /// OpenAPI Schema: blob-by-a
    /// Used in: scene preset operations
    /// </summary>
    public record ChannelScopeMapping
    {
        /// <summary>Source channel reference</summary>
        public ChannelReference Src { get; init; } = new();
        
        /// <summary>Scope bitmask</summary>
        public int Scope { get; init; }
        
        /// <summary>Destination channel reference</summary>
        public ChannelReference Dest { get; init; } = new();
    }

    /// <summary>
    /// POST /app/presets/scenes/apply - Request
    /// POST /app/presets/scenes/create - Response
    /// OpenAPI Schema: blob-by-d
    /// Scene preset data with global and channel scopes
    /// </summary>
    public record AppPresetsSceneData
    {
        /// <summary>Scene data (freeform object, OpenAPI: blob-bn-c)</summary>
        public object Data { get; init; } = new();
        
        /// <summary>Global scope bitmask</summary>
        public int GlobalScope { get; init; }
        
        /// <summary>Channel scope mappings</summary>
        public ChannelScopeMapping[] ChannelScopes { get; init; } = Array.Empty<ChannelScopeMapping>();
    }

    // NOTE: POST /app/presets/scenes/apply returns 204 No Content (NO RESPONSE BODY!)
    // NOTE: POST /app/presets/scenes/create returns AppPresetsSceneData

    /// <summary>
    /// GET /app/presets/lastError - Response
    /// OpenAPI Schema: blob-bx-i
    /// Returns errors/warnings from last preset recall
    /// </summary>
    public record AppPresetsLastErrorResponse
    {
        /// <summary>Warning messages</summary>
        public string[] Warnings { get; init; } = Array.Empty<string>();
        
        /// <summary>Error messages</summary>
        public string[] Errors { get; init; } = Array.Empty<string>();
    }

    // ========================================
    // Phase 8: App IDCA & UI (/app/idcas/*, /app/ui/*)
    // ========================================

    /// <summary>
    /// POST /app/idcas - Request
    /// POST /app/idcas/{index} - Request (modify IDCA)
    /// OpenAPI Schema: blob-bw-e
    /// Creates or modifies an IDCA (Input/Direct Channel Assignment)
    /// </summary>
    public record AppIdcaRequest
    {
        /// <summary>IDCA member channel references</summary>
        public ChannelReference[] Members { get; init; } = Array.Empty<ChannelReference>();
    }

    /// <summary>
    /// POST /app/idcas - Response
    /// POST /app/idcas/{index} - Response (modify IDCA)
    /// OpenAPI Schema: blob-bw-c
    /// Returns IDCA with assigned index
    /// </summary>
    public record AppIdcaResponse
    {
        /// <summary>IDCA member channel references</summary>
        public ChannelReference[] Members { get; init; } = Array.Empty<ChannelReference>();
        
        /// <summary>IDCA index in data tree (appears as 'idca.X')</summary>
        public int Index { get; init; }
    }

    /// <summary>
    /// POST /app/idcas/rearrange - Request
    /// OpenAPI Schema: blob-bw-h
    /// Updates IDCA order by source indices
    /// </summary>
    public record AppIdcaRearrangeRequest
    {
        /// <summary>Source indices of existing IDCAs (new order)</summary>
        public int[] NewIndices { get; init; } = Array.Empty<int>();
    }

    /// <summary>
    /// POST /app/idcas/rearrange - Response
    /// OpenAPI Schema: blob-bw-c$a
    /// Returns all IDCAs in new order
    /// </summary>
    public record AppIdcaRearrangeResponse
    {
        /// <summary>All IDCAs after rearranging</summary>
        public AppIdcaResponse[] Dcas { get; init; } = Array.Empty<AppIdcaResponse>();
    }

    // NOTE: POST /app/idcas/{index}/delete returns 204 No Content (NO RESPONSE BODY!)

    /// <summary>
    /// GET /app/ui/selectedChannel - Response
    /// GET /app/ui/selectedChannel/{nameOrIndex} - Response
    /// OpenAPI Schema: blob-bx-a
    /// Returns currently selected channel info
    /// </summary>
    public record AppUiSelectedChannelResponse
    {
        /// <summary>Generic channel name (e.g., "Input 1")</summary>
        public string GenericName { get; init; } = string.Empty;
        
        /// <summary>Custom channel name</summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>Channel index</summary>
        public int Index { get; init; }
    }

    // ========================================
    // Phase 9: App Network & Misc (/app/network/*, /app/save)
    // ========================================

    /// <summary>
    /// Network interface information
    /// OpenAPI Schema: blob-bw-g$a
    /// </summary>
    public record NetworkInterface
    {
        /// <summary>Human-readable interface name</summary>
        public string DisplayName { get; init; } = string.Empty;
        
        /// <summary>Whether this is the primary interface</summary>
        public bool IsPrimary { get; init; }
        
        /// <summary>System interface name</summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>IP address</summary>
        public string IpAddress { get; init; } = string.Empty;
        
        /// <summary>Subnet mask</summary>
        public string SubnetMask { get; init; } = string.Empty;
        
        /// <summary>Whether primary interface is overridden</summary>
        public bool OverridePrimary { get; init; }
    }

    /// <summary>
    /// GET /app/network/interfaces - Response
    /// POST /app/network/interfaces/primary - Response
    /// OpenAPI Schema: blob-bw-g
    /// Returns list of network interfaces
    /// </summary>
    public record NetworkInterfacesResponse
    {
        /// <summary>All network interfaces</summary>
        public NetworkInterface[] Interfaces { get; init; } = Array.Empty<NetworkInterface>();
    }

    /// <summary>
    /// POST /app/network/interfaces/primary - Request
    /// OpenAPI Schema: blob-bw-f
    /// Sets primary network interface
    /// </summary>
    public record NetworkInterfacePrimaryRequest
    {
        /// <summary>Name of interface to set as primary</summary>
        public string Name { get; init; } = string.Empty;
    }

    // NOTE: POST /app/save returns 204 No Content (NO RESPONSE BODY!)

    // ========================================
    // Legacy / Deprecated
    // ========================================

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
