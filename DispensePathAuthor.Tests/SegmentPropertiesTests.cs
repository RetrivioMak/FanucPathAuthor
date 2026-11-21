using DispensePathAuthor.Models;
using DispensePathAuthor.Services;
using Xunit;

namespace DispensePathAuthor.Tests;

public class SegmentPropertiesTests
{
    [Theory]
    [InlineData(SegmentKind.Dot, "Dot", 1)]
    [InlineData(SegmentKind.Line, "Line", 2)]
    [InlineData(SegmentKind.Arc, "Arc", 3)]
    public void Kind_DefinesFixedTypeAndPointCount(SegmentKind kind, string name, int points)
    {
        Assert.Equal(name, SegmentProperties.TypeName(kind));
        Assert.Equal(points, SegmentProperties.PointsOf(kind).Count);
    }

    [Fact]
    public void Points_FollowKind()
    {
        Assert.Equal(
            new[] { "Point" },
            SegmentProperties.PointsOf(SegmentKind.Dot).Select(point => point.Name).ToArray());
        Assert.Equal(
            new[] { "Start", "End" },
            SegmentProperties.PointsOf(SegmentKind.Line).Select(point => point.Name).ToArray());
        Assert.Equal(
            new[] { "Start", "Mid", "End" },
            SegmentProperties.PointsOf(SegmentKind.Arc).Select(point => point.Name).ToArray());
    }

    [Fact]
    public void Apply_ChangesOnlyTheEditedCoordinate()
    {
        var segment = new PathSegment
        {
            Kind = SegmentKind.Line,
            X1 = 1,
            Y1 = 2,
            X2 = 3,
            Y2 = 4,
            GlueOn = true,
            Speed_mm_s = 50
        };

        Assert.True(SegmentProperties.WouldChange(segment, SegmentPropertyField.X2, 8, false));
        SegmentProperties.Apply(segment, SegmentPropertyField.X2, 8, false);

        Assert.Equal(SegmentKind.Line, segment.Kind);
        Assert.Equal(1, segment.X1);
        Assert.Equal(2, segment.Y1);
        Assert.Equal(8, segment.X2);
        Assert.Equal(4, segment.Y2);
        Assert.True(segment.GlueOn);
        Assert.Equal(50, segment.Speed_mm_s);
    }

    [Fact]
    public void Apply_GlueAndSpeed_LeaveGeometryAndType()
    {
        var segment = new PathSegment
        {
            Kind = SegmentKind.Dot,
            X1 = 1.5,
            Y1 = -2,
            GlueOn = true,
            Speed_mm_s = 10
        };

        SegmentProperties.Apply(segment, SegmentPropertyField.GlueOn, 0, false);
        SegmentProperties.Apply(segment, SegmentPropertyField.Speed, 25, false);

        Assert.Equal(SegmentKind.Dot, segment.Kind);
        Assert.False(segment.GlueOn);
        Assert.Equal(25, segment.Speed_mm_s);
        Assert.Equal(1.5, segment.X1);
        Assert.Equal(-2, segment.Y1);
    }

    [Fact]
    public void WouldChange_IsFalse_WhenValueAlreadyMatches()
    {
        var segment = new PathSegment
        {
            Kind = SegmentKind.Arc,
            GlueOn = true,
            X1 = 1,
            Y1 = 2,
            X2 = 3,
            Y2 = 4,
            X3 = 5,
            Y3 = 6
        };

        Assert.False(SegmentProperties.WouldChange(segment, SegmentPropertyField.Y3, 6, false));
        Assert.False(SegmentProperties.WouldChange(segment, SegmentPropertyField.GlueOn, 0, true));
    }

    [Fact]
    public void Apply_Width_ChangesLineAndLeavesDot()
    {
        var line = new PathSegment { Kind = SegmentKind.Line, Width_mm = 0.2, X1 = 1, Y1 = 1, X2 = 2, Y2 = 2 };
        Assert.True(SegmentProperties.WouldChange(line, SegmentPropertyField.Width, 0.5, false));
        SegmentProperties.Apply(line, SegmentPropertyField.Width, 0.5, false);
        Assert.Equal(0.5, line.Width_mm);
        Assert.Equal(1, line.X1);

        var dot = new PathSegment { Kind = SegmentKind.Dot, X1 = 3, Y1 = 4 };
        Assert.False(SegmentProperties.WouldChange(dot, SegmentPropertyField.Width, 1, false));
        SegmentProperties.Apply(dot, SegmentPropertyField.Width, 1, false);
        Assert.Equal(0, dot.Width_mm);
        Assert.Equal(3, dot.X1);
    }
}
