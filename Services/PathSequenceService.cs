using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public sealed class SequenceGap
{
    public int FromSegmentIndex { get; init; }
    public int ToSegmentIndex { get; init; }
    public double FromX_px { get; init; }
    public double FromY_px { get; init; }
    public double ToX_px { get; init; }
    public double ToY_px { get; init; }
    public double Distance_px { get; init; }

    public bool HasGap(double thresholdPx) => Distance_px > thresholdPx;
}

public static class PathSequenceService
{
    public const double DefaultGapThresholdPx = 5.0;

    public static (double X, double Y) GetStartPoint(PathSegment segment) =>
        (segment.X1, segment.Y1);

    public static (double X, double Y) GetEndPoint(PathSegment segment) =>
        segment.Kind switch
        {
            SegmentKind.Line => (segment.X2, segment.Y2),
            SegmentKind.Arc => (segment.X3, segment.Y3),
            SegmentKind.Dot => (segment.X1, segment.Y1),
            _ => (segment.X1, segment.Y1)
        };

    public static IReadOnlyList<SequenceGap> AnalyzeGaps(IReadOnlyList<PathSegment> segments)
    {
        if (segments.Count < 2)
        {
            return Array.Empty<SequenceGap>();
        }

        var gaps = new List<SequenceGap>(segments.Count - 1);
        for (int i = 0; i < segments.Count - 1; i++)
        {
            (double fromX, double fromY) = GetEndPoint(segments[i]);
            (double toX, double toY) = GetStartPoint(segments[i + 1]);
            gaps.Add(new SequenceGap
            {
                FromSegmentIndex = i,
                ToSegmentIndex = i + 1,
                FromX_px = fromX,
                FromY_px = fromY,
                ToX_px = toX,
                ToY_px = toY,
                Distance_px = Distance(fromX, fromY, toX, toY)
            });
        }

        return gaps;
    }

    public static int CountGaps(IReadOnlyList<PathSegment> segments, double thresholdPx = DefaultGapThresholdPx) =>
        AnalyzeGaps(segments).Count(gap => gap.HasGap(thresholdPx));

    public static bool CanMoveSegment(int segmentCount, int index, int delta)
    {
        if (segmentCount == 0)
        {
            return false;
        }

        int newIndex = index + delta;
        return index >= 0 && index < segmentCount && newIndex >= 0 && newIndex < segmentCount;
    }

    public static void MoveSegment(IList<PathSegment> segments, int index, int delta)
    {
        if (!CanMoveSegment(segments.Count, index, delta))
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Segment cannot be moved in that direction.");
        }

        PathSegment segment = segments[index];
        segments.RemoveAt(index);
        segments.Insert(index + delta, segment);
    }

    private static double Distance(double x1, double y1, double x2, double y2)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
