using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public readonly record struct WaypointExportValues(
    double ExportX_mm,
    double ExportY_mm,
    double ExportR_deg,
    double SensorX_px,
    double SensorY_px);

public static class WaypointExportService
{
    public static double GetEffectiveExportAngleDeg(FrameDefinition frame, ProcessDefaults defaults) =>
        frame.AngleDeg + (defaults.UseFixedCameraInversion ? 180.0 : 0.0);

    public static WaypointExportValues ComputeExportValues(
        double localX,
        double localY,
        FrameDefinition frame,
        RobotCalibrationFile calibration,
        ProcessDefaults defaults)
    {
        PixelToRobotCoeffs coeffs = calibration.PixelToRobot
            ?? throw new InvalidOperationException("Calibration has no pixelToRobot coefficients.");

        double angleDeg = GetEffectiveExportAngleDeg(frame, defaults);
        (double sensorX, double sensorY) = CoordinateTransform.LocalToSensor(localX, localY, frame, angleDeg);
        (double baseX, double baseY) = ComputeBaseRobotMm(sensorX, sensorY, calibration, defaults, coeffs);
        (double exportX, double exportY) = ApplyExportMode(
            baseX,
            baseY,
            localX,
            localY,
            frame,
            calibration,
            defaults,
            coeffs,
            angleDeg);

        double exportR = ResolveExportR_deg(calibration);
        if (defaults.UseNeedleOffset)
        {
            exportR += defaults.NeedleOffsetR_deg;
        }

        return new WaypointExportValues(exportX, exportY, exportR, sensorX, sensorY);
    }

    private static (double X, double Y) ApplyExportMode(
        double baseX,
        double baseY,
        double localX,
        double localY,
        FrameDefinition frame,
        RobotCalibrationFile calibration,
        ProcessDefaults defaults,
        PixelToRobotCoeffs coeffs,
        double angleDeg)
    {
        return defaults.CoordinateMode switch
        {
            ExportCoordinateMode.UframeAbsolute => (baseX, baseY),
            ExportCoordinateMode.OffsetFromCalibrationRef => (
                baseX - calibration.ReferenceRobotX_mm,
                baseY - calibration.ReferenceRobotY_mm),
            ExportCoordinateMode.OffsetFromFrameOrigin => ComputeFrameOriginOffset(
                baseX,
                baseY,
                localX,
                localY,
                frame,
                calibration,
                defaults,
                coeffs,
                angleDeg),
            _ => throw new InvalidOperationException($"Unknown export coordinate mode: {defaults.CoordinateMode}")
        };
    }

    private static (double X, double Y) ComputeFrameOriginOffset(
        double baseX,
        double baseY,
        double localX,
        double localY,
        FrameDefinition frame,
        RobotCalibrationFile calibration,
        ProcessDefaults defaults,
        PixelToRobotCoeffs coeffs,
        double angleDeg)
    {
        if (Math.Abs(localX) < 1e-9 && Math.Abs(localY) < 1e-9)
        {
            return (0, 0);
        }

        (double originSensorX, double originSensorY) = CoordinateTransform.LocalToSensor(0, 0, frame, angleDeg);
        (double frameBaseX, double frameBaseY) = ComputeBaseRobotMm(
            originSensorX,
            originSensorY,
            calibration,
            defaults,
            coeffs);

        return (baseX - frameBaseX, baseY - frameBaseY);
    }

    private static (double X, double Y) ComputeBaseRobotMm(
        double sensorX,
        double sensorY,
        RobotCalibrationFile calibration,
        ProcessDefaults defaults,
        PixelToRobotCoeffs coeffs)
    {
        (double transformedX, double transformedY) = coeffs.Transform(sensorX, sensorY);
        (double referencePixelRobotX, double referencePixelRobotY) = coeffs.Transform(
            calibration.ReferencePixelX,
            calibration.ReferencePixelY);

        double robotX = calibration.ReferenceRobotX_mm + transformedX - referencePixelRobotX;
        double robotY = calibration.ReferenceRobotY_mm + transformedY - referencePixelRobotY;

        if (defaults.UseFovCenterNeedleOffset &&
            calibration.FrameWidth > 0 &&
            calibration.FrameHeight > 0)
        {
            double fovCenterX = calibration.FrameWidth / 2.0;
            double fovCenterY = calibration.FrameHeight / 2.0;
            (double fovCenterRobotX, double fovCenterRobotY) = coeffs.Transform(fovCenterX, fovCenterY);
            robotX -= fovCenterRobotX - referencePixelRobotX;
            robotY -= fovCenterRobotY - referencePixelRobotY;
        }

        if (defaults.UseNeedleOffset)
        {
            robotX += defaults.NeedleOffsetX_mm;
            robotY += defaults.NeedleOffsetY_mm;
        }

        return (robotX, robotY);
    }

    public static double ResolveExportR_deg(RobotCalibrationFile? calibration)
    {
        if (calibration == null)
        {
            return double.NaN;
        }

        if (calibration.IsCompactMarker)
        {
            return calibration.FixedRobotR_deg
                ?? calibration.ReferenceRobotR_deg
                ?? 0;
        }

        return calibration.ReferenceRobotR_deg ?? 0;
    }

    public static string DescribeExportMode(ExportCoordinateMode mode) =>
        mode switch
        {
            ExportCoordinateMode.UframeAbsolute => "UFRAME absolute",
            ExportCoordinateMode.OffsetFromFrameOrigin => "Offset from Frame origin",
            ExportCoordinateMode.OffsetFromCalibrationRef => "Offset from Calib Ref",
            _ => mode.ToString()
        };
}
