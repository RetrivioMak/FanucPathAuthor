using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

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

public sealed class GeometryImportResult
{
    public required IReadOnlyList<PathSegment> Segments { get; init; }

    public required IReadOnlyList<string> Warnings { get; init; }
}

public interface IGeometryImporter
{
    string Id { get; }

    string MenuLabel { get; }

    string FileFilter { get; }

    string? UnavailableReason(WorkspaceKind workspace);

    GeometryImportSurvey Survey(string sourcePath);

    GeometryImportResult Import(string sourcePath, GeometryImportOptions options, bool glueOn, double speedMmPerS);
}

public static class ImporterRegistry
{
    private static readonly IGeometryImporter[] Registered = [new DxfImporter()];

    public static IReadOnlyList<IGeometryImporter> Importers => Registered;

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
