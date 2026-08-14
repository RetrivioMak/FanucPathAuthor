using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public enum VertexDragMode
{
    SharedPoint,
    ArcPoint,
    CircleCenter,
    CircleRadius
}

public sealed class VertexPick
{
    public VertexDragMode Mode { get; init; }
    public IReadOnlyList<(int SegmentIndex, int PointSlot)> Bindings { get; init; } = [];
    public double LocalX { get; init; }
    public double LocalY { get; init; }
    public int CircleArcIndexA { get; init; } = -1;
    public int CircleArcIndexB { get; init; } = -1;
    public double CircleCenterX { get; init; }
    public double CircleCenterY { get; init; }
    public double CircleRadius { get; init; }
}

public static class PointEditHelper
{
    private const double PointTolerance = 0.5;

    public static bool TryPickVertex(
        PathDocument document,
        double sensorX,
        double sensorY,
        Func<double, double, PointF> sensorToScreen,
        double pickRadiusScreenPx,
        out VertexPick? pick)
    {
        pick = null;
        if (document.Segments.Count == 0)
        {
            return false;
        }

        PointF clickScreen = sensorToScreen(sensorX, sensorY);
        double bestDist = double.MaxValue;
        VertexPick? bestPick = null;

        for (int segmentIndex = 0; segmentIndex < document.Segments.Count; segmentIndex++)
        {
            PathSegment segment = document.Segments[segmentIndex];
            foreach (int pointSlot in GetPointSlots(segment.Kind))
            {
                (double localX, double localY) = GetPoint(segment, pointSlot);
                var (pointSensorX, pointSensorY) = CoordinateTransform.LocalToSensor(
                    localX,
                    localY,
                    document.Frame);
                PointF pointScreen = sensorToScreen(pointSensorX, pointSensorY);
                double dist = ScreenDistance(clickScreen, pointScreen);
                if (dist > pickRadiusScreenPx || dist >= bestDist)
                {
                    continue;
                }

                bestDist = dist;
                bestPick = CreateVertexPick(document, segmentIndex, pointSlot, localX, localY);
            }
        }

        foreach ((int indexA, int indexB, double centerX, double centerY, double radius) in EnumerateCircles(document.Segments))
        {
            var (centerSensorX, centerSensorY) = CoordinateTransform.LocalToSensor(
                centerX,
                centerY,
                document.Frame);
            PointF centerScreen = sensorToScreen(centerSensorX, centerSensorY);
            double dist = ScreenDistance(clickScreen, centerScreen);
            if (dist > pickRadiusScreenPx || dist >= bestDist)
            {
                continue;
            }

            bestDist = dist;
            bestPick = new VertexPick
            {
                Mode = VertexDragMode.CircleCenter,
                Bindings = [],
                LocalX = centerX,
                LocalY = centerY,
                CircleArcIndexA = indexA,
                CircleArcIndexB = indexB,
                CircleCenterX = centerX,
                CircleCenterY = centerY,
                CircleRadius = radius
            };
        }

        if (bestPick == null)
        {
            return false;
        }

        pick = bestPick;
        return true;
    }

    public static void ApplyDrag(PathDocument document, VertexPick pick, double newLocalX, double newLocalY)
    {
        switch (pick.Mode)
        {
            case VertexDragMode.SharedPoint:
            case VertexDragMode.ArcPoint:
                SetBindings(document, pick.Bindings, newLocalX, newLocalY);
                break;

            case VertexDragMode.CircleCenter:
                MoveCircleCenter(document, pick, newLocalX, newLocalY);
                break;

            case VertexDragMode.CircleRadius:
                ResizeCircle(document, pick, newLocalX, newLocalY);
                break;
        }
    }

    public static (double SensorX, double SensorY)? GetHoverSensorPoint(
        PathDocument document,
        double sensorX,
        double sensorY,
        Func<double, double, PointF> sensorToScreen,
        double pickRadiusScreenPx)
    {
        if (!TryPickVertex(document, sensorX, sensorY, sensorToScreen, pickRadiusScreenPx, out VertexPick? pick) ||
            pick == null)
        {
            return null;
        }

        var (hoverSensorX, hoverSensorY) = CoordinateTransform.LocalToSensor(
            pick.LocalX,
            pick.LocalY,
            document.Frame);
        return (hoverSensorX, hoverSensorY);
    }

    private static VertexPick CreateVertexPick(
        PathDocument document,
        int segmentIndex,
        int pointSlot,
        double localX,
        double localY)
    {
        if (TryGetCircleForArc(document.Segments, segmentIndex, out int arcA, out int arcB, out double centerX, out double centerY, out double radius))
        {
            return new VertexPick
            {
                Mode = VertexDragMode.CircleRadius,
                Bindings = [(segmentIndex, pointSlot)],
                LocalX = localX,
                LocalY = localY,
                CircleArcIndexA = arcA,
                CircleArcIndexB = arcB,
                CircleCenterX = centerX,
                CircleCenterY = centerY,
                CircleRadius = radius
            };
        }

        PathSegment segment = document.Segments[segmentIndex];
        if (segment.Kind == SegmentKind.Arc)
        {
            return new VertexPick
            {
                Mode = VertexDragMode.ArcPoint,
                Bindings = [(segmentIndex, pointSlot)],
                LocalX = localX,
                LocalY = localY
            };
        }

        IReadOnlyList<(int SegmentIndex, int PointSlot)> bindings =
            FindBindingsAt(document.Segments, localX, localY);

        return new VertexPick
        {
            Mode = VertexDragMode.SharedPoint,
            Bindings = bindings,
            LocalX = localX,
            LocalY = localY
        };
    }

    private static IReadOnlyList<(int SegmentIndex, int PointSlot)> FindBindingsAt(
        IReadOnlyList<PathSegment> segments,
        double localX,
        double localY)
    {
        var bindings = new List<(int SegmentIndex, int PointSlot)>();
        for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
        {
            PathSegment segment = segments[segmentIndex];
            foreach (int pointSlot in GetPointSlots(segment.Kind))
            {
                (double pointX, double pointY) = GetPoint(segment, pointSlot);
                if (Math.Abs(pointX - localX) <= PointTolerance &&
                    Math.Abs(pointY - localY) <= PointTolerance)
                {
                    bindings.Add((segmentIndex, pointSlot));
                }
            }
        }

        return bindings;
    }

    private static void SetBindings(
        PathDocument document,
        IReadOnlyList<(int SegmentIndex, int PointSlot)> bindings,
        double localX,
        double localY)
    {
        foreach ((int segmentIndex, int pointSlot) in bindings)
        {
            SetPoint(document.Segments[segmentIndex], pointSlot, localX, localY);
        }
    }

    private static void MoveCircleCenter(PathDocument document, VertexPick pick, double newCenterX, double newCenterY)
    {
        PathSegment arcA = document.Segments[pick.CircleArcIndexA];
        PathSegment arcB = document.Segments[pick.CircleArcIndexB];
        if (!TryParseCirclePair(arcA, arcB, out double currentCenterX, out double currentCenterY, out _))
        {
            return;
        }

        double deltaX = newCenterX - currentCenterX;
        double deltaY = newCenterY - currentCenterY;

        foreach (int arcIndex in new[] { pick.CircleArcIndexA, pick.CircleArcIndexB })
        {
            PathSegment arc = document.Segments[arcIndex];
            SetPoint(arc, 0, arc.X1 + deltaX, arc.Y1 + deltaY);
            SetPoint(arc, 1, arc.X2 + deltaX, arc.Y2 + deltaY);
            SetPoint(arc, 2, arc.X3 + deltaX, arc.Y3 + deltaY);
        }
    }

    private static void ResizeCircle(PathDocument document, VertexPick pick, double radiusPointX, double radiusPointY)
    {
        PathSegment template = document.Segments[pick.CircleArcIndexA];
        IReadOnlyList<PathSegment> arcs = ShapeExpander.CircleToArcs(
            pick.CircleCenterX,
            pick.CircleCenterY,
            radiusPointX,
            radiusPointY,
            template.GlueOn,
            template.Speed_mm_s);

        if (arcs.Count != 2)
        {
            return;
        }

        int firstIndex = Math.Min(pick.CircleArcIndexA, pick.CircleArcIndexB);
        int secondIndex = Math.Max(pick.CircleArcIndexA, pick.CircleArcIndexB);
        CopyArcGeometry(document.Segments[firstIndex], arcs[0]);
        CopyArcGeometry(document.Segments[secondIndex], arcs[1]);
    }

    private static void CopyArcGeometry(PathSegment target, PathSegment source)
    {
        target.X1 = source.X1;
        target.Y1 = source.Y1;
        target.X2 = source.X2;
        target.Y2 = source.Y2;
        target.X3 = source.X3;
        target.Y3 = source.Y3;
    }

    public static bool TryGetCircleForArc(
        IReadOnlyList<PathSegment> segments,
        int arcIndex,
        out int arcIndexA,
        out int arcIndexB,
        out double centerX,
        out double centerY,
        out double radius)
    {
        arcIndexA = -1;
        arcIndexB = -1;
        centerX = 0;
        centerY = 0;
        radius = 0;

        if (arcIndex < 0 || arcIndex >= segments.Count || segments[arcIndex].Kind != SegmentKind.Arc)
        {
            return false;
        }

        for (int otherIndex = 0; otherIndex < segments.Count; otherIndex++)
        {
            if (otherIndex == arcIndex || segments[otherIndex].Kind != SegmentKind.Arc)
            {
                continue;
            }

            if (TryParseCirclePair(segments[arcIndex], segments[otherIndex], out centerX, out centerY, out radius))
            {
                arcIndexA = Math.Min(arcIndex, otherIndex);
                arcIndexB = Math.Max(arcIndex, otherIndex);
                return true;
            }
        }

        return false;
    }

    public static IEnumerable<(int IndexA, int IndexB, double CenterX, double CenterY, double Radius)> EnumerateCircles(
        IReadOnlyList<PathSegment> segments)
    {
        for (int indexA = 0; indexA < segments.Count; indexA++)
        {
            if (segments[indexA].Kind != SegmentKind.Arc)
            {
                continue;
            }

            for (int indexB = indexA + 1; indexB < segments.Count; indexB++)
            {
                if (segments[indexB].Kind != SegmentKind.Arc)
                {
                    continue;
                }

                if (TryParseCirclePair(segments[indexA], segments[indexB], out double centerX, out double centerY, out double radius))
                {
                    yield return (indexA, indexB, centerX, centerY, radius);
                }
            }
        }
    }

    public static bool TryParseCirclePair(
        PathSegment arcA,
        PathSegment arcB,
        out double centerX,
        out double centerY,
        out double radius)
    {
        centerX = 0;
        centerY = 0;
        radius = 0;

        if (arcA.Kind != SegmentKind.Arc || arcB.Kind != SegmentKind.Arc)
        {
            return false;
        }

        if (!PointsEqual(arcA.X3, arcA.Y3, arcB.X1, arcB.Y1) ||
            !PointsEqual(arcB.X3, arcB.Y3, arcA.X1, arcA.Y1))
        {
            return false;
        }

        if (!TryArcCenter(arcA.X1, arcA.Y1, arcA.X2, arcA.Y2, arcA.X3, arcA.Y3, out centerX, out centerY, out radius))
        {
            return false;
        }

        return PointOnCircle(arcB.X2, arcB.Y2, centerX, centerY, radius) &&
               PointOnCircle(arcB.X3, arcB.Y3, centerX, centerY, radius);
    }

    public static bool TryArcCenter(
        double x1,
        double y1,
        double x2,
        double y2,
        double x3,
        double y3,
        out double centerX,
        out double centerY,
        out double radius)
    {
        centerX = 0;
        centerY = 0;
        radius = 0;

        double d = 2 * (x1 * (y2 - y3) +
                        x2 * (y3 - y1) +
                        x3 * (y1 - y2));
        if (Math.Abs(d) < 1e-6)
        {
            return false;
        }

        double startSq = x1 * x1 + y1 * y1;
        double midSq = x2 * x2 + y2 * y2;
        double endSq = x3 * x3 + y3 * y3;

        centerX = (startSq * (y2 - y3) +
                   midSq * (y3 - y1) +
                   endSq * (y1 - y2)) / d;
        centerY = (startSq * (x3 - x2) +
                   midSq * (x1 - x3) +
                   endSq * (x2 - x1)) / d;

        radius = Math.Sqrt((x1 - centerX) * (x1 - centerX) +
                           (y1 - centerY) * (y1 - centerY));
        return radius > 1e-6;
    }

    private static IEnumerable<int> GetPointSlots(SegmentKind kind) =>
        kind switch
        {
            SegmentKind.Dot => [0],
            SegmentKind.Line => [0, 1],
            SegmentKind.Arc => [0, 1, 2],
            _ => []
        };

    private static (double X, double Y) GetPoint(PathSegment segment, int pointSlot) =>
        pointSlot switch
        {
            0 => (segment.X1, segment.Y1),
            1 => (segment.X2, segment.Y2),
            2 => (segment.X3, segment.Y3),
            _ => throw new ArgumentOutOfRangeException(nameof(pointSlot))
        };

    private static void SetPoint(PathSegment segment, int pointSlot, double x, double y)
    {
        switch (pointSlot)
        {
            case 0:
                segment.X1 = x;
                segment.Y1 = y;
                break;
            case 1:
                segment.X2 = x;
                segment.Y2 = y;
                break;
            case 2:
                segment.X3 = x;
                segment.Y3 = y;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(pointSlot));
        }
    }

    private static bool PointsEqual(double x1, double y1, double x2, double y2) =>
        Math.Abs(x1 - x2) <= PointTolerance && Math.Abs(y1 - y2) <= PointTolerance;

    private static bool PointOnCircle(double x, double y, double centerX, double centerY, double radius) =>
        Math.Abs(Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY)) - radius) <= 1.0;

    private static double ScreenDistance(PointF a, PointF b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
