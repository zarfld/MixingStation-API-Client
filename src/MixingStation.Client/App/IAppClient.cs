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
    }
}
