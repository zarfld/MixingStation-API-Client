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
    }
}
