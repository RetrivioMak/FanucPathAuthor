using DispensePathAuthor.Models;
using DispensePathAuthor.Services;
using Xunit;

namespace DispensePathAuthor.Tests;

public class CvFrameMillimeterTests
{
    [Fact]
    public void Migrate_CvPixels_RoundTripTheSameFanucCoordinates()
    {
        RobotCalibrationFile calibration = Calibration();
        var frame = new FrameDefinition();
        var pixels = new PathDocument
        {
            SchemaVersion = 4,
            Workspace = WorkspaceKind.Cv,
            Frame = frame,
            FrameIsDefined = true
        };
        pixels.Segments.Add(new PathSegment
        {
            Kind = SegmentKind.Line,
            X1 = 80,
            Y1 = 40,
            X2 = 120,
            Y2 = 40
        });

        RobotWaypoint before = GeometryHelper.BuildWaypoints(pixels, calibration)[0];
        Assert.True(FrameMillimeters.Migrate(pixels, calibration));
        Assert.Equal(FrameMillimeters.SchemaVersion, pixels.SchemaVersion);
        Assert.Equal(0.8, pixels.Segments[0].X1, 6);
        Assert.Equal(0.4, pixels.Segments[0].Y1, 6);

        RobotWaypoint after = GeometryHelper.BuildWaypoints(pixels, calibration)[0];
        Assert.Equal(before.ExportX_mm, after.ExportX_mm, 6);
        Assert.Equal(before.ExportY_mm, after.ExportY_mm, 6);
        Assert.Equal(before.SensorX_px, after.SensorX_px, 6);
        Assert.Equal(before.SensorY_px, after.SensorY_px, 6);
        Assert.Equal(80, after.LocalX_px, 6);
        Assert.Equal(40, after.LocalY_px, 6);
    }

    [Fact]
    public void Migrate_Teach_KeepsMillimetersAndAdvancesSchema()
    {
        var document = new PathDocument
        {
            SchemaVersion = 4,
            Workspace = WorkspaceKind.Teach
        };
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Dot, X1 = 12.5, Y1 = 3 });

        Assert.True(FrameMillimeters.Migrate(document, calibration: null));

        Assert.Equal(FrameMillimeters.SchemaVersion, document.SchemaVersion);
        Assert.Equal(12.5, document.Segments[0].X1, 6);
        Assert.Equal(3, document.Segments[0].Y1, 6);
    }

    [Fact]
    public void Migrate_CvWithoutCalibration_LeavesPixelsInPlace()
    {
        var document = new PathDocument
        {
            SchemaVersion = 4,
            Workspace = WorkspaceKind.Cv
        };
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Dot, X1 = 15, Y1 = 4 });

        Assert.False(FrameMillimeters.Migrate(document, calibration: null));

        Assert.Equal(4, document.SchemaVersion);
        Assert.Equal(15, document.Segments[0].X1, 6);
    }

    [Fact]
    public void Migrate_EmptyCv_AdvancesSchemaWithoutCalibration()
    {
        var document = new PathDocument { SchemaVersion = 4, Workspace = WorkspaceKind.Cv };

        Assert.True(FrameMillimeters.Migrate(document, calibration: null));

        Assert.Equal(FrameMillimeters.SchemaVersion, document.SchemaVersion);
        Assert.Empty(document.Segments);
    }

    [Fact]
    public void SameDxf_IsStoredInMillimetersForCvAndTeach()
    {
        string fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "teach-path.dxf");
        var importer = new DxfImporter();
        GeometryImportResult result = importer.Import(
            fixture,
            new GeometryImportOptions
            {
                Layers = ["0", "Path"],
                Unit = DrawingLengthUnit.Millimeter,
                OriginX_mm = 0,
                OriginY_mm = 0
            },
            glueOn: true,
            speedMmPerS: 40);

        var cv = new PathDocument { Workspace = WorkspaceKind.Cv, SchemaVersion = FrameMillimeters.SchemaVersion };
        var teach = new PathDocument { Workspace = WorkspaceKind.Teach, SchemaVersion = FrameMillimeters.SchemaVersion };
        cv.Segments.AddRange(result.Segments.Select(DocumentClone.CloneSegment));
        teach.Segments.AddRange(result.Segments.Select(DocumentClone.CloneSegment));

        Assert.Equal(teach.Segments.Count, cv.Segments.Count);
        Assert.True(cv.Segments.Count > 0);
        for (int i = 0; i < cv.Segments.Count; i++)
        {
            Assert.Equal(teach.Segments[i].Kind, cv.Segments[i].Kind);
            Assert.Equal(teach.Segments[i].X1, cv.Segments[i].X1, 6);
            Assert.Equal(teach.Segments[i].Y1, cv.Segments[i].Y1, 6);
            Assert.Equal(teach.Segments[i].X2, cv.Segments[i].X2, 6);
            Assert.Equal(teach.Segments[i].Y2, cv.Segments[i].Y2, 6);
            Assert.Equal(teach.Segments[i].X3, cv.Segments[i].X3, 6);
            Assert.Equal(teach.Segments[i].Y3, cv.Segments[i].Y3, 6);
        }
    }

    private static RobotCalibrationFile Calibration() =>
        new()
        {
            ReferenceRobotX_mm = 100,
            ReferenceRobotY_mm = 200,
            ReferenceRobotR_deg = 0,
            ReferencePixelX = 0,
            ReferencePixelY = 0,
            PixelToRobot = new PixelToRobotCoeffs
            {
                A = 0.01,
                B = 0,
                Tx = 0,
                C = 0,
                D = 0.01,
                Ty = 0
            }
        };
}
