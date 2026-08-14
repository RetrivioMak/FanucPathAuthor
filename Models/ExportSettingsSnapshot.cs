namespace FanucPathAuthor.Models;

/// <summary>
/// User export preferences persisted across application sessions.
/// </summary>
public sealed class ExportSettingsSnapshot
{
    public int SchemaVersion { get; set; } = 1;
    public ExportCoordinateMode CoordinateMode { get; set; } = ExportCoordinateMode.OffsetFromCalibrationRef;
    public bool UseFovCenterNeedleOffset { get; set; }
    public bool UseFixedCameraInversion { get; set; } = true;
    public bool UseNeedleOffset { get; set; }
    public double NeedleOffsetX_mm { get; set; }
    public double NeedleOffsetY_mm { get; set; }
    public double NeedleOffsetR_deg { get; set; }

    public static ExportSettingsSnapshot FromProcessDefaults(ProcessDefaults defaults) =>
        new()
        {
            CoordinateMode = defaults.CoordinateMode,
            UseFovCenterNeedleOffset = defaults.UseFovCenterNeedleOffset,
            UseFixedCameraInversion = defaults.UseFixedCameraInversion,
            UseNeedleOffset = defaults.UseNeedleOffset,
            NeedleOffsetX_mm = defaults.NeedleOffsetX_mm,
            NeedleOffsetY_mm = defaults.NeedleOffsetY_mm,
            NeedleOffsetR_deg = defaults.NeedleOffsetR_deg
        };

    public void ApplyTo(ProcessDefaults defaults)
    {
        defaults.CoordinateMode = CoordinateMode;
        defaults.UseFovCenterNeedleOffset = UseFovCenterNeedleOffset;
        defaults.UseFixedCameraInversion = UseFixedCameraInversion;
        defaults.UseNeedleOffset = UseNeedleOffset;
        defaults.NeedleOffsetX_mm = NeedleOffsetX_mm;
        defaults.NeedleOffsetY_mm = NeedleOffsetY_mm;
        defaults.NeedleOffsetR_deg = NeedleOffsetR_deg;
    }
}
