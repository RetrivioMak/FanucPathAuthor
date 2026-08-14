using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public readonly record struct CaptureRobotPose(double X_mm, double Y_mm, double R_deg);

public static class CapturePoseHelper
{
    public static CaptureRobotPose? TryGetAnchorCapturePose(PathDocument document)
    {
        FovTile? anchor = TileDocumentHelper.GetAnchor(document);
        if (anchor == null)
        {
            return null;
        }

        return new CaptureRobotPose(anchor.RobotX_mm, anchor.RobotY_mm, anchor.RobotR_deg);
    }

    public static void ApplyCalibrationReference(FovTile tile, RobotCalibrationFile calibration)
    {
        tile.RobotX_mm = calibration.ReferenceRobotX_mm;
        tile.RobotY_mm = calibration.ReferenceRobotY_mm;
        tile.RobotR_deg = calibration.ReferenceRobotR_deg
            ?? calibration.FixedRobotR_deg
            ?? 0;
    }

    public static bool IsUnsetCapturePose(FovTile tile) =>
        Math.Abs(tile.RobotX_mm) < 1e-9 &&
        Math.Abs(tile.RobotY_mm) < 1e-9 &&
        Math.Abs(tile.RobotR_deg) < 1e-9;
}
