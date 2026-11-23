using DispensePathAuthor.Models;
using DispensePathAuthor.Services;
using Xunit;

namespace DispensePathAuthor.Tests;

public class ZOffsetTests
{
    [Fact]
    public void StoredList_KeepsZeroAndThePresets()
    {
        List<double> stored = ZOffsets.StoredList(null);

        Assert.Contains(0, stored);
        Assert.Contains(-0.5, stored);
        Assert.Contains(1, stored);
        Assert.Equal(stored.OrderBy(value => value), stored);
    }

    [Fact]
    public void Format_ShowsTheSignAndParsesBack()
    {
        Assert.Equal("0", ZOffsets.Format(0));
        Assert.StartsWith("+", ZOffsets.Format(1));
        Assert.StartsWith("-", ZOffsets.Format(-0.5));
        Assert.True(ZOffsets.TryParse(ZOffsets.Format(1), out double raised));
        Assert.True(ZOffsets.TryParse(ZOffsets.Format(-0.5), out double lowered));
        Assert.Equal(1, raised);
        Assert.Equal(-0.5, lowered);
    }

    [Fact]
    public void Export_AddsTheSegmentOffsetToProjectZ()
    {
        var defaults = new ProcessDefaults { Z_mm = 10 };
        var plain = new PathSegment { ZDelta_mm = 0 };
        var raised = new PathSegment { ZDelta_mm = 1 };
        var lowered = new PathSegment { ZDelta_mm = -0.5 };

        Assert.Equal(10, ZOffsets.ExportZ(defaults, plain));
        Assert.Equal(11, ZOffsets.ExportZ(defaults, raised));
        Assert.Equal(9.5, ZOffsets.ExportZ(defaults, lowered));
    }

    [Fact]
    public void Trajectory_WritesProjectZPlusTheSegmentOffset()
    {
        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Teach,
            Defaults = new ProcessDefaults { Z_mm = 8 }
        };
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Dot, X1 = 1, Y1 = 2, ZDelta_mm = 0 });
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Line, X1 = 0, Y1 = 0, X2 = 4, Y2 = 0, ZDelta_mm = -0.5 });

        FrameTrajectoryResult result = FrameTrajectory.Build(document, calibration: null);

        Assert.Equal(8, result.Moves[0].Z_mm);
        Assert.Equal(7.5, result.Moves[1].Z_mm);
        Assert.Equal(7.5, GeometryHelper.BuildWaypoints(document, calibration: null)[1].Z_mm);
    }

    [Fact]
    public void Apply_ZOffset_ChangesEveryKind()
    {
        var dot = new PathSegment { Kind = SegmentKind.Dot, X1 = 1, Y1 = 2 };
        SegmentProperties.Apply(dot, SegmentPropertyField.ZDelta, -0.5, false);

        Assert.Equal(-0.5, dot.ZDelta_mm);
        Assert.Equal(1, dot.X1);
        Assert.Equal(2, dot.Y1);
    }
}
