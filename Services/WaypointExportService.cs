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
    public static (double X, double Y) ApplyExportPlaneRotation(
        double exportX,
        double exportY,
        ExportLocalRotation rotation) =>
        rotation switch
        {
            ExportLocalRotation.Degree90 => (-exportY, exportX),
            ExportLocalRotation.Degree180 => (-exportX, -exportY),
            _ => (exportX, exportY)
        };

    public static string DescribeLocalRotation(ExportLocalRotation rotation) =>
        rotation switch
        {
            ExportLocalRotation.Degree90 => "Export rot 90°",
            ExportLocalRotation.Degree180 => "Export rot 180°",
            _ => "Export rot none"
        };

    public static WaypointExportValues ComputeExportValues(
        double localX,
        double localY,
        FrameDefinition frame,
        RobotCalibrationFile calibration,
        ProcessDefaults defaults)
    {
        PixelToRobotCoeffs coeffs = calibration.PixelToRobot
            ?? throw new InvalidOperationException("Calibration has no pixelToRobot coefficients.");

        double angleDeg = frame.AngleDeg;
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

        if (frame.InvertY)
        {
            (exportX, exportY) = ToFanucFrameMillimeters(localX, localY, frame, coeffs);
        }

        (exportX, exportY) = ApplyExportPlaneRotation(exportX, exportY, defaults.LocalRotation);

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

    /// <summary>
    /// Millimeters along the frame axes. +Y is the Fanuc side of +X, not image-down.
    /// </summary>
    private static (double X, double Y) ToFanucFrameMillimeters(
        double localX,
        double localY,
        FrameDefinition frame,
        PixelToRobotCoeffs coeffs)
    {
        (double originX, double originY) = CoordinateTransform.LocalToSensor(0, 0, frame);
        (double xAxisX, double xAxisY) = CoordinateTransform.LocalToSensor(1, 0, frame);
        (double yAxisX, double yAxisY) = CoordinateTransform.LocalToSensor(0, 1, frame);
        (double axisXx, double axisXy) = LinearPixelDelta(coeffs, xAxisX - originX, xAxisY - originY);
        (double axisYx, double axisYy) = LinearPixelDelta(coeffs, yAxisX - originX, yAxisY - originY);
        return (localX * Hypot(axisXx, axisXy), localY * Hypot(axisYx, axisYy));
    }

    private static (double X, double Y) LinearPixelDelta(
        PixelToRobotCoeffs coeffs,
        double pixelX,
        double pixelY) =>
        (coeffs.A * pixelX + coeffs.B * pixelY, coeffs.C * pixelX + coeffs.D * pixelY);

    private static double Hypot(double x, double y) => Math.Sqrt(x * x + y * y);

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
