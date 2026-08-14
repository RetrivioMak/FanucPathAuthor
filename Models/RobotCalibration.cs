using System.Text.Json.Serialization;

namespace FanucPathAuthor.Models;

public enum CalibrationFileKind
{
    RobotCvTransform,
    CompactMarker
}

public sealed class RobotCalibrationFile
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("frameWidth")]
    public int FrameWidth { get; set; }

    [JsonPropertyName("frameHeight")]
    public int FrameHeight { get; set; }

    [JsonPropertyName("robotFrame")]
    public string RobotFrame { get; set; } = string.Empty;

    [JsonPropertyName("robotTool")]
    public string RobotTool { get; set; } = string.Empty;

    [JsonPropertyName("referencePixelX")]
    public double ReferencePixelX { get; set; }

    [JsonPropertyName("referencePixelY")]
    public double ReferencePixelY { get; set; }

    [JsonPropertyName("referenceRobotX_mm")]
    public double ReferenceRobotX_mm { get; set; }

    [JsonPropertyName("referenceRobotY_mm")]
    public double ReferenceRobotY_mm { get; set; }

    [JsonPropertyName("referenceRobotR_deg")]
    public double? ReferenceRobotR_deg { get; set; }

    [JsonPropertyName("pixelToRobot")]
    public PixelToRobotCoeffs? PixelToRobot { get; set; }

    [JsonPropertyName("calibrationPoseId")]
    public string? CalibrationPoseId { get; set; }

    [JsonPropertyName("fixedRobotZ_mm")]
    public double? FixedRobotZ_mm { get; set; }

    [JsonPropertyName("fixedRobotW_deg")]
    public double? FixedRobotW_deg { get; set; }

    [JsonPropertyName("fixedRobotP_deg")]
    public double? FixedRobotP_deg { get; set; }

    [JsonPropertyName("fixedRobotR_deg")]
    public double? FixedRobotR_deg { get; set; }

    [JsonIgnore]
    public CalibrationFileKind Kind { get; set; } = CalibrationFileKind.RobotCvTransform;

    [JsonIgnore]
    public bool IsCompactMarker => Kind == CalibrationFileKind.CompactMarker;

    [JsonIgnore]
    public bool HasPixelToRobot => PixelToRobot != null;
}

public sealed class PixelToRobotCoeffs
{
    [JsonPropertyName("a")]
    public double A { get; set; }

    [JsonPropertyName("b")]
    public double B { get; set; }

    [JsonPropertyName("tx")]
    public double Tx { get; set; }

    [JsonPropertyName("c")]
    public double C { get; set; }

    [JsonPropertyName("d")]
    public double D { get; set; }

    [JsonPropertyName("ty")]
    public double Ty { get; set; }

    public (double X, double Y) Transform(double pixelX, double pixelY) =>
        (A * pixelX + B * pixelY + Tx, C * pixelX + D * pixelY + Ty);
}

public sealed class RobotWaypoint
{
    public int Index { get; init; }
    public string SegmentLabel { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public double LocalX_px { get; init; }
    public double LocalY_px { get; init; }
    public double SensorX_px { get; init; }
    public double SensorY_px { get; init; }
    public double ExportX_mm { get; init; }
    public double ExportY_mm { get; init; }
    public double ExportR_deg { get; init; }
    public double RobotX_mm { get; init; }
    public double RobotY_mm { get; init; }
    public double Z_mm { get; init; }
    public double W_deg { get; init; }
    public double P_deg { get; init; }
    public double R_deg { get; init; }
    public double Speed_mm_s { get; init; }
    public bool GlueOn { get; init; }
    public bool IsPreviewOnly { get; init; }
}
