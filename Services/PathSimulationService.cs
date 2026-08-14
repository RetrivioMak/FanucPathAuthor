using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public enum SimulationMoveKind
{
    DotDwell,
    SegmentPath,
    GapJump
}

public sealed class SimulationMove
{
    public SimulationMoveKind Kind { get; init; }
    public bool GlueOn { get; init; }
    public int? SegmentIndex { get; init; }
    public double Speed_px_s { get; init; }
    public IReadOnlyList<(double X, double Y)> Points { get; init; } = Array.Empty<(double, double)>();
    public double Length_px { get; init; }

    public const double DotDwellDuration_s = 0.08;

    public double Duration_s =>
        Kind == SimulationMoveKind.DotDwell
            ? DotDwellDuration_s
            : Length_px / Math.Max(Speed_px_s, 1e-6);
}

public sealed class SimulationPose
{
    public double LocalX { get; init; }
    public double LocalY { get; init; }
    public bool GlueOn { get; init; }
    public int? SegmentIndex { get; init; }
    public double Elapsed_s { get; init; }
    public double TotalDuration_s { get; init; }
    public double Progress => TotalDuration_s > 1e-6 ? Math.Clamp(Elapsed_s / TotalDuration_s, 0, 1) : 0;
    public bool IsComplete { get; init; }
}

public sealed class PathSimulationTimeline
{
    public IReadOnlyList<SimulationMove> Moves { get; init; } = Array.Empty<SimulationMove>();
    public double TotalDuration_s { get; init; }

    public SimulationPose GetPoseAtTime(double time_s)
    {
        if (Moves.Count == 0)
        {
            return new SimulationPose
            {
                TotalDuration_s = 0,
                Elapsed_s = 0,
                IsComplete = true
            };
        }

        double clampedTime = Math.Max(0, time_s);
        double moveStart = 0;

        foreach (SimulationMove move in Moves)
        {
            double moveEnd = moveStart + move.Duration_s;
            if (clampedTime <= moveEnd || move == Moves[^1])
            {
                double localTime = Math.Min(clampedTime, moveEnd) - moveStart;
                (double x, double y) = InterpolateMove(move, localTime);
                return new SimulationPose
                {
                    LocalX = x,
                    LocalY = y,
                    GlueOn = move.GlueOn,
                    SegmentIndex = move.SegmentIndex,
                    Elapsed_s = Math.Min(clampedTime, TotalDuration_s),
                    TotalDuration_s = TotalDuration_s,
                    IsComplete = clampedTime >= TotalDuration_s - 1e-6
                };
            }

            moveStart = moveEnd;
        }

        SimulationMove last = Moves[^1];
        (double lastX, double lastY) = last.Points[^1];
        return new SimulationPose
        {
            LocalX = lastX,
            LocalY = lastY,
            GlueOn = last.GlueOn,
            SegmentIndex = last.SegmentIndex,
            Elapsed_s = TotalDuration_s,
            TotalDuration_s = TotalDuration_s,
            IsComplete = true
        };
    }

    private static (double X, double Y) InterpolateMove(SimulationMove move, double localTime_s)
    {
        if (move.Points.Count == 0)
        {
            return (0, 0);
        }

        if (move.Kind == SimulationMoveKind.DotDwell || move.Length_px <= 1e-6)
        {
            return move.Points[0];
        }

        double targetDistance = move.Speed_px_s * localTime_s;
        return PointAlongPolyline(move.Points, targetDistance);
    }

    private static (double X, double Y) PointAlongPolyline(
        IReadOnlyList<(double X, double Y)> points,
        double distance_px)
    {
        if (points.Count == 1)
        {
            return points[0];
        }

        double remaining = Math.Max(0, distance_px);
        for (int i = 1; i < points.Count; i++)
        {
            (double x1, double y1) = points[i - 1];
            (double x2, double y2) = points[i];
            double segmentLength = Distance(x1, y1, x2, y2);
            if (segmentLength <= 1e-6)
            {
                continue;
            }

            if (remaining <= segmentLength)
            {
                double t = remaining / segmentLength;
                return (x1 + (x2 - x1) * t, y1 + (y2 - y1) * t);
            }

            remaining -= segmentLength;
        }

        return points[^1];
    }

    private static double Distance(double x1, double y1, double x2, double y2)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

public static class PathSimulationService
{
    /// <summary>
    /// Visual playback speed in px/s at the default process speed (50 mm/s).
    /// Robot mm/s values are scaled relative to this for preview only.
    /// </summary>
    public const double SimulationBaseSpeed_px_s = 1200;
    public const double DefaultReferenceSpeed_mm_s = 50;
    public const double TravelSpeedFactor = 2.5;
    private const double MinSimulationSpeed_px_s = 200;
    private const double MinPointSpacing_px = 4;
    private const int MinArcSteps = 16;
    private const int MaxArcSteps = 96;

    public static PathSimulationTimeline BuildTimeline(PathDocument document)
    {
        var moves = new List<SimulationMove>();
        IReadOnlyList<PathSegment> segments = document.Segments;
        if (segments.Count == 0)
        {
            return new PathSimulationTimeline();
        }

        for (int i = 0; i < segments.Count; i++)
        {
            if (i > 0)
            {
                SimulationMove? gapMove = CreateGapMove(segments, i - 1, i, document);
                if (gapMove != null)
                {
                    moves.Add(gapMove);
                }
            }

            moves.Add(CreateSegmentMove(segments[i], i, document));
        }

        double totalDuration = moves.Sum(move => move.Duration_s);
        return new PathSimulationTimeline
        {
            Moves = moves,
            TotalDuration_s = totalDuration
        };
    }

    private static SimulationMove? CreateGapMove(
        IReadOnlyList<PathSegment> segments,
        int fromIndex,
        int toIndex,
        PathDocument document)
    {
        (double fromX, double fromY) = PathSequenceService.GetEndPoint(segments[fromIndex]);
        (double toX, double toY) = PathSequenceService.GetStartPoint(segments[toIndex]);
        double length = Distance(fromX, fromY, toX, toY);
        if (length <= 0.05)
        {
            return null;
        }

        var points = new List<(double X, double Y)>
        {
            (fromX, fromY),
            (toX, toY)
        };

        return new SimulationMove
        {
            Kind = SimulationMoveKind.GapJump,
            GlueOn = false,
            SegmentIndex = toIndex,
            Speed_px_s = ResolveTravelSpeed(document),
            Points = points,
            Length_px = length
        };
    }

    private static SimulationMove CreateSegmentMove(PathSegment segment, int segmentIndex, PathDocument document)
    {
        double speed = ResolveSegmentSpeed(segment, document);
        IReadOnlyList<(double X, double Y)> points = segment.Kind switch
        {
            SegmentKind.Dot => new[] { (segment.X1, segment.Y1) },
            SegmentKind.Line => new[] { (segment.X1, segment.Y1), (segment.X2, segment.Y2) },
            SegmentKind.Arc => SampleArc(segment),
            _ => Array.Empty<(double, double)>()
        };

        if (segment.Kind == SegmentKind.Dot)
        {
            return new SimulationMove
            {
                Kind = SimulationMoveKind.DotDwell,
                GlueOn = segment.GlueOn,
                SegmentIndex = segmentIndex,
                Speed_px_s = speed,
                Points = points,
                Length_px = 0
            };
        }

        return new SimulationMove
        {
            Kind = SimulationMoveKind.SegmentPath,
            GlueOn = segment.GlueOn,
            SegmentIndex = segmentIndex,
            Speed_px_s = speed,
            Points = points,
            Length_px = PolylineLength(points)
        };
    }

    private static double ResolveSegmentSpeed(PathSegment segment, PathDocument document)
    {
        double robotSpeed = segment.Speed_mm_s > 0 ? segment.Speed_mm_s : document.Defaults.Speed_mm_s;
        double referenceSpeed = document.Defaults.Speed_mm_s > 0
            ? document.Defaults.Speed_mm_s
            : DefaultReferenceSpeed_mm_s;
        double px_s = SimulationBaseSpeed_px_s * (robotSpeed / referenceSpeed);
        return Math.Max(px_s, MinSimulationSpeed_px_s);
    }

    private static double ResolveTravelSpeed(PathDocument document) =>
        Math.Max(SimulationBaseSpeed_px_s * TravelSpeedFactor, MinSimulationSpeed_px_s * TravelSpeedFactor);

    public static IReadOnlyList<(double X, double Y)> SampleArc(PathSegment segment)
    {
        if (!PointEditHelper.TryArcCenter(
                segment.X1,
                segment.Y1,
                segment.X2,
                segment.Y2,
                segment.X3,
                segment.Y3,
                out double centerX,
                out double centerY,
                out double radius))
        {
            return new[] { (segment.X1, segment.Y1), (segment.X3, segment.Y3) };
        }

        double startAngle = Math.Atan2(segment.Y1 - centerY, segment.X1 - centerX);
        double midAngle = Math.Atan2(segment.Y2 - centerY, segment.X2 - centerX);
        double endAngle = Math.Atan2(segment.Y3 - centerY, segment.X3 - centerX);
        double sweep = ComputeSweepThroughMid(startAngle, midAngle, endAngle);
        double arcLength = Math.Abs(sweep) * radius;
        int steps = (int)Math.Clamp(Math.Ceiling(arcLength / MinPointSpacing_px), MinArcSteps, MaxArcSteps);

        var points = new List<(double X, double Y)>(steps + 1);
        for (int i = 0; i <= steps; i++)
        {
            double t = (double)i / steps;
            double angle = startAngle + sweep * t;
            points.Add((centerX + radius * Math.Cos(angle), centerY + radius * Math.Sin(angle)));
        }

        return points;
    }

    private static double ComputeSweepThroughMid(double startAngle, double midAngle, double endAngle)
    {
        double sweepToMid = NormalizeAngleDelta(startAngle, midAngle);
        double sweepToEnd = NormalizeAngleDelta(startAngle, endAngle);
        if (Math.Abs(sweepToMid) <= Math.Abs(NormalizeAngleDelta(startAngle, endAngle - Math.Sign(sweepToMid) * 2 * Math.PI)))
        {
            return sweepToEnd;
        }

        return sweepToEnd - Math.Sign(sweepToEnd) * 2 * Math.PI;
    }

    private static double NormalizeAngleDelta(double startAngle, double targetAngle)
    {
        double delta = targetAngle - startAngle;
        while (delta <= -Math.PI)
        {
            delta += 2 * Math.PI;
        }

        while (delta > Math.PI)
        {
            delta -= 2 * Math.PI;
        }

        return delta;
    }

    private static double PolylineLength(IReadOnlyList<(double X, double Y)> points)
    {
        double length = 0;
        for (int i = 1; i < points.Count; i++)
        {
            length += Distance(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y);
        }

        return length;
    }

    private static double Distance(double x1, double y1, double x2, double y2)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
