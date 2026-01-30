/**
 * IExcelExporter - Exports CanonicalModel to Excel files
 * 
 * NOT a REST client - internal export logic
 * 
 * Implements:
 * - #8 REQ-F-005: Excel Export (InputList + PatchingList)
 * 
 * Architecture:
 * - #13 ADR-004: Excel Export Library (EPPlus 7.x)
 * 
 * Design: Phase 1 Detailed Design (04-design/phase-1-detailed-design.md)
 * 
 * Output files:
 * - InputList.xlsx: Channel names, colors, stereo pairs
 * - PatchingList.xlsx: Patch routing information
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/8
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/13
 */

using System.Threading.Tasks;
using MixingStation.Client.Models;

namespace MixingStation.Client.Export
{
    /// <summary>
    /// Exports CanonicalModel to Excel files (ADR-004).
    /// NOT a REST client - internal export logic.
    /// </summary>
    public interface IExcelExporter
    {
        /// <summary>
        /// Export CanonicalModel to InputList.xlsx and PatchingList.xlsx.
        /// </summary>
        /// <param name="model">Canonical model to export.</param>
        /// <param name="outputDirectory">Output directory path.</param>
        /// <returns>Export result (file paths, success/failure).</returns>
        Task<ExportResult> ExportAsync(
            CanonicalModel model, 
            string outputDirectory);
    }
}
