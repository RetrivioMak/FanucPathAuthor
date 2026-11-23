namespace DispensePathAuthor.Contracts;

public enum ImportedSegmentKind
{
    Dot,
    Line,
    Arc
}

public sealed class ImportedSegment
{
    public required ImportedSegmentKind Kind { get; init; }

    public double X1 { get; init; }

    public double Y1 { get; init; }

    public double X2 { get; init; }

    public double Y2 { get; init; }

    public double X3 { get; init; }

    public double Y3 { get; init; }
}

public sealed class GeometryImportResult
{
    public required IReadOnlyList<ImportedSegment> Segments { get; init; }

    public required IReadOnlyList<string> Warnings { get; init; }
}

public interface IGeometryImporter
{
    string Id { get; }

    string MenuLabel { get; }

    /// <summary>
    /// Shows the plugin's import dialog. Null when the user cancels.
    /// </summary>
    GeometryImportResult? Prompt(IWin32Window owner);
}

public interface IImporterCatalog
{
    void Add(IGeometryImporter importer);
}

public interface IImportPluginEntry
{
    void Register(IImporterCatalog catalog);
}
