/**
 * IConsoleClient - REST client interface for /console/* endpoints
 * 
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
    /// </summary>
    public interface IConsoleClient
    {
        /// <summary>
        /// GET /console/data - Read full console state (all channels).
        /// Naming: /{group}/{verb} → {Verb}Async() (single-verb exception)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Console data (obfuscated blob-* schema).</returns>
        Task<ConsoleDataResponse> DataAsync(
            CancellationToken cancellationToken = default);
    }
}
