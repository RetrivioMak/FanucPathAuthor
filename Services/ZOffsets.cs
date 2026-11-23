using System.Globalization;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class ZOffsets
{
    public const double MinMm = -10_000;
    public const double MaxMm = 10_000;

    public static string Format(double value)
    {
        double normalized = Normalize(value);
        if (NearlyEqual(normalized, 0))
        {
            return "0";
        }

        string magnitude = Math.Abs(normalized).ToString("0.###", CultureInfo.CurrentCulture);
        return normalized > 0 ? "+" + magnitude : "-" + magnitude;
    }

    public static double Normalize(double value) =>
        Math.Round(value, 3, MidpointRounding.AwayFromZero);

    public static bool NearlyEqual(double left, double right) => Math.Abs(left - right) < 0.0005;

    public static bool TryParse(string? text, out double value)
    {
        if (!string.IsNullOrWhiteSpace(text) &&
            (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
             double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) &&
            value >= MinMm - 0.0005 &&
            value <= MaxMm + 0.0005)
        {
            value = Math.Clamp(Normalize(value), MinMm, MaxMm);
            return true;
        }

        value = 0;
        return false;
    }

    public static double ExportZ(ProcessDefaults defaults, PathSegment segment) =>
        defaults.Z_mm + segment.ZDelta_mm;

    public static List<double> StoredList(IEnumerable<double>? steps)
    {
        var values = new List<double>();
        if (steps != null)
        {
            foreach (double step in steps)
            {
                TryInclude(values, step);
            }
        }

        if (values.Count == 0)
        {
            foreach (double step in ProcessDefaults.DefaultZDeltasMm)
            {
                TryInclude(values, step);
            }
        }

        TryInclude(values, 0);
        values.Sort();
        return values;
    }

    public static List<double> DisplayList(IEnumerable<double>? steps, double selected, out double current)
    {
        List<double> values = StoredList(steps);
        double chosen = Normalize(selected);
        if (chosen < MinMm || chosen > MaxMm || !double.IsFinite(chosen))
        {
            chosen = 0;
        }

        TryInclude(values, chosen);
        values.Sort();
        current = values.First(value => NearlyEqual(value, chosen));
        return values;
    }

    public static void EnsureStored(ProcessDefaults defaults)
    {
        defaults.ZDeltas_mm = StoredList(defaults.ZDeltas_mm);
    }

    public static bool SameList(IReadOnlyList<double> left, IReadOnlyList<double> right)
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

    private static bool TryInclude(List<double> values, double offset)
    {
        if (!double.IsFinite(offset) || offset < MinMm - 0.0005 || offset > MaxMm + 0.0005)
        {
            return false;
        }

        double normalized = Math.Clamp(Normalize(offset), MinMm, MaxMm);
        if (values.Any(existing => NearlyEqual(existing, normalized)))
        {
            return false;
        }

        values.Add(normalized);
        return true;
    }
}
