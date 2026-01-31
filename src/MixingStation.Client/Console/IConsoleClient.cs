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
        // ========================================
        // Phase 4: Console Data Discovery
        // ========================================

        /// <summary>
        /// GET /console/data/categories - Returns all data categories.
        /// OpenAPI: Response blob-bx-c (empty object - dynamic content)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Categories dictionary (dynamic key-value pairs).</returns>
        Task<ConsoleDataCategoriesResponse> GetDataCategoriesAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /console/data/paths - Returns all data paths available for the current mixer.
        /// OpenAPI: Response blob-bx-f (recursive structure: val array + child dictionary)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Root data paths tree (recursive structure).</returns>
        Task<ConsoleDataPathsResponse> GetDataPathsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /console/data/paths/{path} - Returns a sub-path.
        /// OpenAPI: Response blob-bx-f (recursive structure: val array + child dictionary)
        /// </summary>
        /// <param name="path">Console data path (e.g., "ch", "ch.0").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Sub-path tree (recursive structure).</returns>
        Task<ConsoleDataPathsResponse> GetDataPathsAsync(
            string path,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /console/data/definitions/{path} - Returns the data definitions for the given path.
        /// DEPRECATED: Use GetDataDefinitions2Async instead.
        /// OpenAPI: Response blob-bx-d (definitions dictionary)
        /// </summary>
        /// <param name="path">Console data path (e.g., "ch.0.config.gain").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Data definitions dictionary.</returns>
        [Obsolete("Use GetDataDefinitions2Async instead - /console/data/definitions2/{path}")]
        Task<ConsoleDataDefinitionsResponse> GetDataDefinitionsAsync(
            string path,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /console/data/definitions2/{path} - Returns the data definitions for the given paths.
        /// OpenAPI: Response blob-bx-e (node + value details)
        /// </summary>
        /// <param name="path">Console data path (e.g., "ch.0.config.gain").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Data definitions (node and value details).</returns>
        Task<ConsoleDataDefinitions2Response> GetDataDefinitions2Async(
            string path,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /console/data/unsubscribe - Unsubscribes the data matching the given pattern.
        /// OpenAPI: Request blob-bw-j (path, format), Response 204 No Content
        /// WebSocket-Only: This endpoint only works after WebSocket connection is established.
        /// </summary>
        /// <param name="request">Unsubscribe parameters (path pattern, format).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PostDataUnsubscribeAsync(
            ConsoleDataSubscribeRequest request,
            CancellationToken cancellationToken = default);    }
}
