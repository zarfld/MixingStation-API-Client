/**
 * CanonicalModel - Normalized representation of console state (v0)
 * 
 * Architecture:
 * - #12 ADR-003: CanonicalModel v0
 * 
 * Design: Phase 1 Detailed Design (04-design/phase-1-detailed-design.md)
 * 
 * Schema rules:
 * - 1-based channel numbering (user-friendly)
 * - Explicit "N/A" for unset values
 * - Bidirectional stereo pair links
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/12
 */

using System.Collections.Generic;

namespace MixingStation.Client.Models
{
    /// <summary>
    /// CanonicalModel v0 - Normalized representation of console state.
    /// </summary>
    public class CanonicalModel
    {
        public MixerInfo Mixer { get; set; } = new MixerInfo();
        public List<ChannelInfo> Channels { get; set; } = new List<ChannelInfo>();
        public List<PatchInfo> Patches { get; set; } = new List<PatchInfo>();
    }

    /// <summary>
    /// Mixer metadata (type, firmware, capabilities).
    /// </summary>
    public class MixerInfo
    {
        /// <summary>Mixer type (e.g., "X32", "M32", "Wing")</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Firmware version (e.g., "4.08")</summary>
        public string Firmware { get; set; } = string.Empty;

        /// <summary>Physical input count</summary>
        public int InputCount { get; set; }

        /// <summary>Physical output count</summary>
        public int OutputCount { get; set; }
    }

    /// <summary>
    /// Channel information (name, color, stereo pairing).
    /// </summary>
    public class ChannelInfo
    {
        /// <summary>Channel number (1-based)</summary>
        public int ChannelNumber { get; set; }

        /// <summary>User-defined name</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Hex color code or "N/A"</summary>
        public string Color { get; set; } = "N/A";

        /// <summary>True if part of stereo pair</summary>
        public bool IsStereo { get; set; }

        /// <summary>Linked channel number (if stereo), null otherwise</summary>
        public int? StereoPairWith { get; set; }
    }

    /// <summary>
    /// Patch routing information.
    /// </summary>
    public class PatchInfo
    {
        /// <summary>Channel number (1-based)</summary>
        public int ChannelNumber { get; set; }

        /// <summary>Patch source (e.g., "Local 1", "AES50-A 1")</summary>
        public string PatchSource { get; set; } = string.Empty;

        /// <summary>Patch type ("Local", "AES50-A", "AES50-B", etc.)</summary>
        public string PatchType { get; set; } = string.Empty;
    }
}
