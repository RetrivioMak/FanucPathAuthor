using System.Text.Json;

namespace DispensePathAuthor.Contracts;

public enum FrameMoveKind
{
    Dot,
    Line,
    Arc
}

public sealed class FrameMove
{
    public required FrameMoveKind Kind { get; init; }

    public double X1 { get; init; }

    public double Y1 { get; init; }

    public double X2 { get; init; }

    public double Y2 { get; init; }

    public double X3 { get; init; }

    public double Y3 { get; init; }

    public bool Dispense { get; init; }

    public double SpeedMmPerS { get; init; }
}

public sealed class ProgramExportRequest
{
    public required IReadOnlyList<FrameMove> Moves { get; init; }

    public required JsonElement Settings { get; init; }

    public required string DestinationPath { get; init; }

    public string? TrajectoryError { get; init; }

    public object? Host { get; init; }
}

public interface IExporterSettingsPage
{
    void LoadSection(JsonElement section);

    JsonElement SaveSection();
}

public interface IProgramExporter
{
    string Id { get; }

    string MenuLabel { get; }

    string SettingsTitle { get; }

    string FileFilter { get; }

    string DefaultFileName { get; }

    IReadOnlyList<string> RequiredChannels { get; }

    UserControl? CreateSettingsPage();

    string? Validate(ProgramExportRequest request);

    void Export(ProgramExportRequest request);
}

public interface IExporterCatalog
{
    void Add(IProgramExporter exporter);
}

public interface IPluginEntry
{
    void Register(IExporterCatalog catalog);
}
