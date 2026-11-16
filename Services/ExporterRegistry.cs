using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public sealed class ProgramExportRequest
{
    public required PathDocument Document { get; init; }

    public required RobotCalibrationFile? Calibration { get; init; }

    public required string DestinationPath { get; init; }
}

public interface IProgramExporter
{
    string Id { get; }

    string MenuLabel { get; }

    string FileFilter { get; }

    string DefaultFileName { get; }

    string? Validate(PathDocument document, RobotCalibrationFile? calibration);

    void Export(ProgramExportRequest request);
}

public static class ExporterRegistry
{
    private static readonly IProgramExporter[] Registered = [new FanucLsExporter()];

    public static IReadOnlyList<IProgramExporter> Exporters => Registered;
}
