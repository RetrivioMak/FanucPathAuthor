using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class CoordinateTransform
{
    public static (double X, double Y) LocalToSensor(
        double localX,
        double localY,
        FrameDefinition frame,
        double? angleDegOverride = null)
    {
        double angleDeg = angleDegOverride ?? frame.AngleDeg;
        double orientedY = frame.InvertY ? -localY : localY;
        double rad = DegreesToRadians(angleDeg);
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        return (
            frame.OriginX_px + localX * cos - orientedY * sin,
            frame.OriginY_px + localX * sin + orientedY * cos);
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
        double localX = dx * cos - dy * sin;
        double localY = dx * sin + dy * cos;
        return frame.InvertY ? (localX, -localY) : (localX, localY);
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
        if (document.Workspace == WorkspaceKind.Teach)
        {
            return BuildTeachWaypoints(document);
        }

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
                        document.Exporters.FanucLs);
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

    private static IReadOnlyList<RobotWaypoint> BuildTeachWaypoints(PathDocument document)
    {
        var waypoints = new List<RobotWaypoint>();
        int index = 1;
        foreach (PathSegment segment in document.Segments)
        {
            string[] roles = segment.Kind switch
            {
                SegmentKind.Dot => ["dot"],
                SegmentKind.Line => ["start", "end"],
                SegmentKind.Arc => ["start", "mid", "end"],
                _ => []
            };

            int roleIndex = 0;
            foreach ((double localX, double localY) in ExpandSegment(segment))
            {
                WaypointExportValues values = TeachWaypointExport.Compute(
                    localX,
                    localY,
                    document.Defaults,
                    document.Exporters.FanucLs);
                waypoints.Add(new RobotWaypoint
                {
                    Index = index++,
                    SegmentLabel = segment.DisplayName,
                    Role = roles[roleIndex++],
                    LocalX_px = localX,
                    LocalY_px = localY,
                    SensorX_px = localX,
                    SensorY_px = localY,
                    ExportX_mm = values.ExportX_mm,
                    ExportY_mm = values.ExportY_mm,
                    ExportR_deg = values.ExportR_deg,
                    RobotX_mm = values.ExportX_mm,
                    RobotY_mm = values.ExportY_mm,
                    R_deg = values.ExportR_deg,
                    Z_mm = document.Defaults.Z_mm,
                    W_deg = document.Defaults.W_deg,
                    P_deg = document.Defaults.P_deg,
                    Speed_mm_s = segment.Speed_mm_s,
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
        if (document.Workspace == WorkspaceKind.Teach)
        {
            WaypointExportValues values = TeachWaypointExport.Compute(0, 0, document.Defaults, document.Exporters.FanucLs);
            return new RobotWaypoint
            {
                Index = 0,
                SegmentLabel = "Frame origin",
                Role = "Frame",
                LocalX_px = 0,
                LocalY_px = 0,
                SensorX_px = 0,
                SensorY_px = 0,
                ExportX_mm = values.ExportX_mm,
                ExportY_mm = values.ExportY_mm,
                ExportR_deg = values.ExportR_deg,
                RobotX_mm = values.ExportX_mm,
                RobotY_mm = values.ExportY_mm,
                R_deg = values.ExportR_deg,
                Z_mm = document.Defaults.Z_mm,
                W_deg = document.Defaults.W_deg,
                P_deg = document.Defaults.P_deg,
                GlueOn = false,
                IsPreviewOnly = true
            };
        }

        double exportX = double.NaN;
        double exportY = double.NaN;
        double exportR = double.NaN;
        double angleDeg = document.Frame.AngleDeg;
        (double sensorX, double sensorY) = CoordinateTransform.LocalToSensor(0, 0, document.Frame, angleDeg);

        if (calibration?.PixelToRobot != null)
        {
            WaypointExportValues values = WaypointExportService.ComputeExportValues(
                0,
                0,
                document.Frame,
                calibration,
                document.Exporters.FanucLs);
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
