using DispensePathAuthor.Models;
using DispensePathAuthor.Services;
using Xunit;

namespace DispensePathAuthor.Tests;

public class DxfImportTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"dpath-dxf-{Guid.NewGuid():N}");

    public DxfImportTests() => Directory.CreateDirectory(_directory);

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private static string Fixture => Path.Combine(AppContext.BaseDirectory, "Fixtures", "teach-path.dxf");

    [Fact]
    public void PositiveBulge_RunsCounterclockwise()
    {
        Assert.True(DxfCurve.TryBulgeMidpoint(0, 0, 10, 0, 1, out double x, out double y));
        Assert.Equal(5, x, 6);
        Assert.Equal(-5, y, 6);
    }

    [Fact]
    public void NegativeBulge_RunsClockwise()
    {
        Assert.True(DxfCurve.TryBulgeMidpoint(0, 0, 10, 0, -1, out double x, out double y));
        Assert.Equal(5, x, 6);
        Assert.Equal(5, y, 6);
    }

    [Fact]
    public void QuarterArc_KeepsTheMidpointOnTheArc()
    {
        PathSegment arc = DxfCurve.Arc(0, 0, 10, 0, 90, glueOn: true, speedMmPerS: 40)!;
        Assert.Equal(SegmentKind.Arc, arc.Kind);
        Assert.Equal(10, arc.X1, 6);
        Assert.Equal(0, arc.Y1, 6);
        Assert.Equal(5 * Math.Sqrt(2), arc.X2, 6);
        Assert.Equal(5 * Math.Sqrt(2), arc.Y2, 6);
        Assert.Equal(0, arc.X3, 6);
        Assert.Equal(10, arc.Y3, 6);
    }

    [Fact]
    public void Import_LinesAndArcs_BecomeTeachSegmentsInPackage()
    {
        var importer = new DxfImporter();
        Assert.Null(importer.UnavailableReason(WorkspaceKind.Teach));
        Assert.Contains("millimeters", importer.UnavailableReason(WorkspaceKind.Cv), StringComparison.Ordinal);

        GeometryImportSurvey survey = importer.Survey(Fixture);
        Assert.Equal(["0", "Notes", "Path", "SkipMe"], survey.Layers);
        Assert.Equal(DrawingLengthUnit.Millimeter, survey.SuggestedUnit);
        Assert.Null(survey.Note);

        GeometryImportResult result = importer.Import(
            Fixture,
            new GeometryImportOptions
            {
                Layers = survey.Layers.Where(name => !string.Equals(name, "SkipMe", StringComparison.OrdinalIgnoreCase)).ToArray(),
                Unit = DrawingLengthUnit.Millimeter,
                OriginX_mm = 0,
                OriginY_mm = 0
            },
            glueOn: true,
            speedMmPerS: 40);

        Assert.Equal(8, result.Segments.Count);
        Assert.All(result.Segments, segment => Assert.Equal(40, segment.Speed_mm_s));

        PathSegment line = result.Segments[0];
        Assert.Equal(SegmentKind.Line, line.Kind);
        Assert.Equal(0, line.X1, 6);
        Assert.Equal(0, line.Y1, 6);
        Assert.Equal(10, line.X2, 6);
        Assert.Equal(0, line.Y2, 6);

        PathSegment arc = result.Segments[1];
        Assert.Equal(SegmentKind.Arc, arc.Kind);
        Assert.Equal(10, arc.X1, 6);
        Assert.Equal(0, arc.Y1, 6);
        Assert.Equal(5 * Math.Sqrt(2), arc.X2, 6);
        Assert.Equal(5 * Math.Sqrt(2), arc.Y2, 6);
        Assert.Equal(0, arc.X3, 6);
        Assert.Equal(10, arc.Y3, 6);

        PathSegment upper = result.Segments[2];
        Assert.Equal(7, upper.X1, 6);
        Assert.Equal(5, upper.Y1, 6);
        Assert.Equal(5, upper.X2, 6);
        Assert.Equal(7, upper.Y2, 6);
        Assert.Equal(3, upper.X3, 6);
        Assert.Equal(5, upper.Y3, 6);

        PathSegment lower = result.Segments[3];
        Assert.Equal(3, lower.X1, 6);
        Assert.Equal(5, lower.Y1, 6);
        Assert.Equal(5, lower.X2, 6);
        Assert.Equal(3, lower.Y2, 6);
        Assert.Equal(7, lower.X3, 6);
        Assert.Equal(5, lower.Y3, 6);

        Assert.Equal(SegmentKind.Dot, result.Segments[4].Kind);
        Assert.Equal(1, result.Segments[4].X1, 6);
        Assert.Equal(2, result.Segments[4].Y1, 6);

        Assert.Equal(SegmentKind.Line, result.Segments[5].Kind);
        Assert.Equal(0, result.Segments[5].X1, 6);
        Assert.Equal(10, result.Segments[5].X2, 6);

        PathSegment bulge = result.Segments[6];
        Assert.Equal(SegmentKind.Arc, bulge.Kind);
        Assert.Equal(10, bulge.X1, 6);
        Assert.Equal(0, bulge.Y1, 6);
        Assert.Equal(15, bulge.X2, 6);
        Assert.Equal(5, bulge.Y2, 6);
        Assert.Equal(10, bulge.X3, 6);
        Assert.Equal(10, bulge.Y3, 6);

        Assert.Equal(SegmentKind.Line, result.Segments[7].Kind);
        Assert.Equal(10, result.Segments[7].X1, 6);
        Assert.Equal(10, result.Segments[7].Y1, 6);
        Assert.Equal(0, result.Segments[7].X2, 6);
        Assert.Equal(0, result.Segments[7].Y2, 6);

        Assert.Contains(result.Warnings, warning => warning.Contains("text", StringComparison.Ordinal));
        Assert.Contains(result.Warnings, warning => warning.Contains("dimension", StringComparison.Ordinal));
        Assert.Contains(result.Warnings, warning => warning.Contains("hatch", StringComparison.Ordinal));
        Assert.Contains(result.Warnings, warning => warning.Contains("out-of-plane", StringComparison.Ordinal));
        Assert.Contains(result.Warnings, warning => warning.Contains("paper-space", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Segments, segment => Math.Abs(segment.X1 - 100) < 1e-6);

        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Teach,
            FrameIsDefined = true
        };
        document.Segments.AddRange(result.Segments);
        string packagePath = Path.Combine(_directory, "imported.dpath");
        ProjectPackage.Save(document, packagePath, null);

        PathDocument loaded = ProjectPackage.Load(packagePath).Document;
        Assert.Equal(WorkspaceKind.Teach, loaded.Workspace);
        Assert.Equal(8, loaded.Segments.Count);
        Assert.Equal(SegmentKind.Arc, loaded.Segments[1].Kind);
        Assert.Equal(10, loaded.Segments[1].Y3, 6);
        Assert.Equal(15, loaded.Segments[6].X2, 6);
        Assert.Equal(5, loaded.Segments[6].Y2, 6);
    }

    [Fact]
    public void Import_UnitOverride_ScalesFromTheDrawingZero()
    {
        var importer = new DxfImporter();
        GeometryImportResult result = importer.Import(
            Fixture,
            new GeometryImportOptions
            {
                Layers = ["0"],
                Unit = DrawingLengthUnit.Inch,
                OriginX_mm = 1,
                OriginY_mm = 2
            },
            glueOn: true,
            speedMmPerS: 50);

        PathSegment line = result.Segments[0];
        Assert.Equal(1, line.X1, 6);
        Assert.Equal(2, line.Y1, 6);
        Assert.Equal(10 * 25.4 + 1, line.X2, 6);
        Assert.Equal(2, line.Y2, 6);
    }
}
