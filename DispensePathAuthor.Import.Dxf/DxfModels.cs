namespace DispensePathAuthor.Import.Dxf;

public enum DrawingLengthUnit
{
    Micrometer,
    Millimeter,
    Centimeter,
    Meter,
    Inch,
    Foot
}

public sealed class GeometryImportSurvey
{
    public required IReadOnlyList<string> Layers { get; init; }

    public required DrawingLengthUnit SuggestedUnit { get; init; }

    public string? Note { get; init; }
}

public sealed class GeometryImportOptions
{
    public required IReadOnlyCollection<string> Layers { get; init; }

    public required DrawingLengthUnit Unit { get; init; }

    public double OriginX_mm { get; init; }

    public double OriginY_mm { get; init; }
}

internal static class DrawingScale
{
    public static double ToMillimeters(DrawingLengthUnit unit) => unit switch
    {
        DrawingLengthUnit.Micrometer => 0.001,
        DrawingLengthUnit.Millimeter => 1,
        DrawingLengthUnit.Centimeter => 10,
        DrawingLengthUnit.Meter => 1000,
        DrawingLengthUnit.Inch => 25.4,
        DrawingLengthUnit.Foot => 304.8,
        _ => throw new ArgumentOutOfRangeException(nameof(unit))
    };
}
