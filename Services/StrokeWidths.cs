using System.Globalization;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class StrokeWidths
{
    public const double MinMm = 0.001;
    public const double MaxMm = 1000;
    public const double ReferenceMin = 0.0001;
    public const double ReferenceMax = 100;

    public static string Format(double value) =>
        Normalize(value).ToString("0.###", CultureInfo.CurrentCulture);

    public static double Normalize(double value) =>
        Math.Round(value, 3, MidpointRounding.AwayFromZero);

    public static double Floor(double minimum)
    {
        if (minimum <= 0 || !double.IsFinite(minimum))
        {
            return ProcessDefaults.DefaultMinWidthMm;
        }

        return Normalize(Math.Clamp(minimum, MinMm, MaxMm));
    }

    public static bool NearlyEqual(double left, double right) => Math.Abs(left - right) < 0.0005;

    public static bool TryParse(string? text, double minimum, out double value)
    {
        double floor = Floor(minimum);
        if (!string.IsNullOrWhiteSpace(text) &&
            (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
             double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) &&
            value >= floor - 0.0005 &&
            value <= MaxMm)
        {
            value = Math.Max(Normalize(value), floor);
            return true;
        }

        value = 0;
        return false;
    }

    public static double Effective(PathSegment segment, double minimum)
    {
        if (segment.Kind == SegmentKind.Dot)
        {
            return 0;
        }

        if (segment.Width_mm > 0)
        {
            return segment.Width_mm;
        }

        return Floor(minimum);
    }

    /// <summary>
    /// Width written to a program. Zero means the glue is off.
    /// A glue-on dot has no bead, so it exports the project minimum and stays distinct from zero.
    /// </summary>
    public static double ExportWidth(PathSegment segment, double minimum)
    {
        if (!segment.GlueOn)
        {
            return 0;
        }

        if (segment.Kind == SegmentKind.Dot)
        {
            return Floor(minimum);
        }

        return Effective(segment, minimum);
    }

    public static float ScreenPixels(double widthMm, double mmPerPx, float zoom)
    {
        if (widthMm <= 0)
        {
            return 1f;
        }

        double scale = mmPerPx > 1e-12 ? mmPerPx : ProcessDefaults.DefaultReferenceMmPerPx;
        double px = widthMm / scale * Math.Max(zoom, 0f);
        if (!double.IsFinite(px) || px < 1)
        {
            return 1f;
        }

        return (float)Math.Min(px, 10000);
    }

    public static List<double> StoredList(IEnumerable<double>? steps, double minimum)
    {
        double floor = Floor(minimum);
        var values = new List<double>();
        if (steps != null)
        {
            foreach (double step in steps)
            {
                if (step >= floor - 0.0005)
                {
                    TryInclude(values, Math.Max(step, floor));
                }
            }
        }

        if (!values.Any(value => value > floor + 0.0005))
        {
            foreach (double step in ProcessDefaults.DefaultWidthsMm)
            {
                if (step >= floor - 0.0005)
                {
                    TryInclude(values, step);
                }
            }
        }

        TryInclude(values, floor);
        values.Sort();
        return values;
    }

    public static List<double> DisplayList(
        IEnumerable<double>? steps,
        double selected,
        double minimum,
        out double current)
    {
        double floor = Floor(minimum);
        List<double> values = StoredList(steps, floor);
        double chosen = Normalize(selected);
        if (chosen < floor - 0.0005)
        {
            chosen = floor;
        }

        TryInclude(values, Math.Max(chosen, floor));
        values.Sort();
        current = values.First(value => NearlyEqual(value, Math.Max(chosen, floor)));
        return values;
    }

    public static void EnsureStored(ProcessDefaults defaults)
    {
        defaults.MinWidth_mm = Floor(defaults.MinWidth_mm);
        if (defaults.ReferenceMmPerPx < ReferenceMin ||
            defaults.ReferenceMmPerPx > ReferenceMax ||
            !double.IsFinite(defaults.ReferenceMmPerPx))
        {
            defaults.ReferenceMmPerPx = ProcessDefaults.DefaultReferenceMmPerPx;
        }

        defaults.Widths_mm = StoredList(defaults.Widths_mm, defaults.MinWidth_mm);
        ZOffsets.EnsureStored(defaults);
    }

    public static void AssignMissing(PathDocument document)
    {
        EnsureStored(document.Defaults);
        double minimum = document.Defaults.MinWidth_mm;
        foreach (PathSegment segment in document.Segments)
        {
            if (segment.Kind is not (SegmentKind.Line or SegmentKind.Arc))
            {
                continue;
            }

            if (!segment.GlueOn)
            {
                segment.Width_mm = 0;
                continue;
            }

            if (segment.Width_mm <= 0)
            {
                segment.Width_mm = minimum;
            }
        }
    }

    public static bool SameSettings(ProcessDefaults left, ProcessDefaults right) =>
        left.Z_mm.Equals(right.Z_mm) &&
        left.W_deg.Equals(right.W_deg) &&
        left.P_deg.Equals(right.P_deg) &&
        left.R_deg.Equals(right.R_deg) &&
        left.GlueOn == right.GlueOn &&
        NearlyEqual(left.MinWidth_mm, right.MinWidth_mm) &&
        Math.Abs(left.ReferenceMmPerPx - right.ReferenceMmPerPx) < 1e-9 &&
        SameList(left.Widths_mm, right.Widths_mm) &&
        ZOffsets.SameList(left.ZDeltas_mm, right.ZDeltas_mm);

    private static bool TryInclude(List<double> values, double width)
    {
        if (width < MinMm || width > MaxMm)
        {
            return false;
        }

        double normalized = Normalize(width);
        if (values.Any(existing => NearlyEqual(existing, normalized)))
        {
            return false;
        }

        values.Add(normalized);
        return true;
    }

    private static bool SameList(IReadOnlyList<double> left, IReadOnlyList<double> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (int i = 0; i < left.Count; i++)
        {
            if (!NearlyEqual(left[i], right[i]))
            {
                return false;
            }
        }

        return true;
    }
}
