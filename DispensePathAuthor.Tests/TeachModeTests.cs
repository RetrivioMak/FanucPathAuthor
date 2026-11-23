using System.Drawing;
using System.Text.Json;
using DispensePathAuthor.Contracts;
using DispensePathAuthor.Controls;
using DispensePathAuthor.Export.Fanuc;
using DispensePathAuthor.Models;
using DispensePathAuthor.Services;
using Xunit;

namespace DispensePathAuthor.Tests;

public class TeachModeTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"fanuc-teach-{Guid.NewGuid():N}");

    public TeachModeTests() => Directory.CreateDirectory(_directory);

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Load_MissingWorkspace_OpensAsCv()
    {
        string path = Write("""
            {
              "schemaVersion": 3,
              "segments": []
            }
            """);

        PathDocument document = PathSerializer.Load(path);

        Assert.Equal(WorkspaceKind.Cv, document.Workspace);
        Assert.Equal(4, document.SchemaVersion);
    }

    [Fact]
    public void Load_TeachWorkspace_RoundTripsPointsAndGrid()
    {
        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Teach,
            FrameIsDefined = true
        };
        document.ReferencePoints.Add(new ReferencePoint { Name = "Left", X_mm = 12.5, Y_mm = 4 });
        document.Grid.OriginX_mm = 12.5;
        document.Grid.OriginY_mm = 4;
        document.Grid.IsAligned = true;
        document.Grid.Step_mm = 0.2;
        document.Grid.Steps_mm = new List<double> { 0.2, 1, 2.5 };
        string path = Path.Combine(_directory, "teach.json");
        PathSerializer.Save(document, path);

        PathDocument loaded = PathSerializer.Load(path);

        Assert.Equal(WorkspaceKind.Teach, loaded.Workspace);
        Assert.Equal("Left", loaded.ReferencePoints[0].Name);
        Assert.Equal(12.5, loaded.ReferencePoints[0].X_mm, 3);
        Assert.True(loaded.Grid.IsAligned);
        Assert.Equal(0.2, loaded.Grid.Step_mm, 3);
        Assert.Equal(new[] { 0.2, 1, 2.5 }, loaded.Grid.Steps_mm);
    }

    [Fact]
    public void Csv_ReadsNameXY_AndIgnoresExtraColumn()
    {
        TeachPointCsvResult result = TeachPointCsv.Parse("""
            name,x,y,z
            LeftHole,12.5,4.0,9
            ,40,4.2,1
            """);

        Assert.True(result.Success);
        IReadOnlyList<ReferencePoint> points = TeachPointCsv.ToReferencePoints(
            result.Rows,
            LengthUnit.Millimeter,
            existingNames: Array.Empty<string>());

        Assert.Equal("LeftHole", points[0].Name);
        Assert.Equal(12.5, points[0].X_mm, 3);
        Assert.Equal("P1", points[1].Name);
        Assert.Equal(40, points[1].X_mm, 3);
    }

    [Fact]
    public void Csv_SemicolonAndMeters_ConvertToMillimeters()
    {
        TeachPointCsvResult result = TeachPointCsv.Parse("""
            name;x;y
            Hole;0,0125;0,004
            """);

        Assert.True(result.Success);
        IReadOnlyList<ReferencePoint> points = TeachPointCsv.ToReferencePoints(
            result.Rows,
            LengthUnit.Meter,
            existingNames: ["P1"]);

        Assert.Equal("Hole", points[0].Name);
        Assert.Equal(12.5, points[0].X_mm, 3);
        Assert.Equal(4, points[0].Y_mm, 3);
    }

    [Fact]
    public void Grid_IsAvailableForCvAndTeach()
    {
        Assert.True(new CvCanvasSpace().HasGrid);
        Assert.True(new TeachCanvasSpace().HasGrid);

        var document = new PathDocument();
        ProjectSession.Start(document, WorkspaceKind.Cv);

        Assert.True(document.Grid.Enabled);
        Assert.Equal(0.2, document.Grid.Step_mm, 3);
        Assert.NotEmpty(document.Grid.Steps_mm);
    }

    [Fact]
    public void GridSteps_NormalizesSortsAndKeepsSelection()
    {
        List<double> steps = GridSteps.NormalizeList(new[] { 2.0, 0.2, 0.2, 0.0001, 5000 }, 0.35, out double current);

        Assert.Equal(new[] { 0.2, 0.35, 2 }, steps);
        Assert.Equal(0.35, current, 3);
        Assert.True(GridSteps.TryParse("0.35", out double parsed));
        Assert.Equal(0.35, parsed, 3);
    }

    [Fact]
    public void GridSnap_UsesOriginAndStep()
    {
        var grid = new TeachGridSettings
        {
            Enabled = true,
            Step_mm = 0.2,
            OriginX_mm = 10,
            OriginY_mm = 4,
            IsAligned = true
        };

        (double x, double y) = TeachGridSnap.Snap(10.31, 4.09, grid);

        Assert.Equal(10.4, x, 6);
        Assert.Equal(4.0, y, 6);
    }

    [Fact]
    public void GridSnap_WhenHidden_StillSnaps()
    {
        var grid = new TeachGridSettings { Enabled = false, Step_mm = 1, OriginX_mm = 0, OriginY_mm = 0 };

        (double x, double y) = TeachGridSnap.Snap(1.6, -0.4, grid);

        Assert.Equal(2, x, 6);
        Assert.Equal(0, y, 6);
    }

    [Fact]
    public void Pointer_PrefersNearbyPointOverGrid()
    {
        var grid = new TeachGridSettings { Enabled = true, Step_mm = 1, OriginX_mm = 0, OriginY_mm = 0 };
        (double x, double y, bool snappedToPoint) = TeachPointer.Resolve(
            0.4,
            0.1,
            new[] { (0d, 0d) },
            grid,
            static (px, py) => new PointF((float)px, (float)py),
            pickRadiusScreenPx: 1,
            snapEnabled: true);

        Assert.True(snappedToPoint);
        Assert.Equal(0, x, 6);
        Assert.Equal(0, y, 6);

        (double gridX, double gridY, bool gridPoint) = TeachPointer.Resolve(
            2.2,
            0.1,
            new[] { (0d, 0d) },
            grid,
            static (px, py) => new PointF((float)px, (float)py),
            pickRadiusScreenPx: 1,
            snapEnabled: true);

        Assert.False(gridPoint);
        Assert.Equal(2, gridX, 6);
        Assert.Equal(0, gridY, 6);
    }

    [Fact]
    public void Pointer_HiddenGrid_KeepsCursor_AndStillSnapsToPoint()
    {
        var grid = new TeachGridSettings { Enabled = false, Step_mm = 1, OriginX_mm = 0, OriginY_mm = 0 };
        Func<double, double, PointF> toScreen = static (px, py) => new PointF((float)px, (float)py);

        (double x, double y, bool snappedToPoint) = TeachPointer.Resolve(
            1.4,
            0.2,
            new[] { (0d, 0d) },
            grid,
            toScreen,
            pickRadiusScreenPx: 1,
            snapEnabled: true);

        Assert.False(snappedToPoint);
        Assert.Equal(1.4, x, 6);
        Assert.Equal(0.2, y, 6);

        (double pointX, double pointY, bool onPoint) = TeachPointer.Resolve(
            0.4,
            0.1,
            new[] { (0d, 0d) },
            grid,
            toScreen,
            pickRadiusScreenPx: 1,
            snapEnabled: true);

        Assert.True(onPoint);
        Assert.Equal(0, pointX, 6);
        Assert.Equal(0, pointY, 6);
    }

    [Fact]
    public void Pointer_SnapOff_KeepsCursor()
    {
        var grid = new TeachGridSettings { Enabled = true, Step_mm = 1, OriginX_mm = 0, OriginY_mm = 0 };

        (double x, double y, bool snappedToPoint) = TeachPointer.Resolve(
            1.4,
            0.2,
            new[] { (0d, 0d) },
            grid,
            static (px, py) => new PointF((float)px, (float)py),
            pickRadiusScreenPx: 8,
            snapEnabled: false);

        Assert.False(snappedToPoint);
        Assert.Equal(1.4, x, 6);
        Assert.Equal(0.2, y, 6);
    }

    [Fact]
    public void TeachExport_WritesAbsoluteMillimeters_AndIgnoresCameraCorrections()
    {
        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Teach,
            FrameIsDefined = true,
            Defaults = new ProcessDefaults
            {
                R_deg = 12
            },
            Exporters = new ExporterSettings
            {
                FanucLs = new FanucLsSettings
                {
                    CoordinateMode = ExportCoordinateMode.OffsetFromCalibrationRef,
                    UseFovCenterNeedleOffset = true,
                    LocalRotation = ExportLocalRotation.Degree180,
                    UseNeedleOffset = true,
                    NeedleOffsetX_mm = 1,
                    NeedleOffsetY_mm = -2,
                    NeedleOffsetR_deg = 3
                }
            }
        };
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Dot, X1 = 10, Y1 = 4, GlueOn = true });

        RobotWaypoint waypoint = Assert.Single(GeometryHelper.BuildWaypoints(document, calibration: null));

        Assert.Equal(11, waypoint.ExportX_mm, 3);
        Assert.Equal(2, waypoint.ExportY_mm, 3);
        Assert.Equal(15, waypoint.ExportR_deg, 3);
    }

    [Fact]
    public void TeachLsExport_DoesNotReflectY()
    {
        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Teach,
            Defaults = new ProcessDefaults { R_deg = 0 }
        };
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Dot, X1 = 0, Y1 = 0 });
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Dot, X1 = 10, Y1 = 0 });
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Dot, X1 = 0, Y1 = 10 });

        string path = Path.Combine(_directory, "GLUE.LS");
        FanucExporter.ExportLs(
            GeometryHelper.BuildWaypoints(document, calibration: null).Select(static waypoint => new FanucExportPoint
            {
                Index = waypoint.Index,
                LocalX_px = waypoint.LocalX_px,
                LocalY_px = waypoint.LocalY_px,
                ExportX_mm = waypoint.ExportX_mm,
                ExportY_mm = waypoint.ExportY_mm,
                ExportR_deg = waypoint.ExportR_deg,
                Z_mm = waypoint.Z_mm,
                IsPreviewOnly = waypoint.IsPreviewOnly
            }).ToList(),
            "GLUE",
            path,
            applyFrameYReflection: false);

        string ls = File.ReadAllText(path);
        Assert.Contains("UFRAME_NUM=1", ls, StringComparison.Ordinal);
        Assert.Contains("Y = 10 mm", ls, StringComparison.Ordinal);
        Assert.DoesNotContain("Y = -10", ls, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkspaceGuard_BlocksTheOtherProjectType()
    {
        var document = new PathDocument();
        ProjectSession session = ProjectSession.Start(document, WorkspaceKind.Cv);

        Assert.Equal(WorkspaceKind.Cv, session.Kind);
        Assert.False(WorkspaceGuard.CanAdopt(document.Workspace, WorkspaceKind.Teach));
        Assert.Throws<InvalidOperationException>(() => ProjectSession.Start(document, WorkspaceKind.Teach));
        Assert.Equal(WorkspaceKind.Cv, document.Workspace);
    }

    [Fact]
    public void Session_StartTeach_InitializesFrameAndRejectsCv()
    {
        var document = new PathDocument();
        ProjectSession session = ProjectSession.Start(document, WorkspaceKind.Teach);
        document.Grid.Step_mm = 1;
        document.Grid.IsAligned = true;

        ProjectSession again = ProjectSession.Start(document, WorkspaceKind.Teach);

        Assert.Equal(WorkspaceKind.Teach, session.Kind);
        Assert.True(document.FrameIsDefined);
        Assert.True(document.Grid.Enabled);
        Assert.Equal(1, again.Document.Grid.Step_mm, 3);
        Assert.True(again.Document.Grid.IsAligned);
        Assert.Throws<InvalidOperationException>(() => ProjectSession.Start(document, WorkspaceKind.Cv));
    }

    [Fact]
    public void Session_RejectsUnsetAndBindsLoadedKind()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectSession.Start(new PathDocument(), WorkspaceKind.Unset));
        Assert.False(WorkspaceGuard.CanAdopt(WorkspaceKind.Unset, WorkspaceKind.Cv));

        var loaded = new PathDocument { Workspace = WorkspaceKind.Teach };
        ProjectSession session = ProjectSession.Bind(loaded);
        session.FilePath = "teach.json";
        session.CalibrationPath = "cal.json";

        Assert.Equal(WorkspaceKind.Teach, session.Kind);
        Assert.Equal("teach.json", session.FilePath);
        Assert.Equal("cal.json", session.CalibrationPath);
        session.Require(WorkspaceKind.Teach);
        Assert.Throws<InvalidOperationException>(() => session.Require(WorkspaceKind.Cv));
    }

    [Fact]
    public void Load_LegacyFanucDefaults_MoveIntoFanucSection()
    {
        PathDocument document = PathSerializer.LoadFromJson("""
            {
              "schemaVersion": 4,
              "workspace": "cv",
              "defaults": {
                "z_mm": 8,
                "coordinateMode": "uframeAbsolute",
                "localRotation": "none",
                "useNeedleOffset": true,
                "needleOffsetX_mm": 1.5
              }
            }
            """);

        Assert.Equal(8, document.Defaults.Z_mm);
        Assert.Equal(ExportCoordinateMode.UframeAbsolute, document.Exporters.FanucLs.CoordinateMode);
        Assert.Equal(ExportLocalRotation.None, document.Exporters.FanucLs.LocalRotation);
        Assert.True(document.Exporters.FanucLs.UseNeedleOffset);
        Assert.Equal(1.5, document.Exporters.FanucLs.NeedleOffsetX_mm, 3);

        using JsonDocument saved = JsonDocument.Parse(PathSerializer.ToJson(document));
        Assert.False(saved.RootElement.GetProperty("defaults").TryGetProperty("coordinateMode", out _));
        Assert.Equal(
            "uframeAbsolute",
            saved.RootElement.GetProperty("exporters").GetProperty("fanuc-ls").GetProperty("coordinateMode").GetString());
    }

    private string Write(string json)
    {
        string path = Path.Combine(_directory, $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }
}
