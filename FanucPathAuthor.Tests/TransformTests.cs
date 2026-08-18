using FanucPathAuthor.Models;
using FanucPathAuthor.Services;
using Xunit;

namespace FanucPathAuthor.Tests;

public class CoordinateTransformTests
{
    [Fact]
    public void LocalToSensor_AndBack_RoundTrips()
    {
        var frame = new FrameDefinition { OriginX_px = 1000, OriginY_px = 800, AngleDeg = 15 };
        var (localX, localY) = (42.0, -17.0);

        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(localX, localY, frame);
        var (backX, backY) = CoordinateTransform.SensorToLocal(sensorX, sensorY, frame);

        Assert.Equal(localX, backX, 3);
        Assert.Equal(localY, backY, 3);
    }

    [Fact]
    public void PixelToRobot_UsesAffineFromCalibration()
    {
        var coeffs = new PixelToRobotCoeffs
        {
            A = 0.0001,
            B = -0.0157,
            Tx = -141.4,
            C = -0.0157,
            D = -0.0001,
            Ty = -328.9
        };

        (double x, double y) = coeffs.Transform(1229, 1017);
        Assert.True(x < -140);
        Assert.True(y < -320);
    }
}

public class ShapeExpanderTests
{
    [Fact]
    public void RectangleToLines_CreatesFourSegments()
    {
        IReadOnlyList<PathSegment> lines = ShapeExpander.RectangleToLines(0, 0, 100, 50, glueOn: true, speed_mm_s: 40);
        Assert.Equal(4, lines.Count);
        Assert.All(lines, line => Assert.Equal(SegmentKind.Line, line.Kind));
    }

    [Fact]
    public void RectangleToLines_CornersAreClosedLoop()
    {
        IReadOnlyList<PathSegment> lines = ShapeExpander.RectangleToLines(10, 20, 110, 70, glueOn: true, speed_mm_s: 40);
        Assert.Equal(lines[0].X1, lines[3].X2, 3);
        Assert.Equal(lines[0].Y1, lines[3].Y2, 3);
        Assert.Equal(lines[0].X2, lines[1].X1, 3);
        Assert.Equal(lines[0].Y2, lines[1].Y1, 3);
    }

    [Fact]
    public void CircleToArcs_CreatesTwoSemicircleSegments()
    {
        IReadOnlyList<PathSegment> arcs = ShapeExpander.CircleToArcs(50, 50, 100, 50, glueOn: true, speed_mm_s: 40);
        Assert.Equal(2, arcs.Count);
        Assert.All(arcs, arc => Assert.Equal(SegmentKind.Arc, arc.Kind));
    }

    [Fact]
    public void CircleToArcs_FormsClosedLoop()
    {
        IReadOnlyList<PathSegment> arcs = ShapeExpander.CircleToArcs(0, 0, 50, 0, glueOn: true, speed_mm_s: 40);
        Assert.Equal(arcs[0].X1, arcs[1].X3, 3);
        Assert.Equal(arcs[0].Y1, arcs[1].Y3, 3);
        Assert.Equal(arcs[0].X3, arcs[1].X1, 3);
        Assert.Equal(arcs[0].Y3, arcs[1].Y1, 3);
    }
}

public class PointEditHelperTests
{
    [Fact]
    public void TryParseCirclePair_DetectsCircleFromTwoArcs()
    {
        IReadOnlyList<PathSegment> arcs = ShapeExpander.CircleToArcs(50, 50, 100, 50, glueOn: true, speed_mm_s: 40);
        Assert.True(PointEditHelper.TryParseCirclePair(arcs[0], arcs[1], out double cx, out double cy, out double radius));
        Assert.Equal(50, cx, 1);
        Assert.Equal(50, cy, 1);
        Assert.Equal(50, radius, 1);
    }

    [Fact]
    public void TryPickVertex_FindsSharedRectangleCorner()
    {
        var document = new PathDocument();
        document.Segments.AddRange(ShapeExpander.RectangleToLines(0, 0, 100, 50, glueOn: true, speed_mm_s: 40));
        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(0, 0, document.Frame);

        Assert.True(PointEditHelper.TryPickVertex(
            document,
            sensorX,
            sensorY,
            static (x, y) => new PointF((float)x, (float)y),
            20,
            out VertexPick? pick));

        Assert.NotNull(pick);
        Assert.Equal(VertexDragMode.SharedPoint, pick!.Mode);
        Assert.Equal(2, pick.Bindings.Count);
    }

    [Fact]
    public void ResizeCircle_ChangesRadius()
    {
        IReadOnlyList<PathSegment> arcs = ShapeExpander.CircleToArcs(0, 0, 50, 0, glueOn: true, speed_mm_s: 40);
        var document = new PathDocument();
        document.Segments.AddRange(arcs);

        var pick = new VertexPick
        {
            Mode = VertexDragMode.CircleRadius,
            CircleArcIndexA = 0,
            CircleArcIndexB = 1,
            CircleCenterX = 0,
            CircleCenterY = 0,
            CircleRadius = 50
        };

        PointEditHelper.ApplyDrag(document, pick, 0, 80);

        Assert.True(PointEditHelper.TryParseCirclePair(document.Segments[0], document.Segments[1], out _, out _, out double radius));
        Assert.Equal(80, radius, 1);
    }
}

public class TileTransformTests
{
    private static RobotCalibrationFile CreateCalibration() =>
        new()
        {
            ReferenceRobotX_mm = 100,
            ReferenceRobotY_mm = 200,
            ReferenceRobotR_deg = 0,
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

    [Fact]
    public void TilePixelToWorld_SubtractsPoseDeltaFromReference()
    {
        var calibration = CreateCalibration();
        var tile = new FovTile { RobotX_mm = 130, RobotY_mm = 250 };

        (double worldX, double worldY) = TileTransformService.TilePixelToWorld(1000, 2000, tile, calibration);

        Assert.Equal(1000 * 0.01 - 30, worldX, 3);
        Assert.Equal(2000 * 0.01 - 50, worldY, 3);
    }

    [Fact]
    public void TilePixelToAnchorCanvas_AnchorTileAtReference_IsIdentity()
    {
        var calibration = CreateCalibration();
        var anchor = new FovTile { RobotX_mm = 100, RobotY_mm = 200, IsAnchor = true };

        (double canvasX, double canvasY) = TileTransformService.TilePixelToAnchorCanvas(
            500,
            700,
            anchor,
            anchor,
            calibration);

        Assert.Equal(500, canvasX, 3);
        Assert.Equal(700, canvasY, 3);
    }

    [Fact]
    public void GetTileCanvasOffset_MatchesShiftedTileOrigin()
    {
        var calibration = CreateCalibration();
        var anchor = new FovTile { RobotX_mm = 100, RobotY_mm = 200, IsAnchor = true };
        var tile = new FovTile { RobotX_mm = 150, RobotY_mm = 200 };

        (double offsetX, double offsetY) = TileTransformService.GetTileCanvasOffset(tile, anchor, calibration);

        Assert.Equal(-500, offsetX, 1);
        Assert.Equal(0, offsetY, 1);
    }

    [Fact]
    public void WorldToTilePixel_RoundTripsThroughTilePixelToWorld()
    {
        var calibration = CreateCalibration();
        var tile = new FovTile { RobotX_mm = 120, RobotY_mm = 180 };

        (double worldX, double worldY) = TileTransformService.TilePixelToWorld(900, 1100, tile, calibration);
        (double pixelX, double pixelY) = TileTransformService.WorldToTilePixel(worldX, worldY, tile, calibration);

        Assert.Equal(900, pixelX, 2);
        Assert.Equal(1100, pixelY, 2);
    }
}

public class TileDocumentHelperTests
{
    [Fact]
    public void EnsureLegacyMigrated_CreatesAnchorFromImagePath()
    {
        var document = new PathDocument { ImagePath = "C:\\shots\\anchor.png" };

        TileDocumentHelper.EnsureLegacyMigrated(document);

        Assert.Single(document.Tiles);
        Assert.True(document.Tiles[0].IsAnchor);
        Assert.Contains("anchor.png", document.Tiles[0].ImagePath!, StringComparison.OrdinalIgnoreCase);
    }
}

public class WaypointExportServiceTests
{
    private static RobotCalibrationFile CreateCalibration() =>
        new()
        {
            FrameWidth = 2448,
            FrameHeight = 2048,
            ReferencePixelX = 1000,
            ReferencePixelY = 800,
            ReferenceRobotX_mm = 100,
            ReferenceRobotY_mm = 200,
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

    private static FrameDefinition CreateFrameAtReferencePixel(RobotCalibrationFile calibration) =>
        new()
        {
            OriginX_px = calibration.ReferencePixelX,
            OriginY_px = calibration.ReferencePixelY
        };

    private static ProcessDefaults AbsoluteDefaults => new()
    {
        CoordinateMode = ExportCoordinateMode.UframeAbsolute,
        LocalRotation = ExportLocalRotation.None
    };

    [Fact]
    public void UframeAbsolute_UsesCalibrationReferenceRobotPlusPixelDelta()
    {
        var calibration = CreateCalibration();
        var frame = CreateFrameAtReferencePixel(calibration);
        var values = WaypointExportService.ComputeExportValues(
            324,
            324,
            frame,
            calibration,
            AbsoluteDefaults);

        Assert.Equal(103.24, values.ExportX_mm, 3);
        Assert.Equal(203.24, values.ExportY_mm, 3);
    }

    [Fact]
    public void OffsetFromCalibrationRef_AtReferencePixel_IsZero()
    {
        var calibration = CreateCalibration();
        var frame = CreateFrameAtReferencePixel(calibration);
        var defaults = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromCalibrationRef,
            LocalRotation = ExportLocalRotation.None
        };

        WaypointExportValues values = WaypointExportService.ComputeExportValues(
            0,
            0,
            frame,
            calibration,
            defaults);

        Assert.Equal(0, values.ExportX_mm, 3);
        Assert.Equal(0, values.ExportY_mm, 3);
    }

    [Fact]
    public void OffsetFromCalibrationRef_OffsetFromReferencePixel_MatchesPixelDelta()
    {
        var calibration = CreateCalibration();
        var frame = CreateFrameAtReferencePixel(calibration);
        var defaults = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromCalibrationRef,
            LocalRotation = ExportLocalRotation.None
        };

        WaypointExportValues values = WaypointExportService.ComputeExportValues(
            324,
            324,
            frame,
            calibration,
            defaults);

        Assert.Equal(3.24, values.ExportX_mm, 3);
        Assert.Equal(3.24, values.ExportY_mm, 3);
    }

    [Fact]
    public void FovCenterNeedleOffset_SubtractsFovCenterDeltaFromReferencePixel()
    {
        var calibration = CreateCalibration();
        var frame = CreateFrameAtReferencePixel(calibration);
        var defaults = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromCalibrationRef,
            UseFovCenterNeedleOffset = true,
            LocalRotation = ExportLocalRotation.None
        };

        WaypointExportValues values = WaypointExportService.ComputeExportValues(
            324,
            324,
            frame,
            calibration,
            defaults);

        Assert.Equal(1, values.ExportX_mm, 3);
        Assert.Equal(1, values.ExportY_mm, 3);
    }

    [Fact]
    public void FovCenterNeedleOffset_OnReferencePixelAbsolute_MovesToFovCenterPose()
    {
        var calibration = CreateCalibration();
        var frame = CreateFrameAtReferencePixel(calibration);
        var defaults = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.UframeAbsolute,
            UseFovCenterNeedleOffset = true,
            LocalRotation = ExportLocalRotation.None
        };

        WaypointExportValues values = WaypointExportService.ComputeExportValues(
            0,
            0,
            frame,
            calibration,
            defaults);

        Assert.Equal(97.76, values.ExportX_mm, 3);
        Assert.Equal(197.76, values.ExportY_mm, 3);
    }

    [Fact]
    public void OffsetFromFrameOrigin_AtFrameOrigin_IsZero()
    {
        var calibration = CreateCalibration();
        var frame = new FrameDefinition { OriginX_px = 500, OriginY_px = 600 };
        var defaults = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromFrameOrigin,
            LocalRotation = ExportLocalRotation.None
        };

        WaypointExportValues values = WaypointExportService.ComputeExportValues(
            0,
            0,
            frame,
            calibration,
            defaults);

        Assert.Equal(0, values.ExportX_mm, 3);
        Assert.Equal(0, values.ExportY_mm, 3);
    }

    [Fact]
    public void NeedleOffset_AddsToAllExportValues()
    {
        var calibration = CreateCalibration();
        var frame = CreateFrameAtReferencePixel(calibration);
        var defaults = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromCalibrationRef,
            UseNeedleOffset = true,
            NeedleOffsetX_mm = 5,
            NeedleOffsetY_mm = -2,
            LocalRotation = ExportLocalRotation.None
        };

        WaypointExportValues values = WaypointExportService.ComputeExportValues(
            324,
            324,
            frame,
            calibration,
            defaults);

        Assert.Equal(8.24, values.ExportX_mm, 3);
        Assert.Equal(1.24, values.ExportY_mm, 3);
    }

    [Fact]
    public void LocalRotation180_NegatesRobotDeltaComparedToNone()
    {
        var calibration = CreateCalibration();
        var frame = new FrameDefinition { OriginX_px = 1000, OriginY_px = 800 };
        var without = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromCalibrationRef,
            LocalRotation = ExportLocalRotation.None
        };
        var with = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromCalibrationRef,
            LocalRotation = ExportLocalRotation.Degree180
        };

        WaypointExportValues normal = WaypointExportService.ComputeExportValues(50, 20, frame, calibration, without);
        WaypointExportValues inverted = WaypointExportService.ComputeExportValues(50, 20, frame, calibration, with);

        Assert.Equal(-normal.ExportX_mm, inverted.ExportX_mm, 3);
        Assert.Equal(-normal.ExportY_mm, inverted.ExportY_mm, 3);
    }

    [Fact]
    public void LocalRotation90_RotatesRobotDeltaComparedToNone()
    {
        var calibration = CreateCalibration();
        var frame = new FrameDefinition { OriginX_px = 1000, OriginY_px = 800, AngleDeg = 0 };
        var none = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromCalibrationRef,
            LocalRotation = ExportLocalRotation.None
        };
        var rotated = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromCalibrationRef,
            LocalRotation = ExportLocalRotation.Degree90
        };

        WaypointExportValues baseline = WaypointExportService.ComputeExportValues(100, 0, frame, calibration, none);
        WaypointExportValues quarterTurn = WaypointExportService.ComputeExportValues(100, 0, frame, calibration, rotated);

        Assert.Equal(-baseline.ExportY_mm, quarterTurn.ExportX_mm, 1);
        Assert.Equal(baseline.ExportX_mm, quarterTurn.ExportY_mm, 1);
    }

    [Theory]
    [InlineData(ExportCoordinateMode.UframeAbsolute)]
    [InlineData(ExportCoordinateMode.OffsetFromFrameOrigin)]
    [InlineData(ExportCoordinateMode.OffsetFromCalibrationRef)]
    public void LocalRotation90_RotatesExportPlane_IndependentOfCoordinateMode(ExportCoordinateMode mode)
    {
        var calibration = CreateCalibration();
        var frame = new FrameDefinition { OriginX_px = 1000, OriginY_px = 800, AngleDeg = 15 };
        var none = new ProcessDefaults
        {
            CoordinateMode = mode,
            LocalRotation = ExportLocalRotation.None
        };
        var rotated = new ProcessDefaults
        {
            CoordinateMode = mode,
            LocalRotation = ExportLocalRotation.Degree90
        };

        WaypointExportValues baseline = WaypointExportService.ComputeExportValues(80, 30, frame, calibration, none);
        WaypointExportValues quarterTurn = WaypointExportService.ComputeExportValues(80, 30, frame, calibration, rotated);

        Assert.Equal(-baseline.ExportY_mm, quarterTurn.ExportX_mm, 1);
        Assert.Equal(baseline.ExportX_mm, quarterTurn.ExportY_mm, 1);
    }

    [Fact]
    public void ApplyExportPlaneRotation_FrameOrigin_StaysZero()
    {
        (double x, double y) = WaypointExportService.ApplyExportPlaneRotation(0, 0, ExportLocalRotation.Degree90);
        Assert.Equal(0, x, 3);
        Assert.Equal(0, y, 3);
    }

    [Fact]
    public void AnchorCapturePose_DoesNotAffectExport()
    {
        var calibration = CreateCalibration();
        var frame = CreateFrameAtReferencePixel(calibration);
        var defaults = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromCalibrationRef,
            LocalRotation = ExportLocalRotation.None
        };

        WaypointExportValues values = WaypointExportService.ComputeExportValues(
            324,
            324,
            frame,
            calibration,
            defaults);

        Assert.Equal(3.24, values.ExportX_mm, 3);
        Assert.Equal(3.24, values.ExportY_mm, 3);
    }
}

public class GeometryHelperTests
{
    private static RobotCalibrationFile CreateCalibration() =>
        new()
        {
            FrameWidth = 2448,
            FrameHeight = 2048,
            ReferencePixelX = 1000,
            ReferencePixelY = 800,
            ReferenceRobotX_mm = 100,
            ReferenceRobotY_mm = 200,
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

    [Fact]
    public void BuildPreviewWaypoints_IncludesFixedFrameRowAtIndexZero()
    {
        var document = new PathDocument
        {
            FrameIsDefined = true,
            Frame = new FrameDefinition { OriginX_px = 1000, OriginY_px = 800 },
            Defaults = new ProcessDefaults
            {
                CoordinateMode = ExportCoordinateMode.OffsetFromFrameOrigin,
                LocalRotation = ExportLocalRotation.None
            }
        };
        document.Segments.Add(new PathSegment
        {
            Kind = SegmentKind.Dot,
            X1 = 10,
            Y1 = 20,
            GlueOn = true
        });

        var calibration = CreateCalibration();
        IReadOnlyList<RobotWaypoint> preview = GeometryHelper.BuildPreviewWaypoints(document, calibration);
        IReadOnlyList<RobotWaypoint> export = GeometryHelper.BuildWaypoints(document, calibration);

        Assert.Equal(2, preview.Count);
        Assert.Equal(0, preview[0].Index);
        Assert.Equal("Frame", preview[0].Role);
        Assert.True(preview[0].IsPreviewOnly);
        Assert.Equal(0, preview[0].LocalX_px, 3);
        Assert.Equal(0, preview[0].LocalY_px, 3);
        Assert.Equal(0, preview[0].ExportX_mm, 3);
        Assert.Equal(0, preview[0].ExportY_mm, 3);
        Assert.Equal(1, preview[1].Index);

        Assert.Single(export);
        Assert.DoesNotContain(export, wp => wp.IsPreviewOnly);
    }

    [Fact]
    public void ExportCsv_ExcludesPreviewOnlyFrameRow()
    {
        var document = new PathDocument
        {
            FrameIsDefined = true,
            Frame = new FrameDefinition { OriginX_px = 1000, OriginY_px = 800 },
            Defaults = new ProcessDefaults { LocalRotation = ExportLocalRotation.None }
        };
        document.Segments.Add(new PathSegment
        {
            Kind = SegmentKind.Dot,
            X1 = 10,
            Y1 = 20,
            GlueOn = true
        });

        var calibration = CreateCalibration();
        IReadOnlyList<RobotWaypoint> preview = GeometryHelper.BuildPreviewWaypoints(document, calibration);
        string csvPath = Path.Combine(Path.GetTempPath(), $"fanuc-frame-row-{Guid.NewGuid():N}.csv");
        try
        {
            FanucExporter.ExportCsv(preview, csvPath);
            string csv = File.ReadAllText(csvPath);
            Assert.DoesNotContain(",Frame,", csv, StringComparison.Ordinal);
            Assert.Contains(",dot,", csv, StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(csvPath))
            {
                File.Delete(csvPath);
            }
        }
    }
}

public class ExportSettingsPersistenceTests : IDisposable
{
    private readonly string? _originalPath;
    private readonly string _tempDirectory;

    public ExportSettingsPersistenceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"fanuc-export-settings-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
        _originalPath = Environment.GetEnvironmentVariable("FANUC_PATH_AUTHOR_EXPORT_SETTINGS");
        Environment.SetEnvironmentVariable(
            "FANUC_PATH_AUTHOR_EXPORT_SETTINGS",
            Path.Combine(_tempDirectory, "export-settings.json"));
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("FANUC_PATH_AUTHOR_EXPORT_SETTINGS", _originalPath);
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void SaveAndLoad_RoundTripsExportPreferences()
    {
        var source = new ProcessDefaults
        {
            CoordinateMode = ExportCoordinateMode.OffsetFromFrameOrigin,
            UseFovCenterNeedleOffset = true,
            LocalRotation = ExportLocalRotation.None,
            UseNeedleOffset = true,
            NeedleOffsetX_mm = 1.25,
            NeedleOffsetY_mm = -2.5,
            NeedleOffsetR_deg = 3.0
        };

        ExportSettingsPersistence.Save(source);

        var loaded = new ProcessDefaults();
        Assert.True(ExportSettingsPersistence.TryLoadInto(loaded));
        Assert.Equal(source.CoordinateMode, loaded.CoordinateMode);
        Assert.Equal(source.UseFovCenterNeedleOffset, loaded.UseFovCenterNeedleOffset);
        Assert.Equal(source.LocalRotation, loaded.LocalRotation);
        Assert.Equal(source.UseNeedleOffset, loaded.UseNeedleOffset);
        Assert.Equal(source.NeedleOffsetX_mm, loaded.NeedleOffsetX_mm, 3);
        Assert.Equal(source.NeedleOffsetY_mm, loaded.NeedleOffsetY_mm, 3);
        Assert.Equal(source.NeedleOffsetR_deg, loaded.NeedleOffsetR_deg, 3);
    }

    [Fact]
    public void TryLoadInto_MigratesLegacyUseFixedCameraInversionFlag()
    {
        string path = ExportSettingsPersistence.SettingsFilePath;
        File.WriteAllText(
            path,
            """
            {
              "schemaVersion": 1,
              "coordinateMode": "offsetFromCalibrationRef",
              "useFixedCameraInversion": false,
              "useNeedleOffset": false
            }
            """);

        var loaded = new ProcessDefaults();
        Assert.True(ExportSettingsPersistence.TryLoadInto(loaded));
        Assert.Equal(ExportLocalRotation.None, loaded.LocalRotation);
    }
}

public class CalibrationLoaderTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    public void Dispose()
    {
        foreach (string path in _tempFiles)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void Load_DetectsCompactMarkerTransform()
    {
        string path = WriteTempJson(
            """
            {
              "schemaVersion": 1,
              "calibrationPoseId": "SIDE_LEFT",
              "frameWidth": 2448,
              "frameHeight": 2048,
              "robotFrame": "UF1",
              "robotTool": "UT1",
              "fixedRobotZ_mm": 12.5,
              "fixedRobotW_deg": 1.0,
              "fixedRobotP_deg": 2.0,
              "fixedRobotR_deg": 90.0,
              "referencePixelX": 1000,
              "referencePixelY": 800,
              "referenceRobotX_mm": 100,
              "referenceRobotY_mm": 200,
              "pixelToRobot": { "a": 0.01, "b": 0, "tx": 0, "c": 0, "d": 0.01, "ty": 0 }
            }
            """);

        RobotCalibrationFile calibration = CalibrationLoader.Load(path);

        Assert.Equal(CalibrationFileKind.CompactMarker, calibration.Kind);
        Assert.Equal("SIDE_LEFT", calibration.CalibrationPoseId);
        Assert.Equal(90.0, calibration.ReferenceRobotR_deg);
        Assert.Equal(12.5, calibration.FixedRobotZ_mm);
    }

    [Fact]
    public void Load_DetectsRobotCvTransform()
    {
        string path = WriteTempJson(
            """
            {
              "schemaVersion": 1,
              "crossModelFilePath": "cross.mdl",
              "frameWidth": 2448,
              "frameHeight": 2048,
              "robotFrame": "UF0",
              "robotTool": "UT0",
              "referencePixelX": 1224,
              "referencePixelY": 1024,
              "referenceRobotX_mm": 100,
              "referenceRobotY_mm": 200,
              "referenceRobotR_deg": 0,
              "pixelToRobot": { "a": 0.01, "b": 0, "tx": 0, "c": 0, "d": 0.01, "ty": 0 },
              "rotation": { "gain_degPerVisionDeg": 1.0 }
            }
            """);

        RobotCalibrationFile calibration = CalibrationLoader.Load(path);

        Assert.Equal(CalibrationFileKind.RobotCvTransform, calibration.Kind);
        Assert.Null(calibration.CalibrationPoseId);
    }

    [Fact]
    public void Load_RejectsCompactMarkerProfilesFile()
    {
        string path = WriteTempJson(
            """
            {
              "schemaVersion": 1,
              "profiles": [
                { "calibrationPoseId": "SIDE_LEFT" }
              ]
            }
            """);

        InvalidDataException ex = Assert.Throws<InvalidDataException>(() => CalibrationLoader.Load(path));
        Assert.Contains("CompactMarkerProfiles", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyCompactMarkerProcessDefaults_CopiesFixedPose()
    {
        var calibration = new RobotCalibrationFile
        {
            Kind = CalibrationFileKind.CompactMarker,
            FixedRobotZ_mm = 10,
            FixedRobotW_deg = 11,
            FixedRobotP_deg = 12,
            FixedRobotR_deg = 13
        };
        var defaults = new ProcessDefaults();

        CalibrationLoader.ApplyCompactMarkerProcessDefaults(calibration, defaults);

        Assert.Equal(10, defaults.Z_mm);
        Assert.Equal(11, defaults.W_deg);
        Assert.Equal(12, defaults.P_deg);
        Assert.Equal(13, defaults.R_deg);
    }

    [Fact]
    public void ResolveExportR_deg_UsesFixedR_ForCompactMarker()
    {
        var calibration = new RobotCalibrationFile
        {
            Kind = CalibrationFileKind.CompactMarker,
            FixedRobotR_deg = 90,
            ReferenceRobotR_deg = 5
        };

        Assert.Equal(90, WaypointExportService.ResolveExportR_deg(calibration));
    }

    [Fact]
    public void ResolveExportR_deg_UsesReferenceR_ForRobotCv()
    {
        var calibration = new RobotCalibrationFile
        {
            Kind = CalibrationFileKind.RobotCvTransform,
            ReferenceRobotR_deg = 42,
            FixedRobotR_deg = 90
        };

        Assert.Equal(42, WaypointExportService.ResolveExportR_deg(calibration));
    }

    private string WriteTempJson(string json)
    {
        string path = Path.Combine(Path.GetTempPath(), $"fanuc-calib-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        _tempFiles.Add(path);
        return path;
    }
}

public class FrameTransformHelperTests
{
    [Fact]
    public void RebindSegments_KeepsSensorPosition_WhenFrameOriginMoves()
    {
        var oldFrame = new FrameDefinition { OriginX_px = 100, OriginY_px = 200, AngleDeg = 0 };
        var newFrame = new FrameDefinition { OriginX_px = 150, OriginY_px = 200, AngleDeg = 0 };
        var segment = new PathSegment { Kind = SegmentKind.Dot, X1 = 10, Y1 = 5 };

        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(segment.X1, segment.Y1, oldFrame);
        FrameTransformHelper.RebindSegmentsToFrame(new[] { segment }, oldFrame, newFrame);
        var (newSensorX, newSensorY) = CoordinateTransform.LocalToSensor(segment.X1, segment.Y1, newFrame);

        Assert.Equal(sensorX, newSensorX, 3);
        Assert.Equal(sensorY, newSensorY, 3);
        Assert.Equal(-40, segment.X1, 3);
    }
}

public class PathSequenceServiceTests
{
    [Fact]
    public void AnalyzeGaps_ReportsZeroForConnectedLineChain()
    {
        var segments = new List<PathSegment>
        {
            new() { Kind = SegmentKind.Line, X1 = 0, Y1 = 0, X2 = 10, Y2 = 0 },
            new() { Kind = SegmentKind.Line, X1 = 10, Y1 = 0, X2 = 20, Y2 = 0 }
        };

        IReadOnlyList<SequenceGap> gaps = PathSequenceService.AnalyzeGaps(segments);

        Assert.Single(gaps);
        Assert.False(gaps[0].HasGap(PathSequenceService.DefaultGapThresholdPx));
    }

    [Fact]
    public void AnalyzeGaps_FlagsDiscontinuousSegments()
    {
        var segments = new List<PathSegment>
        {
            new() { Kind = SegmentKind.Line, X1 = 0, Y1 = 0, X2 = 10, Y2 = 0 },
            new() { Kind = SegmentKind.Line, X1 = 20, Y1 = 0, X2 = 30, Y2 = 0 }
        };

        IReadOnlyList<SequenceGap> gaps = PathSequenceService.AnalyzeGaps(segments);

        Assert.Single(gaps);
        Assert.True(gaps[0].HasGap(PathSequenceService.DefaultGapThresholdPx));
        Assert.Equal(10, gaps[0].Distance_px, 3);
    }

    [Fact]
    public void MoveSegment_SwapsAdjacentEntries()
    {
        var segments = new List<PathSegment>
        {
            new() { Kind = SegmentKind.Dot, X1 = 1, Y1 = 1 },
            new() { Kind = SegmentKind.Dot, X1 = 2, Y1 = 2 },
            new() { Kind = SegmentKind.Dot, X1 = 3, Y1 = 3 }
        };

        PathSequenceService.MoveSegment(segments, 1, 1);

        Assert.Equal(1, segments[0].X1);
        Assert.Equal(3, segments[1].X1);
        Assert.Equal(2, segments[2].X1);
    }
}

public class PathSimulationServiceTests
{
    [Fact]
    public void BuildTimeline_InsertsTravelMoveForGap()
    {
        var document = new PathDocument
        {
            Segments =
            {
                new PathSegment { Kind = SegmentKind.Line, X1 = 0, Y1 = 0, X2 = 10, Y2 = 0, GlueOn = true, Speed_mm_s = 50 },
                new PathSegment { Kind = SegmentKind.Line, X1 = 20, Y1 = 0, X2 = 30, Y2 = 0, GlueOn = true, Speed_mm_s = 50 }
            }
        };

        PathSimulationTimeline timeline = PathSimulationService.BuildTimeline(document);

        Assert.Contains(timeline.Moves, move => move.Kind == SimulationMoveKind.GapJump && !move.GlueOn);
    }

    [Fact]
    public void BuildTimeline_UsesSegmentGlueState()
    {
        var document = new PathDocument
        {
            Segments =
            {
                new PathSegment { Kind = SegmentKind.Line, X1 = 0, Y1 = 0, X2 = 10, Y2 = 0, GlueOn = false, Speed_mm_s = 40 }
            }
        };

        PathSimulationTimeline timeline = PathSimulationService.BuildTimeline(document);

        SimulationMove move = Assert.Single(timeline.Moves);
        Assert.Equal(SimulationMoveKind.SegmentPath, move.Kind);
        Assert.False(move.GlueOn);
    }

    [Fact]
    public void GetPoseAtTime_StartsAtFirstPoint()
    {
        var document = new PathDocument
        {
            Segments =
            {
                new PathSegment { Kind = SegmentKind.Line, X1 = 0, Y1 = 0, X2 = 10, Y2 = 0, GlueOn = true, Speed_mm_s = 10 }
            }
        };

        PathSimulationTimeline timeline = PathSimulationService.BuildTimeline(document);
        SimulationPose pose = timeline.GetPoseAtTime(0);

        Assert.Equal(0, pose.LocalX, 3);
        Assert.Equal(0, pose.LocalY, 3);
        Assert.True(pose.GlueOn);
    }
}
