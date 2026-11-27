using System.Text.Json;
using DispensePathAuthor.Contracts;
using DispensePathAuthor.Models;
using DispensePathAuthor.Services;
using DispensePathAuthor.Export.Xyz;
using Xunit;

namespace DispensePathAuthor.Tests;

public class XyzExportTests
{
    [Fact]
    public void Project_KeepsTheXyzSectionAndThePath()
    {
        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Teach,
            SchemaVersion = FrameMillimeters.SchemaVersion
        };
        document.Segments.Add(new PathSegment
        {
            Kind = SegmentKind.Line,
            X1 = 1,
            Y1 = 2,
            X2 = 8,
            Y2 = 4
        });
        document.Exporters.SetSection(
            "xyz",
            JsonSerializer.Deserialize<JsonElement>("""{"z_mm":1.25,"valveOutput":4,"chordTolerance_mm":0.2,"pointSpacing_mm":0.5}"""));

        PathDocument loaded = PathSerializer.LoadFromJson(PathSerializer.ToJson(document));

        Assert.Equal(WorkspaceKind.Teach, loaded.Workspace);
        Assert.Equal(1, loaded.Segments[0].X1, 6);
        Assert.Equal(8, loaded.Segments[0].X2, 6);
        Assert.Equal(1.25, loaded.Exporters.Section("xyz").GetProperty("z_mm").GetDouble(), 6);
        Assert.Equal(4, loaded.Exporters.Section("xyz").GetProperty("valveOutput").GetInt32());
        Assert.Equal(0.5, loaded.Exporters.Section("xyz").GetProperty("pointSpacing_mm").GetDouble(), 6);
        Assert.Equal(ExportCoordinateMode.OffsetFromCalibrationRef, loaded.Exporters.FanucLs.CoordinateMode);
    }

    [Fact]
    public void Export_WritesConstantZAndSplitsTheArc()
    {
        JsonElement settings = JsonSerializer.Deserialize<JsonElement>("""{"z_mm":1.5,"valveOutput":3,"chordTolerance_mm":0.5,"pointSpacing_mm":10}""");
        FrameMove[] moves =
        [
            new()
            {
                Kind = FrameMoveKind.Line,
                X1 = 0,
                Y1 = 0,
                X2 = 10,
                Y2 = 0,
                Width_mm = 0.5,
                Z_mm = 1.5
            },
            new()
            {
                Kind = FrameMoveKind.Arc,
                X1 = 10,
                Y1 = 0,
                X2 = 0,
                Y2 = -10,
                X3 = -10,
                Y3 = 0,
                Width_mm = 0.5,
                Z_mm = 1.5
            }
        ];
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xyz");
        try
        {
            var exporter = new XyzExporter();
            Assert.Contains("Z", exporter.RequiredChannels);
            exporter.Export(new ProgramExportRequest
            {
                Moves = moves,
                Settings = settings,
                DestinationPath = path
            });

            string[] lines = File.ReadAllLines(path);
            string[] motion = lines.Where(line => line.StartsWith("MOVE ", StringComparison.Ordinal)).ToArray();
            Assert.Contains("MOVE X0.000 Y0.000 Z1.500 W0.500 V3", motion);
            Assert.Contains("MOVE X10.000 Y0.000 Z1.500 W0.500 V3", motion);
            Assert.Equal("MOVE X-10.000 Y0.000 Z1.500 W0.500 V3", motion[^1]);
            Assert.True(motion.Length > 6);
            Assert.Contains(motion, line => line.Contains("Y-9.", StringComparison.Ordinal) || line.Contains("Y-10.", StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Export_PointSpacingShortensTheArcStep()
    {
        FrameMove[] moves =
        [
            new()
            {
                Kind = FrameMoveKind.Arc,
                X1 = 15,
                Y1 = -5,
                X2 = 10,
                Y2 = 0,
                X3 = 5,
                Y3 = -5,
                Width_mm = 0.1,
                Z_mm = 0
            }
        ];

        int coarse = CountMoves("""{"chordTolerance_mm":0.2,"pointSpacing_mm":2}""", moves);
        int fine = CountMoves("""{"chordTolerance_mm":0.2,"pointSpacing_mm":0.5}""", moves);

        Assert.InRange(coarse, 8, 12);
        Assert.True(fine > 30);
    }

    private static int CountMoves(string settingsJson, FrameMove[] moves)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xyz");
        try
        {
            new XyzExporter().Export(new ProgramExportRequest
            {
                Moves = moves,
                Settings = JsonSerializer.Deserialize<JsonElement>(settingsJson),
                DestinationPath = path
            });
            return File.ReadAllLines(path).Count(line => line.StartsWith("MOVE ", StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void PixelCvWithoutCalibration_DoesNotExport()
    {
        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Cv,
            SchemaVersion = 4
        };
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Dot, X1 = 10, Y1 = 4 });
        FrameTrajectoryResult trajectory = FrameTrajectory.Build(document, calibration: null);

        string? error = new XyzExporter().Validate(new ProgramExportRequest
        {
            Moves = trajectory.Moves,
            Settings = default,
            DestinationPath = "path.xyz",
            TrajectoryError = trajectory.Error
        });

        Assert.NotNull(error);
        Assert.Contains("calibration", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Loader_FindsThePluginAssembly()
    {
        string source = typeof(XyzPlugin).Assembly.Location;
        string folder = Path.Combine(Path.GetTempPath(), "dpath-plugins-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            File.Copy(source, Path.Combine(folder, Path.GetFileName(source)));
            var catalog = new ListCatalog();
            IReadOnlyList<string> notes = PluginLoader.Load(folder, catalog);

            Assert.Empty(notes);
            Assert.Contains(catalog.Exporters, exporter => exporter.Id == XyzExporter.ExporterId);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private sealed class ListCatalog : IExporterCatalog
    {
        public List<IProgramExporter> Exporters { get; } = [];

        public void Add(IProgramExporter exporter) => Exporters.Add(exporter);
    }
}
