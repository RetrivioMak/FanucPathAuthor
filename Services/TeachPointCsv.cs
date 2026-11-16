using System.Globalization;
using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public sealed class TeachPointCsvRow
{
    public string? Name { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
}

public sealed class TeachPointCsvResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<TeachPointCsvRow> Rows { get; init; } = Array.Empty<TeachPointCsvRow>();

    public static TeachPointCsvResult Failed(string error) =>
        new() { Success = false, Error = error };

    public static TeachPointCsvResult Ok(IReadOnlyList<TeachPointCsvRow> rows) =>
        new() { Success = true, Rows = rows };
}

public static class TeachPointCsv
{
    public static TeachPointCsvResult Parse(string text)
    {
        string[] lines = text.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
        var content = new List<(string Raw, char Delimiter)>();
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            if (trimmed.StartsWith('\uFEFF'))
            {
                trimmed = trimmed.TrimStart('\uFEFF');
            }

            content.Add((trimmed, trimmed.Contains(';') ? ';' : ','));
        }

        if (content.Count == 0)
        {
            return TeachPointCsvResult.Failed("The CSV file has no point rows.");
        }

        char delimiter = content[0].Delimiter;
        string[] header = Split(content[0].Raw, delimiter);
        bool hasHeader = header.Any(cell => !TryParseNumber(cell, delimiter, out _));
        int nameIndex = -1;
        int xIndex = 0;
        int yIndex = 1;
        int firstData = 0;
        if (hasHeader)
        {
            firstData = 1;
            nameIndex = FindColumn(header, "name");
            xIndex = FindColumn(header, "x");
            yIndex = FindColumn(header, "y");
            if (xIndex < 0 || yIndex < 0)
            {
                return TeachPointCsvResult.Failed("The CSV header needs X and Y columns.");
            }
        }

        var rows = new List<TeachPointCsvRow>();
        for (int i = firstData; i < content.Count; i++)
        {
            (string raw, char rowDelimiter) = content[i];
            string[] cells = Split(raw, rowDelimiter);
            if (!hasHeader)
            {
                if (cells.Length >= 3 && !TryParseNumber(cells[0], rowDelimiter, out _))
                {
                    nameIndex = 0;
                    xIndex = 1;
                    yIndex = 2;
                }
                else
                {
                    nameIndex = -1;
                    xIndex = 0;
                    yIndex = 1;
                }
            }

            if (cells.Length <= Math.Max(xIndex, yIndex))
            {
                return TeachPointCsvResult.Failed($"Row {i + 1} does not have X and Y values.");
            }

            if (!TryParseNumber(cells[xIndex], rowDelimiter, out double x) ||
                !TryParseNumber(cells[yIndex], rowDelimiter, out double y))
            {
                return TeachPointCsvResult.Failed($"Row {i + 1} has a value that is not a number.");
            }

            string? name = nameIndex >= 0 && nameIndex < cells.Length
                ? cells[nameIndex].Trim().Trim('"')
                : null;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = null;
            }

            rows.Add(new TeachPointCsvRow { Name = name, X = x, Y = y });
        }

        if (rows.Count == 0)
        {
            return TeachPointCsvResult.Failed("The CSV file has no point rows.");
        }

        return TeachPointCsvResult.Ok(rows);
    }

    public static string NextName(ISet<string> usedNames)
    {
        int index = 1;
        while (true)
        {
            string name = $"P{index}";
            if (usedNames.Add(name))
            {
                return name;
            }

            index++;
        }
    }

    public static IReadOnlyList<ReferencePoint> ToReferencePoints(
        IReadOnlyList<TeachPointCsvRow> rows,
        LengthUnit unit,
        IEnumerable<string> existingNames)
    {
        var used = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
        var points = new List<ReferencePoint>(rows.Count);
        foreach (TeachPointCsvRow row in rows)
        {
            string name = string.IsNullOrWhiteSpace(row.Name) ? NextName(used) : row.Name.Trim();
            used.Add(name);
            points.Add(new ReferencePoint
            {
                Name = name,
                X_mm = LengthUnits.ToMillimeters(row.X, unit),
                Y_mm = LengthUnits.ToMillimeters(row.Y, unit)
            });
        }

        return points;
    }

    private static int FindColumn(IReadOnlyList<string> header, string name)
    {
        for (int i = 0; i < header.Count; i++)
        {
            if (string.Equals(header[i].Trim().Trim('"'), name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static string[] Split(string line, char delimiter) =>
        line.Split(delimiter);

    private static bool TryParseNumber(string text, char delimiter, out double value)
    {
        string trimmed = text.Trim().Trim('"');
        if (delimiter == ';')
        {
            trimmed = trimmed.Replace(',', '.');
        }

        return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
