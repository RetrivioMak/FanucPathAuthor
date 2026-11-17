using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

/// <summary>
/// Turns DXF circle data into start/mid/end arcs. Angles are degrees, counterclockwise from +X.
/// </summary>
public static class DxfCurve
{
    public static PathSegment Line(double x1, double y1, double x2, double y2, bool glueOn, double speedMmPerS) =>
        new()
        {
            Kind = SegmentKind.Line,
            GlueOn = glueOn,
            Speed_mm_s = speedMmPerS,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2
        };

    public static PathSegment Dot(double x, double y, bool glueOn, double speedMmPerS) =>
        new()
        {
            Kind = SegmentKind.Dot,
            GlueOn = glueOn,
            Speed_mm_s = speedMmPerS,
            X1 = x,
            Y1 = y
        };

    public static PathSegment? Arc(
        double centerX,
        double centerY,
        double radius,
        double startDeg,
        double endDeg,
        bool glueOn,
        double speedMmPerS)
    {
        if (radius < 1e-9 || double.IsNaN(radius))
        {
            return null;
        }

        double sweep = PositiveSweep(endDeg - startDeg);
        if (sweep < 1e-4)
        {
            return null;
        }

        (double x1, double y1) = OnCircle(centerX, centerY, radius, startDeg);
        (double x2, double y2) = OnCircle(centerX, centerY, radius, startDeg + sweep / 2);
        (double x3, double y3) = OnCircle(centerX, centerY, radius, startDeg + sweep);
        return ArcThrough(x1, y1, x2, y2, x3, y3, glueOn, speedMmPerS);
    }

    /// <summary>
    /// Bulge is tan(included angle / 4). Positive bulge runs counterclockwise from start to end.
    /// </summary>
    public static bool TryBulgeMidpoint(
        double x1,
        double y1,
        double x2,
        double y2,
        double bulge,
        out double midX,
        out double midY)
    {
        midX = 0;
        midY = 0;
        if (Math.Abs(bulge) < 1e-12)
        {
            return false;
        }

        double dx = x2 - x1;
        double dy = y2 - y1;
        double chord = Math.Sqrt(dx * dx + dy * dy);
        if (chord < 1e-9)
        {
            return false;
        }

        double absTheta = 4 * Math.Atan(Math.Abs(bulge));
        double gamma = 0.5 * (Math.PI - absTheta);
        double phi = Math.Atan2(dy, dx) + Math.Sign(bulge) * gamma;
        double radius = (0.5 * chord) / Math.Sin(0.5 * absTheta);
        double centerX = x1 + radius * Math.Cos(phi);
        double centerY = y1 + radius * Math.Sin(phi);
        double start = Math.Atan2(y1 - centerY, x1 - centerX);
        double mid = start + Math.Sign(bulge) * absTheta / 2;
        midX = centerX + radius * Math.Cos(mid);
        midY = centerY + radius * Math.Sin(mid);
        return true;
    }

    public static (double X, double Y) OnCircle(double centerX, double centerY, double radius, double angleDeg)
    {
        double radians = angleDeg * Math.PI / 180.0;
        return (centerX + radius * Math.Cos(radians), centerY + radius * Math.Sin(radians));
    }

    private static PathSegment ArcThrough(
        double x1,
        double y1,
        double x2,
        double y2,
        double x3,
        double y3,
        bool glueOn,
        double speedMmPerS) =>
        new()
        {
            Kind = SegmentKind.Arc,
            GlueOn = glueOn,
            Speed_mm_s = speedMmPerS,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            X3 = x3,
            Y3 = y3
        };

    private static double PositiveSweep(double degrees)
    {
        double sweep = degrees % 360;
        if (sweep < 0)
        {
            sweep += 360;
        }

        return sweep;
    }
}
