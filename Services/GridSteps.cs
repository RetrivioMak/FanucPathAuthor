using System.Globalization;

namespace DispensePathAuthor.Services;

public static class GridSteps
{
    public const double MinMm = 0.001;
    public const double MaxMm = 1000;

    public static string Format(double value) =>
        Normalize(value).ToString("0.###", CultureInfo.CurrentCulture);

    public static double Normalize(double value) =>
        Math.Round(value, 3, MidpointRounding.AwayFromZero);

    public static bool NearlyEqual(double left, double right) => Math.Abs(left - right) < 0.0005;

    public static bool TryParse(string? text, out double value)
    {
        if (!string.IsNullOrWhiteSpace(text) &&
            (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
             double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) &&
            value >= MinMm &&
            value <= MaxMm)
        {
            value = Normalize(value);
            return true;
        }

        value = 0;
        return false;
    }

    public static bool TryInclude(List<double> values, double step)
    {
        if (step < MinMm || step > MaxMm)
        {
            return false;
        }

        double normalized = Normalize(step);
        if (values.Any(existing => NearlyEqual(existing, normalized)))
        {
            return false;
        }

        values.Add(normalized);
        return true;
    }

    public static List<double> NormalizeList(IEnumerable<double>? steps, double selected, out double current)
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
            foreach (double step in Models.TeachGridSettings.DefaultStepsMm)
            {
                TryInclude(values, step);
            }
        }

        TryInclude(values, selected);
        values.Sort();
        double chosen = Normalize(selected);
        if (!values.Any(value => NearlyEqual(value, chosen)))
        {
            chosen = values[0];
        }

        current = chosen;
        return values;
    }
}
