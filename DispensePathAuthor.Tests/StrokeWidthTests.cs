using DispensePathAuthor.Models;
using DispensePathAuthor.Services;
using Xunit;

namespace DispensePathAuthor.Tests;

public class StrokeWidthTests
{
    [Fact]
    public void DisplayList_KeepsMinimumAndTheSessionWidth()
    {
        List<double> shown = StrokeWidths.DisplayList(
            new[] { 0.05, 0.2, 0.2, 1, 5 },
            0.4,
            0.1,
            out double current);

        Assert.Equal(0.4, current);
        Assert.Equal(new[] { 0.1, 0.2, 0.4, 1, 5 }, shown);
    }

    [Fact]
    public void AssignMissing_FillsOnlyBlankStrokes()
    {
        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Cv,
            Defaults = new ProcessDefaults { MinWidth_mm = 0.1 }
        };
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Line, X1 = 0, Y1 = 0, X2 = 1, Y2 = 0 });
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Arc, Width_mm = 0.05, X1 = 0, Y1 = 0, X2 = 1, Y2 = 1, X3 = 2, Y3 = 0 });
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Dot, X1 = 1, Y1 = 1 });

        StrokeWidths.AssignMissing(document);

        Assert.Equal(0.1, document.Segments[0].Width_mm);
        Assert.Equal(0.05, document.Segments[1].Width_mm);
        Assert.Equal(0, document.Segments[2].Width_mm);
        Assert.Contains(0.1, document.Defaults.Widths_mm);
    }

    [Fact]
    public void ExportWidth_IsZeroWhenGlueIsOff()
    {
        var off = new PathSegment { Kind = SegmentKind.Line, GlueOn = false, Width_mm = 0.4 };
        var on = new PathSegment { Kind = SegmentKind.Line, GlueOn = true, Width_mm = 0.4 };
        var dot = new PathSegment { Kind = SegmentKind.Dot, GlueOn = true };

        Assert.Equal(0, StrokeWidths.ExportWidth(off, 0.1));
        Assert.Equal(0.4, StrokeWidths.ExportWidth(on, 0.1));
        Assert.Equal(0.1, StrokeWidths.ExportWidth(dot, 0.1));
    }

    [Fact]
    public void ScreenPixels_FollowZoomAndClampToOnePixel()
    {
        Assert.Equal(10f, StrokeWidths.ScreenPixels(1, 0.1, 1f));
        Assert.Equal(1f, StrokeWidths.ScreenPixels(1, 0.1, 0.05f));
        Assert.Equal(6f, StrokeWidths.ScreenPixels(2, 1, 3f));
    }

    [Fact]
    public void ResolveImageScale_UsesCalibrationThenReference()
    {
        var frame = new FrameDefinition();
        var calibration = new RobotCalibrationFile
        {
            PixelToRobot = new PixelToRobotCoeffs { A = 0.02, D = 0.02 }
        };

        FrameAxisScale calibrated = FrameMillimeters.ResolveImageScale(frame, calibration, 0.1);
        FrameAxisScale reference = FrameMillimeters.ResolveImageScale(frame, null, 0.1);

        Assert.Equal(0.02, calibrated.X_mmPerPx, 6);
        Assert.Equal(0.02, calibrated.Y_mmPerPx, 6);
        Assert.Equal(0.1, reference.X_mmPerPx, 6);
        Assert.Equal(0.1, reference.Y_mmPerPx, 6);
    }
}
