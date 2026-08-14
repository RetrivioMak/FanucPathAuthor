using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class ShapeExpander
{
    /// <summary>
    /// Expands a rectangle (two opposite corners in frame-local px) into four line segments.
    /// </summary>
    public static IReadOnlyList<PathSegment> RectangleToLines(
        double x1,
        double y1,
        double x2,
        double y2,
        bool glueOn,
        double speed_mm_s)
    {
        double minX = Math.Min(x1, x2);
        double maxX = Math.Max(x1, x2);
        double minY = Math.Min(y1, y2);
        double maxY = Math.Max(y1, y2);

        (double X, double Y)[] corners =
        [
            (minX, minY),
            (maxX, minY),
            (maxX, maxY),
            (minX, maxY)
        ];

        var lines = new List<PathSegment>(4);
        for (int i = 0; i < 4; i++)
        {
            (double ax, double ay) = corners[i];
            (double bx, double by) = corners[(i + 1) % 4];
            lines.Add(new PathSegment
            {
                Kind = SegmentKind.Line,
                GlueOn = glueOn,
                Speed_mm_s = speed_mm_s,
                X1 = ax,
                Y1 = ay,
                X2 = bx,
                Y2 = by
            });
        }

        return lines;
    }

    /// <summary>
    /// Expands a circle (center + point on radius in frame-local px) into two semicircle arc segments.
    /// </summary>
    public static IReadOnlyList<PathSegment> CircleToArcs(
        double centerX,
        double centerY,
        double radiusPointX,
        double radiusPointY,
        bool glueOn,
        double speed_mm_s)
    {
        double dx = radiusPointX - centerX;
        double dy = radiusPointY - centerY;
        double radius = Math.Sqrt(dx * dx + dy * dy);
        if (radius < 1e-6)
        {
            return Array.Empty<PathSegment>();
        }

        return
        [
            CreateArcSegment(
                centerX + radius,
                centerY,
                centerX,
                centerY + radius,
                centerX - radius,
                centerY,
                glueOn,
                speed_mm_s),
            CreateArcSegment(
                centerX - radius,
                centerY,
                centerX,
                centerY - radius,
                centerX + radius,
                centerY,
                glueOn,
                speed_mm_s)
        ];
    }

    private static PathSegment CreateArcSegment(
        double x1,
        double y1,
        double x2,
        double y2,
        double x3,
        double y3,
        bool glueOn,
        double speed_mm_s) =>
        new()
        {
            Kind = SegmentKind.Arc,
            GlueOn = glueOn,
            Speed_mm_s = speed_mm_s,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            X3 = x3,
            Y3 = y3
        };
}
