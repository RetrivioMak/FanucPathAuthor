using System.Text.Json.Serialization;

namespace DispensePathAuthor.Models;

public sealed class FanucLsSettings
{
    public ExportCoordinateMode CoordinateMode { get; set; } = ExportCoordinateMode.OffsetFromCalibrationRef;

    public bool UseFovCenterNeedleOffset { get; set; }

    public ExportLocalRotation LocalRotation { get; set; } = ExportLocalRotation.Degree180;

    public bool UseNeedleOffset { get; set; }

    public double NeedleOffsetX_mm { get; set; }

    public double NeedleOffsetY_mm { get; set; }

    public double NeedleOffsetR_deg { get; set; }

    public int? UserFrame { get; set; }

    public int? UserTool { get; set; }

    public FanucLsSettings Clone() =>
        new()
        {
            CoordinateMode = CoordinateMode,
            UseFovCenterNeedleOffset = UseFovCenterNeedleOffset,
            LocalRotation = LocalRotation,
            UseNeedleOffset = UseNeedleOffset,
            NeedleOffsetX_mm = NeedleOffsetX_mm,
            NeedleOffsetY_mm = NeedleOffsetY_mm,
            NeedleOffsetR_deg = NeedleOffsetR_deg,
            UserFrame = UserFrame,
            UserTool = UserTool
        };
}

public sealed class ExporterSettings
{
    [JsonPropertyName("fanuc-ls")]
    public FanucLsSettings FanucLs { get; set; } = new();

    public ExporterSettings Clone() =>
        new()
        {
            FanucLs = FanucLs.Clone()
        };
}
