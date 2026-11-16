namespace DispensePathAuthor.Models;

/// <summary>
/// User export preferences persisted across application sessions.
/// </summary>
public sealed class ExportSettingsSnapshot
{
    public int SchemaVersion { get; set; } = 2;
    public ExportCoordinateMode CoordinateMode { get; set; } = ExportCoordinateMode.OffsetFromCalibrationRef;
    public bool UseFovCenterNeedleOffset { get; set; }
    public ExportLocalRotation LocalRotation { get; set; } = ExportLocalRotation.Degree180;
    public bool UseNeedleOffset { get; set; }
    public double NeedleOffsetX_mm { get; set; }
    public double NeedleOffsetY_mm { get; set; }
    public double NeedleOffsetR_deg { get; set; }
    public int? UserFrame { get; set; }
    public int? UserTool { get; set; }

    public static ExportSettingsSnapshot FromFanuc(FanucLsSettings fanuc) =>
        new()
        {
            CoordinateMode = fanuc.CoordinateMode,
            UseFovCenterNeedleOffset = fanuc.UseFovCenterNeedleOffset,
            LocalRotation = fanuc.LocalRotation,
            UseNeedleOffset = fanuc.UseNeedleOffset,
            NeedleOffsetX_mm = fanuc.NeedleOffsetX_mm,
            NeedleOffsetY_mm = fanuc.NeedleOffsetY_mm,
            NeedleOffsetR_deg = fanuc.NeedleOffsetR_deg,
            UserFrame = fanuc.UserFrame,
            UserTool = fanuc.UserTool
        };

    public void ApplyTo(FanucLsSettings fanuc)
    {
        fanuc.CoordinateMode = CoordinateMode;
        fanuc.UseFovCenterNeedleOffset = UseFovCenterNeedleOffset;
        fanuc.LocalRotation = LocalRotation;
        fanuc.UseNeedleOffset = UseNeedleOffset;
        fanuc.NeedleOffsetX_mm = NeedleOffsetX_mm;
        fanuc.NeedleOffsetY_mm = NeedleOffsetY_mm;
        fanuc.NeedleOffsetR_deg = NeedleOffsetR_deg;
        fanuc.UserFrame = UserFrame;
        fanuc.UserTool = UserTool;
    }
}
