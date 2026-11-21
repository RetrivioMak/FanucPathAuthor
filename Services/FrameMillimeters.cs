using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public readonly record struct FrameAxisScale(double X_mmPerPx, double Y_mmPerPx)
{
    public bool IsDefined => X_mmPerPx > 1e-12 && Y_mmPerPx > 1e-12;
}

/// <summary>
/// CV path coordinates at schema 6 are millimeters along the project frame.
/// Sensor pixels and calibration stay source data and supply the scale.
/// </summary>
public static class FrameMillimeters
{
    public const int SchemaVersion = 6;

    public static bool StoresMillimeters(PathDocument document) =>
        document.Workspace == WorkspaceKind.Cv && document.SchemaVersion >= SchemaVersion;

    public static bool TryGetScale(
        FrameDefinition frame,
        RobotCalibrationFile? calibration,
        out FrameAxisScale scale)
    {
        scale = default;
        PixelToRobotCoeffs? coeffs = calibration?.PixelToRobot;
        if (coeffs == null)
        {
            return false;
        }

        (double originX, double originY) = CoordinateTransform.LocalToSensor(0, 0, frame);
        (double xAxisX, double xAxisY) = CoordinateTransform.LocalToSensor(1, 0, frame);
        (double yAxisX, double yAxisY) = CoordinateTransform.LocalToSensor(0, 1, frame);
        (double axisXx, double axisXy) = LinearPixelDelta(coeffs, xAxisX - originX, xAxisY - originY);
        (double axisYx, double axisYy) = LinearPixelDelta(coeffs, yAxisX - originX, yAxisY - originY);
        scale = new FrameAxisScale(Hypot(axisXx, axisXy), Hypot(axisYx, axisYy));
        return scale.IsDefined;
    }

    /// <summary>
    /// Calibration scale when the file is loaded. Otherwise the CV reference scale, isotropic.
    /// </summary>
    public static FrameAxisScale ResolveImageScale(
        FrameDefinition frame,
        RobotCalibrationFile? calibration,
        double referenceMmPerPx)
    {
        if (TryGetScale(frame, calibration, out FrameAxisScale scale))
        {
            return scale;
        }

        double reference = referenceMmPerPx > 1e-12
            ? referenceMmPerPx
            : ProcessDefaults.DefaultReferenceMmPerPx;
        return new FrameAxisScale(reference, reference);
    }

    public static double MeanMmPerPx(FrameAxisScale scale) =>
        (scale.X_mmPerPx + scale.Y_mmPerPx) / 2.0;

    public static (double X, double Y) ToMillimeters(double localX_px, double localY_px, FrameAxisScale scale) =>
        (localX_px * scale.X_mmPerPx, localY_px * scale.Y_mmPerPx);

    public static (double X, double Y) ToLocalPixels(double x_mm, double y_mm, FrameAxisScale scale) =>
        (x_mm / scale.X_mmPerPx, y_mm / scale.Y_mmPerPx);

    public static void ConvertPoints(PathSegment segment, Func<double, double, (double X, double Y)> map)
    {
        (segment.X1, segment.Y1) = map(segment.X1, segment.Y1);
        if (segment.Kind is SegmentKind.Line or SegmentKind.Arc)
        {
            (segment.X2, segment.Y2) = map(segment.X2, segment.Y2);
        }

        if (segment.Kind == SegmentKind.Arc)
        {
            (segment.X3, segment.Y3) = map(segment.X3, segment.Y3);
        }
    }

    /// <summary>
    /// Old CV segments are frame-local pixels. With a calibration they become frame millimeters.
    /// Teach coordinates are already millimeters and only the schema number advances.
    /// Returns false when a CV path still has to stay in pixels because the scale is unknown.
    /// </summary>
    public static bool Migrate(PathDocument document, RobotCalibrationFile? calibration)
    {
        if (document.SchemaVersion >= SchemaVersion)
        {
            return false;
        }

        if (document.Workspace == WorkspaceKind.Teach)
        {
            document.SchemaVersion = SchemaVersion;
            return true;
        }

        if (document.Workspace != WorkspaceKind.Cv)
        {
            return false;
        }

        if (document.Segments.Count == 0)
        {
            document.SchemaVersion = SchemaVersion;
            return true;
        }

        if (!TryGetScale(document.Frame, calibration, out FrameAxisScale scale))
        {
            return false;
        }

        foreach (PathSegment segment in document.Segments)
        {
            ConvertPoints(segment, (x, y) => ToMillimeters(x, y, scale));
        }

        document.SchemaVersion = SchemaVersion;
        return true;
    }

    private static (double X, double Y) LinearPixelDelta(PixelToRobotCoeffs coeffs, double pixelX, double pixelY) =>
        (coeffs.A * pixelX + coeffs.B * pixelY, coeffs.C * pixelX + coeffs.D * pixelY);

    private static double Hypot(double x, double y) => Math.Sqrt(x * x + y * y);
}
