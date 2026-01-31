/**
 * IConsoleClient - REST client interface for /console/* endpoints
 * 
 * SOURCE OF TRUTH: docs/openapi.json (http://localhost:8045/openapi.json)
 * Public API mirrors REST endpoints exactly (ADR-002: #11)
 * Endpoint group: /console/* (console state)
 * 
 * Implements:
 * - #4 REQ-F-001: HTTP Client Transport
 * - #6 REQ-F-003: Console Data Reading
 * 
 * Architecture:
 * - #10 ADR-001: .NET 8 Runtime
 * - #11 ADR-002: HTTP Transport + REST Mirror Naming Policy
 * 
 * Design: Phase 1 Detailed Design (04-design/phase-1-detailed-design.md)
 * Reference: docs/openapi-spec-usage.md
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/4
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/6
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/11
 */

using System.Threading;
using System.Threading.Tasks;
using MixingStation.Client.Models;

namespace MixingStation.Client.Console
{
    /// <summary>
    /// REST client for /console/* endpoints (console state).
    /// Public API mirrors REST endpoints exactly (ADR-002).
    /// OpenAPI: docs/openapi.json
    /// </summary>
    public interface IConsoleClient
    {
        /// <summary>
        /// GET /console/information - Returns channel architecture details.
        /// OpenAPI: Response blob-bx-b (totalChannels, channelColors, channelTypes, rtaFrequencies, dbfsOffset)
        /// Naming: /{group}/{segment} → Get{Segment}Async()
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Console information (channel architecture, colors, types).</returns>
        Task<ConsoleInformationResponse> GetInformationAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /console/data/get/{path}/{format} - Read specific console parameter.
        /// OpenAPI: Response blob-bx-m (format, value)
        /// Naming: /{group}/{segment1}/{segment2}/{param1}/{param2} → Get{Segment1}{Segment2}Async(param1, param2)
        /// </summary>
        /// <param name="path">Console data path (e.g., "ch.0.name", "ch.0.config.gain").</param>
        /// <param name="format">Value format: "val" (actual value) or "norm" (normalized 0-1).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Console data value (format and value).</returns>
        Task<ConsoleDataValue> GetDataGetAsync(
            string path,
            string format,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /console/data/subscribe - Subscribe to data changes via WebSocket.
        /// OpenAPI: Request blob-bw-j (path, format), Response 204 No Content
        /// Naming: /{group}/{segment1}/{segment2} → Post{Segment1}{Segment2}Async()
        /// </summary>
        /// <param name="request">Subscription parameters (path pattern, format).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PostDataSubscribeAsync(
            ConsoleDataSubscribeRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /console/data/set/{path}/{format} - Write value to console parameter.
        /// OpenAPI: Request blob-bx-m (format, value), Response blob-bx-m
        /// Naming: /{group}/{segment1}/{segment2}/{param1}/{param2} → Post{Segment1}{Segment2}Async(param1, param2, request)
        /// </summary>
        /// <param name="path">Console data path (e.g., "ch.0.config.gain", "ch.0.mute").</param>
        /// <param name="format">Value format: "val" (actual value) or "norm" (normalized 0-1).</param>
        /// <param name="request">Value to set (format and value).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Confirmed value after set (format and value).</returns>
        Task<ConsoleDataValue> PostDataSetAsync(
            string path,
            string format,
            ConsoleDataValue request,
            CancellationToken cancellationToken = default);
    }
}
