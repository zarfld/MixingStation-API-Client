/**
 * IAppClient - REST client interface for /app/* endpoints
 * 
 * SOURCE OF TRUTH: docs/openapi.json (http://localhost:8045/openapi.json)
 * Public API mirrors REST endpoints exactly (ADR-002: #11)
 * Endpoint group: /app/* (application lifecycle)
 * 
 * Implements:
 * - #4 REQ-F-001: HTTP Client Transport
 * - #5 REQ-F-002: Application State Reading
 * 
 * Architecture:
 * - #10 ADR-001: .NET 8 Runtime
 * - #11 ADR-002: HTTP Transport + REST Mirror Naming Policy
 * 
 * Design: Phase 1 Detailed Design (04-design/phase-1-detailed-design.md)
 * Reference: docs/openapi-spec-usage.md
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/4
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/5
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/11
 */

using System.Threading;
using System.Threading.Tasks;
using MixingStation.Client.Models;

namespace MixingStation.Client.App
{
    /// <summary>
    /// REST client for /app/* endpoints (application lifecycle).
    /// Public API mirrors REST endpoints exactly (ADR-002).
    /// OpenAPI: docs/openapi.json
    /// </summary>
    public interface IAppClient
    {
        /// <summary>
        /// POST /app/mixers/connect - Connects to mixer with given IP and model id.
        /// OpenAPI: Request blob-ca-b (consoleId, ip), Response 204 No Content
        /// Naming: /{group}/{segment1}/{segment2} → Post{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="request">Connection parameters (consoleId, ip).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PostMixersConnectAsync(
            AppMixersConnectRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /app/mixers/current - Returns meta-data of currently used mixer.
        /// OpenAPI: Response blob-ca-d$b
        /// Naming: /{group}/{segment1}/{segment2} → Get{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Mixer info (consoleId, currentModel, firmwareVersion, ipAddress, etc.).</returns>
        Task<AppMixersCurrentResponse> GetMixersCurrentAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /app/state - Returns current application state and progress.
        /// OpenAPI: Response blob-bw-a (msg, progress, state, topState)
        /// Naming: /{group}/{segment} → Get{Segment}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Application state (msg, progress 0-100, state, topState).</returns>
        Task<AppStateResponse> GetStateAsync(
            CancellationToken cancellationToken = default);

        // ========================================
        // Phase 3: Mixer Lifecycle Management
        // ========================================

        /// <summary>
        /// GET /app/mixers/available - Returns all supported mixer models.
        /// OpenAPI: Response blob-ca-d (consoles array)
        /// Naming: /{group}/{segment1}/{segment2} → Get{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of available mixer consoles with models and capabilities.</returns>
        Task<AppMixersAvailableResponse> GetMixersAvailableAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/mixers/search - Starts searching for mixers of the given variant.
        /// OpenAPI: Request blob-ca-f (consoleId), Response 204 No Content
        /// Naming: /{group}/{segment1}/{segment2} → Post{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="request">Search parameters (consoleId).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PostMixersSearchAsync(
            AppMixersSearchRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /app/mixers/searchResults - Returns list of all mixers found in the network.
        /// OpenAPI: Response blob-ca-e (results array)
        /// Naming: /{group}/{segment1}/{segment2} → Get{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Search results with found mixers (modelId, ip, name, model, version).</returns>
        Task<AppMixersSearchResultsResponse> GetMixersSearchResultsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/mixers/disconnect - Stops the network stack and returns to initial app state.
        /// OpenAPI: Response 204 No Content
        /// Naming: /{group}/{segment1}/{segment2} → Post{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PostMixersDisconnectAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/mixers/offline - Starts offline mode for mixers of the given series and model.
        /// OpenAPI: Request blob-ca-g (consoleId, modelId, model), Response 204 No Content
        /// Naming: /{group}/{segment1}/{segment2} → Post{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="request">Offline mode parameters (consoleId, modelId, model).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PostMixersOfflineAsync(
            AppMixersOfflineRequest request,
            CancellationToken cancellationToken = default);

        // ========================================
        // Phase 7: App Presets Management
        // ========================================

        /// <summary>
        /// GET /app/presets/scopes - Returns all available preset scopes (channel/global).
        /// OpenAPI: Response blob-by-f (channel[], global[])
        /// Naming: /{group}/{segment1}/{segment2} → Get{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Available scopes for channel and global presets.</returns>
        Task<AppPresetsScopesResponse> GetPresetsScopesAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/presets/channel/apply - Recalls the given MS Preset data to a channel.
        /// OpenAPI: Request blob-by-b, Response 204 No Content
        /// Naming: /{group}/{segment1}/{segment2}/{segment3} → Post{Segment1}{Segment2}{Segment3}Async()
        /// </summary>
        /// <param name="request">Preset data (data, scope, channel).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PostPresetsChannelApplyAsync(
            AppPresetsChannelApplyRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/presets/channel/create - Returns the state of a single channel as MS Preset.
        /// OpenAPI: Request blob-by-c, Response blob-by-b
        /// Naming: /{group}/{segment1}/{segment2}/{segment3} → Post{Segment1}{Segment2}{Segment3}Async()
        /// </summary>
        /// <param name="request">Channel reference and scope (src, scope).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Channel preset data.</returns>
        Task<AppPresetsChannelCreateResponse> PostPresetsChannelCreateAsync(
            AppPresetsChannelCreateRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/presets/scenes/apply - Recalls the given MS Scene data.
        /// OpenAPI: Request blob-by-d, Response 204 No Content
        /// Naming: /{group}/{segment1}/{segment2}/{segment3} → Post{Segment1}{Segment2}{Segment3}Async()
        /// </summary>
        /// <param name="request">Scene data (data, globalScope, channelScopes[]).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PostPresetsScenesApplyAsync(
            AppPresetsSceneData request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/presets/scenes/create - Returns the current mixer state as MS Scene.
        /// OpenAPI: Response blob-by-d
        /// Naming: /{group}/{segment1}/{segment2}/{segment3} → Post{Segment1}{Segment2}{Segment3}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Scene preset data.</returns>
        Task<AppPresetsSceneData> PostPresetsScenesCreateAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /app/presets/lastError - Returns errors/warnings from last preset recall.
        /// OpenAPI: Response blob-bx-i (warnings[], errors[])
        /// Naming: /{group}/{segment1}/{segment2} → Get{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Error and warning messages.</returns>
        Task<AppPresetsLastErrorResponse> GetPresetsLastErrorAsync(
            CancellationToken cancellationToken = default);

        // ========================================
        // Phase 8: App IDCA & UI Management
        // ========================================

        /// <summary>
        /// POST /app/idcas - Creates a new IDCA (Input/Direct Channel Assignment).
        /// OpenAPI: Request blob-bw-e, Response blob-bw-c
        /// Naming: /{group}/{segment1} → Post{Segment1}Async()
        /// </summary>
        /// <param name="request">IDCA members (channel references).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created IDCA with assigned index.</returns>
        Task<AppIdcaResponse> PostIdcasAsync(
            AppIdcaRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/idcas/{index} - Modifies an existing IDCA.
        /// OpenAPI: Request blob-bw-e, Response blob-bw-c
        /// Naming: /{group}/{segment1}/{param} → Post{Segment1}Async(param, ...)
        /// </summary>
        /// <param name="index">IDCA index to modify.</param>
        /// <param name="request">Updated IDCA members.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Modified IDCA.</returns>
        Task<AppIdcaResponse> PostIdcasAsync(
            string index,
            AppIdcaRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/idcas/{index}/delete - Deletes an IDCA.
        /// OpenAPI: Response 204 No Content
        /// Naming: /{group}/{segment1}/{param}/{segment2} → Post{Segment1}{Segment2}Async(param)
        /// </summary>
        /// <param name="index">IDCA index to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PostIdcasDeleteAsync(
            string index,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/idcas/rearrange - Updates IDCA order.
        /// OpenAPI: Request blob-bw-h, Response blob-bw-c$a
        /// Naming: /{group}/{segment1}/{segment2} → Post{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="request">New order (source indices of existing IDCAs).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All IDCAs in new order.</returns>
        Task<AppIdcaRearrangeResponse> PostIdcasRearrangeAsync(
            AppIdcaRearrangeRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /app/ui/selectedChannel - Returns currently selected channel.
        /// OpenAPI: Response blob-bx-a (genericName, name, index)
        /// Naming: /{group}/{segment1}/{segment2} → Get{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Selected channel info.</returns>
        Task<AppUiSelectedChannelResponse> GetUiSelectedChannelAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /app/ui/selectedChannel/{nameOrIndex} - Sets selected channel by name or index.
        /// OpenAPI: Response blob-bx-a (genericName, name, index)
        /// Naming: /{group}/{segment1}/{segment2}/{param} → Get{Segment1}{Segment2}Async(param)
        /// </summary>
        /// <param name="nameOrIndex">Channel name or index to select.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Selected channel info.</returns>
        Task<AppUiSelectedChannelResponse> GetUiSelectedChannelAsync(
            string nameOrIndex,
            CancellationToken cancellationToken = default);

        // ========================================
        // Phase 9: App Network & Misc
        // ========================================

        /// <summary>
        /// GET /app/network/interfaces - Returns all network interfaces and their status.
        /// OpenAPI: Response blob-bw-g (interfaces[])
        /// Naming: /{group}/{segment1}/{segment2} → Get{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All network interfaces with their status.</returns>
        Task<NetworkInterfacesResponse> GetNetworkInterfacesAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/network/interfaces/primary - Overrides primary network interface.
        /// OpenAPI: Request blob-bw-f (name), Response blob-bw-g (interfaces[])
        /// Naming: /{group}/{segment1}/{segment2}/{segment3} → Post{Segment1}{Segment2}{Segment3}Async()
        /// Must be set before starting any search/connection process.
        /// </summary>
        /// <param name="request">Interface name to set as primary.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated network interfaces status.</returns>
        Task<NetworkInterfacesResponse> PostNetworkInterfacesPrimaryAsync(
            NetworkInterfacePrimaryRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /app/save - Persists all current app settings.
        /// OpenAPI: Response 204 No Content
        /// Naming: /{group}/{segment} → Post{Segment}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PostSaveAsync(
            CancellationToken cancellationToken = default);
    }
}
