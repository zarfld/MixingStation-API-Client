# Data Model - CanonicalModel v0

**Standard**: ISO/IEC/IEEE 42010:2011 (Data Architecture)  
**ADR**: ADR-003 (CanonicalModel v0 Structure)  
**Phase**: 03 - Architecture  
**Date**: 2026-01-30  

## Overview

CanonicalModel v0 is a console-agnostic, normalized data structure representing mixer configurations. It abstracts away vendor-specific differences (Behringer X32, Midas M32, Behringer Wing) into a unified model suitable for generating input lists and patching sheets.

---

## Design Principles

1. **Console-Agnostic**: Same structure for X32, M32, Wing, etc.
2. **Explicit N/A Handling**: Missing data marked as `"N/A"` (not null, not empty string)
3. **Minimal Phase 1 Scope**: Only fields needed for InputList + PatchingList
4. **Extensible**: Can add fields in v1 without breaking changes
5. **Immutable Where Possible**: Prefer `readonly` properties and `init` setters

---

## Core Model Structure

```csharp
namespace MixingStation.CanonicalModel
{
    /// <summary>
    /// Normalized, console-agnostic representation of mixer configuration.
    /// Version: 0.1 (Phase 1 - Minimal Viable Model)
    /// </summary>
    public class CanonicalModel
    {
        /// <summary>
        /// Mixer metadata (console type, firmware, IP).
        /// </summary>
        public MixerInfo Mixer { get; init; }
        
        /// <summary>
        /// All channels (inputs, aux, buses, etc.).
        /// Ordered by channel number (1-based).
        /// </summary>
        public List<ChannelInfo> Channels { get; init; } = new();
        
        /// <summary>
        /// Physical input patching information.
        /// Maps physical inputs → console channels.
        /// </summary>
        public List<PatchInfo> Patches { get; init; } = new();
    }
}
```

---

## MixerInfo

**Purpose**: Console metadata and connection information.

```csharp
public class MixerInfo
{
    /// <summary>
    /// Console series name (e.g., "Behringer X32", "Midas M32").
    /// Source: /app/mixers/current → consoleSeries
    /// </summary>
    public string ConsoleSeries { get; init; }
    
    /// <summary>
    /// Specific model (e.g., "X32 Compact", "M32R", "Wing").
    /// Source: /app/mixers/current → model
    /// </summary>
    public string Model { get; init; }
    
    /// <summary>
    /// Firmware version (e.g., "4.06").
    /// Source: /app/mixers/current → firmwareVersion
    /// </summary>
    public string FirmwareVersion { get; init; }
    
    /// <summary>
    /// IP address of console (e.g., "192.168.1.100").
    /// Source: /app/mixers/current → ipAddress
    /// </summary>
    public string IpAddress { get; init; }
    
    /// <summary>
    /// Timestamp when data was captured (ISO 8601).
    /// Used in Excel metadata row.
    /// </summary>
    public DateTime CapturedAt { get; init; } = DateTime.UtcNow;
}
```

**Validation Rules**:
- `ConsoleSeries`: Required (not null/empty)
- `Model`: Required
- `FirmwareVersion`: Required
- `IpAddress`: Must be valid IP format (regex validated)

---

## ChannelInfo

**Purpose**: Represents a single console channel (input, aux, bus, etc.).

```csharp
public class ChannelInfo
{
    /// <summary>
    /// Channel number (1-based).
    /// Example: 1, 2, 3, ..., 32
    /// </summary>
    public int ChannelNumber { get; init; }
    
    /// <summary>
    /// User-assigned channel name.
    /// Source: /console/data/get/ch.{N}.config.name/val
    /// Default: "Ch {N}" if missing
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    /// Channel type (Input, Aux, Bus, FX Return, DCA, etc.).
    /// Source: /console/information → channelTypes array
    /// Examples: "Input", "Aux", "Bus", "FX Return", "DCA"
    /// </summary>
    public string Type { get; init; }
    
    /// <summary>
    /// User-assigned color (for visual grouping).
    /// Source: /console/data/get/ch.{N}.config.color/val
    /// Examples: "Red", "#FF0000", "N/A"
    /// "N/A" if not set or not supported by console
    /// </summary>
    public string Color { get; init; } = "N/A";
    
    /// <summary>
    /// Is this channel part of a stereo pair (linked with next channel)?
    /// Source: /console/data/get/ch.{N}.config.link/val (if available)
    /// Examples: true (Ch1+Ch2 stereo), false (mono)
    /// </summary>
    public bool IsMonoInStereo { get; init; } = false;
    
    /// <summary>
    /// Routing information (sends to LR, Aux, Groups).
    /// Phase 1: Minimal - only LR routing
    /// Phase 2: Aux sends, Group assignments
    /// </summary>
    public RoutingInfo Routing { get; init; }
}
```

**Field Mapping Rules**:

| Source API Path | Target Field | Missing Handling |
|----------------|--------------|------------------|
| `/console/information` → `totalChannels` | `ChannelInfo[]` count | Required (fail if missing) |
| `/console/data/get/ch.*.config.name/val` | `Name` | Default to "Ch {N}" |
| `/console/data/get/ch.*.config.color/val` | `Color` | "N/A" |
| `/console/data/get/ch.*.config.link/val` | `IsMonoInStereo` | `false` (assume mono) |

---

## RoutingInfo

**Purpose**: Routing assignments for a channel (sends to LR, Aux, Groups).

```csharp
public class RoutingInfo
{
    /// <summary>
    /// Is channel assigned to main LR (stereo master)?
    /// Source: /console/data/get/ch.{N}.mix.lr.on/val
    /// Phase 1: Simplified - just LR routing
    /// </summary>
    public bool SendToLR { get; init; } = true;
    
    /// <summary>
    /// Aux send assignments (Phase 2 - deferred).
    /// Example: [1, 3, 5] → sends to Aux 1, 3, 5
    /// Phase 1: Empty array
    /// </summary>
    public List<int> AuxSends { get; init; } = new();
    
    /// <summary>
    /// Group/DCA assignments (Phase 2 - deferred).
    /// Example: ["DCA 1", "Group 2"]
    /// Phase 1: Empty array
    /// </summary>
    public List<string> Groups { get; init; } = new();
}
```

**Phase 1 Simplification**:
- Only `SendToLR` is populated
- `AuxSends` and `Groups` are empty (deferred to Phase 2)

---

## PatchInfo

**Purpose**: Physical input patching (maps physical inputs → console channels).

```csharp
public class PatchInfo
{
    /// <summary>
    /// Physical input identifier.
    /// Examples: "Local-01", "S16-A-08", "AES50-B-16", "N/A"
    /// Source: /console/data/get/ch.{N}.preamp.source/val (if available)
    /// "N/A" if not patched or not supported
    /// </summary>
    public string PhysicalInput { get; init; } = "N/A";
    
    /// <summary>
    /// Console channel number this input is patched to (1-based).
    /// Example: 1, 2, 3, ..., 32
    /// </summary>
    public int ConsoleChannel { get; init; }
    
    /// <summary>
    /// Channel label (user-assigned name).
    /// Source: ChannelInfo.Name
    /// Example: "Kick Drum", "Ch 1"
    /// </summary>
    public string Label { get; init; }
    
    /// <summary>
    /// Input type (Mic, Line, Digital, etc.).
    /// Source: /console/data/get/ch.{N}.preamp.type/val (if available)
    /// Examples: "Mic", "Line", "Digital", "N/A"
    /// "N/A" if not available
    /// </summary>
    public string InputType { get; init; } = "N/A";
    
    /// <summary>
    /// Phantom power status.
    /// Source: /console/data/get/ch.{N}.preamp.phantom/val (if available)
    /// Examples: "On", "Off", "N/A"
    /// "N/A" if not supported (e.g., line inputs, digital inputs)
    /// </summary>
    public string PhantomPower { get; init; } = "N/A";
    
    /// <summary>
    /// Preamp gain setting (in dB or raw value).
    /// Source: /console/data/get/ch.{N}.preamp.gain/val (if available)
    /// Examples: "42 dB", "+18 dB", "N/A"
    /// "N/A" if not available
    /// </summary>
    public string Gain { get; init; } = "N/A";
}
```

**Phase 1 API Call Strategy**:
- **Minimal**: Only fetch `PhysicalInput` if API supports it
- **Phantom/Gain**: Deferred to Phase 2 (not critical for basic input list)
- **Reason**: Reduce API calls, focus on core data (channel names, types, counts)

**Phase 1 Simplification**:
- `InputType`, `PhantomPower`, `Gain` default to `"N/A"`
- Only `PhysicalInput`, `ConsoleChannel`, `Label` populated

---

## Data Extraction Flow

### Phase 1 Minimal API Calls

```
1. GET /app/mixers/current
   → Extract: ConsoleSeries, Model, FirmwareVersion, IpAddress
   → Populate: MixerInfo

2. GET /console/information
   → Extract: totalChannels, channelTypes
   → Populate: ChannelInfo[] (count and types)

3. GET /console/data/get/ch.*.config.name/val (batch)
   → Extract: Channel names
   → Populate: ChannelInfo.Name (default "Ch {N}" if missing)

4. GET /console/data/get/ch.*.config.color/val (batch, optional)
   → Extract: Colors
   → Populate: ChannelInfo.Color (default "N/A" if missing)

5. [Phase 2] GET /console/data/get/ch.*.preamp.* (deferred)
   → Phantom, gain, input type
```

**Batch API Call Optimization**:
- Use wildcard paths where supported: `/console/data/get/ch.*.config.name/val`
- If not supported, loop over channels: `/console/data/get/ch.01.config.name/val`, `/console/data/get/ch.02.config.name/val`, etc.

---

## Normalization Rules (ADR-003)

### Missing Data Handling

| Field | Missing Behavior | Example |
|-------|-----------------|---------|
| Channel Name | Default to "Ch {N}" | "Ch 1", "Ch 32" |
| Color | "N/A" | "N/A" |
| Phantom Power | "N/A" | "N/A" |
| Gain | "N/A" | "N/A" |
| Physical Input | "N/A" | "N/A" |

**Why "N/A" instead of `null`?**
- Explicit in Excel output (cell shows "N/A", not blank)
- Easier to validate (non-null strings)
- Clear semantic: "not available" vs. "not fetched" vs. "error"

### Stereo Pair Detection

```csharp
// Pseudo-code for stereo detection
bool IsMonoInStereo(int channelNumber, Dictionary<string, DataValue> rawData)
{
    // Check if link flag is set
    var linkPath = $"ch.{channelNumber:D2}.config.link";
    if (rawData.TryGetValue(linkPath, out var linkValue))
    {
        return linkValue.Value is bool b && b;
    }
    
    // Fallback: Check if next channel name contains " (L)" or " (R)"
    var nextChannelName = rawData.GetValueOrDefault($"ch.{channelNumber + 1:D2}.config.name")?.Value as string;
    return nextChannelName?.Contains("(R)") == true;
}
```

### Color Mapping

```csharp
// Map API color values to Excel-compatible colors
string NormalizeColor(string apiColor)
{
    if (string.IsNullOrEmpty(apiColor))
        return "N/A";
    
    // If already hex, return as-is
    if (apiColor.StartsWith("#"))
        return apiColor;
    
    // Map named colors to hex
    return apiColor.ToLower() switch
    {
        "red"    => "#FF0000",
        "green"  => "#00FF00",
        "blue"   => "#0000FF",
        "yellow" => "#FFFF00",
        "orange" => "#FFA500",
        "purple" => "#800080",
        "pink"   => "#FFC0CB",
        "cyan"   => "#00FFFF",
        _        => "N/A"  // Unknown color
    };
}
```

---

## Validation

### ICanonicalModelValidator

```csharp
public interface ICanonicalModelValidator
{
    ValidationResult Validate(CanonicalModel model);
}

public class ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
}
```

### Validation Rules

```csharp
public class CanonicalModelValidator : ICanonicalModelValidator
{
    public ValidationResult Validate(CanonicalModel model)
    {
        var errors = new List<string>();
        
        // Mixer validation
        if (string.IsNullOrWhiteSpace(model.Mixer.ConsoleSeries))
            errors.Add("Mixer.ConsoleSeries is required");
        
        if (string.IsNullOrWhiteSpace(model.Mixer.Model))
            errors.Add("Mixer.Model is required");
        
        // Channel validation
        if (model.Channels.Count == 0)
            errors.Add("At least one channel is required");
        
        var channelNumbers = model.Channels.Select(c => c.ChannelNumber).ToList();
        if (channelNumbers.Distinct().Count() != channelNumbers.Count)
            errors.Add("Duplicate channel numbers found");
        
        // Patching validation (no duplicates except "N/A")
        var physicalInputs = model.Patches
            .Select(p => p.PhysicalInput)
            .Where(p => p != "N/A")
            .ToList();
        
        if (physicalInputs.Distinct().Count() != physicalInputs.Count)
            errors.Add("Duplicate physical inputs found (excluding N/A)");
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
```

---

## Example Data

### Sample CanonicalModel (X32 Compact - 16 Channels)

```json
{
  "mixer": {
    "consoleSeries": "Behringer X32",
    "model": "X32 Compact",
    "firmwareVersion": "4.06",
    "ipAddress": "192.168.1.100",
    "capturedAt": "2026-01-30T14:30:00Z"
  },
  "channels": [
    {
      "channelNumber": 1,
      "name": "Kick Drum",
      "type": "Input",
      "color": "#FF0000",
      "isMonoInStereo": false,
      "routing": {
        "sendToLR": true,
        "auxSends": [],
        "groups": []
      }
    },
    {
      "channelNumber": 2,
      "name": "Snare",
      "type": "Input",
      "color": "#FF0000",
      "isMonoInStereo": false,
      "routing": {
        "sendToLR": true,
        "auxSends": [],
        "groups": []
      }
    },
    {
      "channelNumber": 3,
      "name": "Bass L",
      "type": "Input",
      "color": "#0000FF",
      "isMonoInStereo": true,
      "routing": {
        "sendToLR": true,
        "auxSends": [],
        "groups": []
      }
    },
    {
      "channelNumber": 4,
      "name": "Bass R",
      "type": "Input",
      "color": "#0000FF",
      "isMonoInStereo": true,
      "routing": {
        "sendToLR": true,
        "auxSends": [],
        "groups": []
      }
    }
  ],
  "patches": [
    {
      "physicalInput": "Local-01",
      "consoleChannel": 1,
      "label": "Kick Drum",
      "inputType": "Mic",
      "phantomPower": "Off",
      "gain": "36 dB"
    },
    {
      "physicalInput": "Local-02",
      "consoleChannel": 2,
      "label": "Snare",
      "inputType": "Mic",
      "phantomPower": "Off",
      "gain": "42 dB"
    },
    {
      "physicalInput": "S16-A-01",
      "consoleChannel": 3,
      "label": "Bass L",
      "inputType": "Line",
      "phantomPower": "N/A",
      "gain": "N/A"
    },
    {
      "physicalInput": "S16-A-02",
      "consoleChannel": 4,
      "label": "Bass R",
      "inputType": "Line",
      "phantomPower": "N/A",
      "gain": "N/A"
    }
  ]
}
```

---

## Phase 2 Extensions (Future)

### CanonicalModel v1 Additions

```csharp
// Phase 2: Add more routing details
public class RoutingInfo
{
    public bool SendToLR { get; init; }
    
    // NEW in v1
    public Dictionary<int, float> AuxSendLevels { get; init; } = new();  // Aux # → Level (0.0 - 1.0)
    public List<string> DcaAssignments { get; init; } = new();
    public List<int> MuteGroups { get; init; } = new();
}

// Phase 2: Add dynamics/EQ info
public class ProcessingInfo
{
    public bool CompressorEnabled { get; init; }
    public bool GateEnabled { get; init; }
    public bool EqEnabled { get; init; }
    public List<EqBand> EqBands { get; init; } = new();
}
```

---

## Traceability

| Model Component | Implements Requirement | Verified By |
|----------------|----------------------|-------------|
| `MixerInfo` | REQ-F-002 (App State) | TEST-NORMALIZE-001 |
| `ChannelInfo` | REQ-F-003 (Console Data) | TEST-NORMALIZE-002 |
| `PatchInfo` | REQ-F-004 (Normalization) | TEST-NORMALIZE-003 |
| Validation Rules | REQ-F-004 (Data Integrity) | TEST-VALIDATE-001 |

---

## Standards Compliance

- ✅ **ISO/IEC/IEEE 42010:2011**: Data architecture with clear semantics
- ✅ **ADR-003**: CanonicalModel v0 design decision
- ✅ **IEEE 1016-2009**: Data model documentation

---

**Next Phase**: Phase 04 - Detailed Design (ER diagrams, transformation logic)
