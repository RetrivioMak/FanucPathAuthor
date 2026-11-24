using DispensePathAuthor.Contracts;
using DispensePathAuthor.Export.Fanuc;
using DispensePathAuthor.Export.FanucFeed;
using DispensePathAuthor.Models;
using DispensePathAuthor.Services;
using Xunit;

namespace DispensePathAuthor.Tests;

public sealed class FanucLsMotionTests
{
    [Fact]
    public void ExportLs_WritesLinearForDotAndLineAndCircularForArc()
    {
        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Teach,
            Defaults = new ProcessDefaults { Z_mm = 1 }
        };
        document.Segments.Add(new PathSegment { Kind = SegmentKind.Dot, X1 = 0, Y1 = 0 });
        document.Segments.Add(new PathSegment
        {
            Kind = SegmentKind.Line,
            X1 = 0,
            Y1 = 0,
            X2 = 10,
            Y2 = 0,
            Width_mm = 0.4
        });
        document.Segments.Add(new PathSegment
        {
            Kind = SegmentKind.Arc,
            X1 = 10,
            Y1 = 0,
            X2 = 15,
            Y2 = 5,
            X3 = 20,
            Y3 = 0,
            Width_mm = 0.4
        });
        document.Segments.Add(new PathSegment
        {
            Kind = SegmentKind.Arc,
            X1 = 30,
            Y1 = 0,
            X2 = 35,
            Y2 = 5,
            X3 = 40,
            Y3 = 0,
            Width_mm = 0.4
        });

        List<FanucExportPoint> points = ExportPoints(document);
        Assert.Equal(
            ["dot", "start", "end", "start", "mid", "end", "start", "mid", "end"],
            points.Select(static point => point.Role).ToArray());

        string ls = WriteLs(points);
        string motion = MotionSection(ls);

        Assert.Contains("L P[1] 100mm/sec CNT100", motion, StringComparison.Ordinal);
        Assert.Contains("L P[3] 100mm/sec CNT100", motion, StringComparison.Ordinal);
        Assert.Contains("C P[5]\n    :  P[6] 100mm/sec CNT100", motion, StringComparison.Ordinal);
        Assert.Contains("L P[7] 100mm/sec CNT100", motion, StringComparison.Ordinal);
        Assert.Contains("C P[8]\n    :  P[9] 100mm/sec FINE", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[2]", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[4]", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[5]", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[6]", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[8]", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[9]", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("A P[", motion, StringComparison.Ordinal);
        Assert.Contains("P[5]{", ls, StringComparison.Ordinal);
        Assert.Contains("P[6]{", ls, StringComparison.Ordinal);
    }

    [Fact]
    public void ExportLs_WritesACircleAsTwoCircularMoves()
    {
        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Teach,
            Defaults = new ProcessDefaults { Z_mm = 1 }
        };
        foreach (PathSegment arc in ShapeExpander.CircleToArcs(0, 0, 10, 0, glueOn: true))
        {
            arc.Width_mm = 0.4;
            document.Segments.Add(arc);
        }

        string motion = MotionSection(WriteLs(ExportPoints(document)));

        Assert.Contains("L P[1] 100mm/sec CNT100", motion, StringComparison.Ordinal);
        Assert.Contains("C P[2]\n    :  P[3] 100mm/sec CNT100", motion, StringComparison.Ordinal);
        Assert.Contains("C P[5]\n    :  P[6] 100mm/sec FINE", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[2]", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[3]", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[4]", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[5]", motion, StringComparison.Ordinal);
        Assert.DoesNotContain("L P[6]", motion, StringComparison.Ordinal);
    }

    [Fact]
    public void ExportLs_UsesTheArcEndSpeedOnTheCircularMove()
    {
        BeadFeedSettings settings = BeadFeedSettings.CreateDefault();
        var points = new List<FanucExportPoint>
        {
            new()
            {
                Index = 1,
                Role = "start",
                ExportX_mm = 0,
                ExportY_mm = 0,
                Z_mm = 1,
                Width_mm = 0.4
            },
            new()
            {
                Index = 2,
                Role = "mid",
                ExportX_mm = 5,
                ExportY_mm = 5,
                Z_mm = 1,
                Width_mm = 0.4
            },
            new()
            {
                Index = 3,
                Role = "end",
                ExportX_mm = 10,
                ExportY_mm = 0,
                Z_mm = 1,
                Width_mm = 0.4
            }
        };

        string motion = MotionSection(WriteLs(
            points,
            (point, index) => BeadFeed.MotionSpeed(point, index, settings)));

        Assert.Contains("L P[1] 200mm/sec CNT100", motion, StringComparison.Ordinal);
        Assert.Contains("C P[2]\n    :  P[3] 144mm/sec FINE", motion, StringComparison.Ordinal);
        Assert.Contains("Speed=144", motion, StringComparison.Ordinal);
    }

    private static List<FanucExportPoint> ExportPoints(PathDocument document) =>
        GeometryHelper.BuildWaypoints(document, calibration: null)
            .Select(static waypoint => new FanucExportPoint
            {
                Index = waypoint.Index,
                Role = waypoint.Role,
                LocalX_px = waypoint.LocalX_px,
                LocalY_px = waypoint.LocalY_px,
                ExportX_mm = waypoint.ExportX_mm,
                ExportY_mm = waypoint.ExportY_mm,
                ExportR_deg = waypoint.ExportR_deg,
                Z_mm = waypoint.Z_mm,
                Width_mm = waypoint.Width_mm,
                IsPreviewOnly = waypoint.IsPreviewOnly
            })
            .ToList();

    private static string WriteLs(
        IReadOnlyList<FanucExportPoint> points,
        Func<FanucExportPoint, int, double>? speed = null)
    {
        string path = Path.Combine(Path.GetTempPath(), "fanuc-ls-" + Guid.NewGuid().ToString("N") + ".LS");
        try
        {
            FanucExporter.ExportLs(
                points,
                "GLUE",
                path,
                applyFrameYReflection: false,
                speedMmPerSec: speed);
            return File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static string MotionSection(string ls)
    {
        int start = ls.IndexOf("/MN", StringComparison.Ordinal);
        int end = ls.IndexOf("/POS", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start);
        return ls[start..end];
    }
}
