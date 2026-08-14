using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class CoordinateTransform
{
    public static (double X, double Y) LocalToSensor(
        double localX,
        double localY,
        FrameDefinition frame,
        double? angleDegOverride = null)
    {
        double angleDeg = angleDegOverride ?? frame.AngleDeg;
        double rad = DegreesToRadians(angleDeg);
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        return (
            frame.OriginX_px + localX * cos - localY * sin,
            frame.OriginY_px + localX * sin + localY * cos);
    }

    public static (double X, double Y) SensorToLocal(
        double sensorX,
        double sensorY,
        FrameDefinition frame)
    {
        double dx = sensorX - frame.OriginX_px;
        double dy = sensorY - frame.OriginY_px;
        double rad = DegreesToRadians(-frame.AngleDeg);
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        return (dx * cos - dy * sin, dx * sin + dy * cos);
    }

    public static (double X, double Y) LocalToRobot(
        double localX,
        double localY,
        FrameDefinition frame,
        PixelToRobotCoeffs coeffs)
    {
        var (sensorX, sensorY) = LocalToSensor(localX, localY, frame);
        return coeffs.Transform(sensorX, sensorY);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}

public static class GeometryHelper
{
    public static IEnumerable<(double X, double Y)> ExpandSegment(PathSegment segment)
    {
        return segment.Kind switch
        {
            SegmentKind.Dot => new[] { (segment.X1, segment.Y1) },
            SegmentKind.Line => new[] { (segment.X1, segment.Y1), (segment.X2, segment.Y2) },
            SegmentKind.Arc => new[]
            {
                (segment.X1, segment.Y1),
                (segment.X2, segment.Y2),
                (segment.X3, segment.Y3)
            },
            _ => Array.Empty<(double, double)>()
        };
    }

    public static IReadOnlyList<RobotWaypoint> BuildWaypoints(
        PathDocument document,
        RobotCalibrationFile? calibration)
    {
        var waypoints = new List<RobotWaypoint>();
        int index = 1;

        foreach (PathSegment segment in document.Segments)
        {
            string[] roles = segment.Kind switch
            {
                SegmentKind.Dot => new[] { "dot" },
                SegmentKind.Line => new[] { "start", "end" },
                SegmentKind.Arc => new[] { "start", "mid", "end" },
                _ => Array.Empty<string>()
            };

            int roleIndex = 0;
            foreach ((double localX, double localY) in ExpandSegment(segment))
            {
                double exportX = double.NaN;
                double exportY = double.NaN;
                double exportR = double.NaN;
                double sensorX = double.NaN;
                double sensorY = double.NaN;
                if (calibration?.PixelToRobot != null)
                {
                    WaypointExportValues values = WaypointExportService.ComputeExportValues(
                        localX,
                        localY,
                        document.Frame,
                        calibration,
                        document.Defaults);
                    exportX = values.ExportX_mm;
                    exportY = values.ExportY_mm;
                    exportR = values.ExportR_deg;
                    sensorX = values.SensorX_px;
                    sensorY = values.SensorY_px;
                }

                waypoints.Add(new RobotWaypoint
                {
                    Index = index++,
                    SegmentLabel = segment.DisplayName,
                    Role = roles[roleIndex++],
                    LocalX_px = localX,
                    LocalY_px = localY,
                    SensorX_px = sensorX,
                    SensorY_px = sensorY,
                    ExportX_mm = exportX,
                    ExportY_mm = exportY,
                    ExportR_deg = exportR,
                    RobotX_mm = exportX,
                    RobotY_mm = exportY,
                    R_deg = exportR,
                    GlueOn = segment.GlueOn
                });
            }
        }

        return waypoints;
    }

    public static RobotWaypoint BuildFrameOriginPreviewWaypoint(
        PathDocument document,
        RobotCalibrationFile? calibration)
    {
        double exportX = double.NaN;
        double exportY = double.NaN;
        double exportR = double.NaN;
        double angleDeg = WaypointExportService.GetEffectiveExportAngleDeg(document.Frame, document.Defaults);
        (double sensorX, double sensorY) = CoordinateTransform.LocalToSensor(0, 0, document.Frame, angleDeg);

        if (calibration?.PixelToRobot != null)
        {
            WaypointExportValues values = WaypointExportService.ComputeExportValues(
                0,
                0,
                document.Frame,
                calibration,
                document.Defaults);
            exportX = values.ExportX_mm;
            exportY = values.ExportY_mm;
            exportR = values.ExportR_deg;
            sensorX = values.SensorX_px;
            sensorY = values.SensorY_px;
        }

        return new RobotWaypoint
        {
            Index = 0,
            SegmentLabel = "Frame origin",
            Role = "Frame",
            LocalX_px = 0,
            LocalY_px = 0,
            SensorX_px = sensorX,
            SensorY_px = sensorY,
            ExportX_mm = exportX,
            ExportY_mm = exportY,
            ExportR_deg = exportR,
            RobotX_mm = exportX,
            RobotY_mm = exportY,
            R_deg = exportR,
            GlueOn = false,
            IsPreviewOnly = true
        };
    }

    public static IReadOnlyList<RobotWaypoint> BuildPreviewWaypoints(
        PathDocument document,
        RobotCalibrationFile? calibration)
    {
        var waypoints = new List<RobotWaypoint>
        {
            BuildFrameOriginPreviewWaypoint(document, calibration)
        };
        waypoints.AddRange(BuildWaypoints(document, calibration));
        return waypoints;
    }
}
