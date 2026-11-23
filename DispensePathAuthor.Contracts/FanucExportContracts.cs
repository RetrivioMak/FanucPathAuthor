namespace DispensePathAuthor.Contracts;

public enum FanucCoordinateMode
{
    UframeAbsolute,
    OffsetFromFrameOrigin,
    OffsetFromCalibrationRef
}

public enum FanucLocalRotation
{
    None,
    Degree90,
    Degree180
}

public sealed class FanucExportSettings
{
    public FanucCoordinateMode CoordinateMode { get; init; } = FanucCoordinateMode.OffsetFromCalibrationRef;

    public bool UseFovCenterNeedleOffset { get; init; }

    public FanucLocalRotation LocalRotation { get; init; } = FanucLocalRotation.Degree180;

    public bool UseNeedleOffset { get; init; }

    public double NeedleOffsetX_mm { get; init; }

    public double NeedleOffsetY_mm { get; init; }

    public double NeedleOffsetR_deg { get; init; }

    public int? UserFrame { get; init; }

    public int? UserTool { get; init; }
}

public sealed class FanucCalibrationSnapshot
{
    public bool HasTransform { get; init; }

    public bool IsCompactMarker { get; init; }

    public string? PoseId { get; init; }

    public int FrameWidth { get; init; }

    public int FrameHeight { get; init; }

    public double ReferencePixelX { get; init; }

    public double ReferencePixelY { get; init; }

    public double ReferenceRobotX_mm { get; init; }

    public double ReferenceRobotY_mm { get; init; }

    public double? FixedRobotZ_mm { get; init; }

    public double? FixedRobotW_deg { get; init; }

    public double? FixedRobotP_deg { get; init; }

    public double? FixedRobotR_deg { get; init; }

    public int DefaultUserFrame { get; init; } = 1;

    public int DefaultUserTool { get; init; } = 1;
}

public sealed class FanucExportPoint
{
    public int Index { get; init; }

    public string Role { get; init; } = string.Empty;

    public double LocalX_px { get; init; }

    public double LocalY_px { get; init; }

    public double SensorX_px { get; init; }

    public double SensorY_px { get; init; }

    public double ExportX_mm { get; init; }

    public double ExportY_mm { get; init; }

    public double ExportR_deg { get; init; }

    public double Z_mm { get; init; }

    public double Width_mm { get; init; }

    public bool IsPreviewOnly { get; init; }
}

public interface IFanucExportSession
{
    bool IsTeachWorkspace { get; }

    FanucExportSettings Settings { get; }

    FanucCalibrationSnapshot Calibration { get; }

    void ApplySettings(FanucExportSettings settings);

    IReadOnlyList<FanucExportPoint> CreatePoints();
}

public interface IInteractiveProgramExporter : IProgramExporter
{
    string? TryRun(IWin32Window owner, IFanucExportSession session);
}
