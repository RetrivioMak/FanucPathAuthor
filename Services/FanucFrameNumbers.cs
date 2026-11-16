using System.Globalization;

namespace DispensePathAuthor.Services;

public static class FanucFrameNumbers
{
    public static int Parse(string? label, int defaultNumber)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return defaultNumber;
        }

        int open = label.IndexOf('[', StringComparison.Ordinal);
        int close = label.IndexOf(']', StringComparison.Ordinal);
        if (open >= 0 && close > open &&
            int.TryParse(label.AsSpan(open + 1, close - open - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
        {
            return number;
        }

        return defaultNumber;
    }
}
