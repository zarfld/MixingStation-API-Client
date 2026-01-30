/**
 * INormalizer - Transforms API responses into CanonicalModel v0
 * 
 * NOT a REST client - internal transformation logic
 * 
 * Implements:
 * - #7 REQ-F-004: Data Normalization
 * 
 * Architecture:
 * - #12 ADR-003: CanonicalModel v0
 * 
 * Design: Phase 1 Detailed Design (04-design/phase-1-detailed-design.md)
 * - Stereo pair detection algorithm (O(n))
 * - Color code mapping (OFF → "N/A")
 * - Patch source formatting
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/7
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/12
 */

using System.Threading.Tasks;
using MixingStation.Client.Models;

namespace MixingStation.Client.Normalization
{
    /// <summary>
    /// Transforms API responses into CanonicalModel v0 (ADR-003).
    /// NOT a REST client - internal transformation logic.
    /// </summary>
    public interface INormalizer
    {
        /// <summary>
        /// Normalize mixer metadata + console data into CanonicalModel v0.
        /// </summary>
        /// <remarks>
        /// Normalization rules (per ADR-003):
        /// - Color mapping: API integers → hex codes (OFF → "N/A")
        /// - Stereo pairs: Detect consecutive channels with link flag
        /// - Channel numbering: 0-based API → 1-based CanonicalModel
        /// - "N/A" handling: Explicit "N/A" for unset values
        /// </remarks>
        /// <param name="mixerInfo">Mixer metadata from /app/mixers/current.</param>
        /// <param name="consoleData">Console state from /console/data.</param>
        /// <returns>Normalized canonical model.</returns>
        Task<CanonicalModel> NormalizeAsync(
            AppMixersCurrentResponse mixerInfo, 
            ConsoleDataResponse consoleData);
    }
}
