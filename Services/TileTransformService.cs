using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class TileTransformService
{
    public static (double WorldX_mm, double WorldY_mm) TilePixelToWorld(
        double tileSensorX,
        double tileSensorY,
        FovTile tile,
        RobotCalibrationFile calibration)
    {
        PixelToRobotCoeffs coeffs = calibration.PixelToRobot
            ?? throw new InvalidOperationException("Calibration does not contain pixelToRobot coefficients.");

        (double robotX, double robotY) = coeffs.Transform(tileSensorX, tileSensorY);
        (double deltaX, double deltaY) = GetPoseDeltaMm(tile, calibration);
        return ApplyInversePoseTranslation(robotX, robotY, deltaX, deltaY);
    }

    public static (double TileSensorX, double TileSensorY) WorldToTilePixel(
        double worldX_mm,
        double worldY_mm,
        FovTile tile,
        RobotCalibrationFile calibration)
    {
        PixelToRobotCoeffs coeffs = calibration.PixelToRobot
            ?? throw new InvalidOperationException("Calibration does not contain pixelToRobot coefficients.");

        (double deltaX, double deltaY) = GetPoseDeltaMm(tile, calibration);
        (double robotX, double robotY) = ApplyPoseTranslation(worldX_mm, worldY_mm, deltaX, deltaY);
        if (!TryRobotToPixel(robotX, robotY, coeffs, out double pixelX, out double pixelY))
        {
            throw new InvalidOperationException("Calibration matrix is not invertible.");
        }

        return (pixelX, pixelY);
    }

    /// <summary>
    /// Maps a pixel on a tile image to anchor canvas sensor pixels.
    /// </summary>
    public static (double CanvasX, double CanvasY) TilePixelToAnchorCanvas(
        double tileSensorX,
        double tileSensorY,
        FovTile tile,
        FovTile anchorTile,
        RobotCalibrationFile calibration)
    {
        (double worldX, double worldY) = TilePixelToWorld(tileSensorX, tileSensorY, tile, calibration);
        return WorldToAnchorCanvas(worldX, worldY, anchorTile, calibration);
    }

    public static (double CanvasX, double CanvasY) WorldToAnchorCanvas(
        double worldX_mm,
        double worldY_mm,
        FovTile anchorTile,
        RobotCalibrationFile calibration)
    {
        return WorldToTilePixel(worldX_mm, worldY_mm, anchorTile, calibration);
    }

    public static (double OffsetX, double OffsetY) GetTileCanvasOffset(
        FovTile tile,
        FovTile anchorTile,
        RobotCalibrationFile calibration) =>
        TilePixelToAnchorCanvas(0, 0, tile, anchorTile, calibration);

    public static bool TryRobotToPixel(
        double robotX_mm,
        double robotY_mm,
        PixelToRobotCoeffs coeffs,
        out double pixelX,
        out double pixelY)
    {
        pixelX = 0;
        pixelY = 0;

        double det = coeffs.A * coeffs.D - coeffs.B * coeffs.C;
        if (Math.Abs(det) < 1e-12)
        {
            return false;
        }

        double dx = robotX_mm - coeffs.Tx;
        double dy = robotY_mm - coeffs.Ty;
        pixelX = (coeffs.D * dx - coeffs.B * dy) / det;
        pixelY = (-coeffs.C * dx + coeffs.A * dy) / det;
        return true;
    }

    private static (double DeltaX_mm, double DeltaY_mm) GetPoseDeltaMm(
        FovTile tile,
        RobotCalibrationFile calibration) =>
        (
            tile.RobotX_mm - calibration.ReferenceRobotX_mm,
            tile.RobotY_mm - calibration.ReferenceRobotY_mm);

    private static (double X, double Y) ApplyInversePoseTranslation(
        double robotX,
        double robotY,
        double deltaX,
        double deltaY) =>
        (robotX - deltaX, robotY - deltaY);

    private static (double X, double Y) ApplyPoseTranslation(
        double worldX,
        double worldY,
        double deltaX,
        double deltaY) =>
        (worldX + deltaX, worldY + deltaY);
}
