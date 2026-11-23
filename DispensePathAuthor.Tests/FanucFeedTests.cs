using DispensePathAuthor.Contracts;
using DispensePathAuthor.Export.Fanuc;
using DispensePathAuthor.Export.FanucFeed;
using Xunit;

namespace DispensePathAuthor.Tests;

public sealed class FanucFeedTests
{
    [Fact]
    public void BeadSpeed_AtNeedleWidth_IsTheReferenceSpeed()
    {
        BeadFeedSettings settings = BeadFeedSettings.CreateDefault();

        double atNeedle = BeadFeed.BeadSpeed(settings.NeedleDiameter_mm, settings.Flow_mm3_s);
        double atSegment = BeadFeed.BeadSpeed(0.4, settings.Flow_mm3_s);

        Assert.Equal(100, atNeedle, 3);
        Assert.Equal(144, atSegment, 3);
    }

    [Fact]
    public void MotionSpeed_UsesTravelOnApproachAndBeadOnTheSegment()
    {
        BeadFeedSettings settings = BeadFeedSettings.CreateDefault();
        var start = new FanucExportPoint { Role = "start", Width_mm = 0.4 };
        var end = new FanucExportPoint { Role = "end", Width_mm = 0.4 };
        var glueOff = new FanucExportPoint { Role = "end", Width_mm = 0 };

        Assert.Equal(settings.TravelSpeed_mm_s, BeadFeed.MotionSpeed(start, 1, settings), 3);
        Assert.Equal(settings.TravelSpeed_mm_s, BeadFeed.MotionSpeed(end, 0, settings), 3);
        Assert.Equal(settings.TravelSpeed_mm_s, BeadFeed.MotionSpeed(glueOff, 1, settings), 3);
        Assert.Equal(144, BeadFeed.MotionSpeed(end, 1, settings), 3);
    }

    [Fact]
    public void MotionSpeed_ClampsTheBeadSpeed()
    {
        var settings = new BeadFeedSettings
        {
            Flow_mm3_s = BeadFeed.DefaultFlow(0.48),
            MinSpeed_mm_s = 10,
            MaxSpeed_mm_s = 120,
            TravelSpeed_mm_s = 200
        };
        var end = new FanucExportPoint { Role = "end", Width_mm = 0.4 };

        Assert.Equal(120, BeadFeed.MotionSpeed(end, 1, settings), 3);
    }

    [Fact]
    public void ExportLs_WritesTheCalculatedSpeed()
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
                Role = "end",
                ExportX_mm = 10,
                ExportY_mm = 0,
                Z_mm = 1,
                Width_mm = 0.4
            }
        };

        string path = Path.Combine(Path.GetTempPath(), "fanuc-feed-" + Guid.NewGuid().ToString("N") + ".LS");
        try
        {
            FanucExporter.ExportLs(
                points,
                "GLUE",
                path,
                applyFrameYReflection: false,
                speedMmPerSec: (point, index) => BeadFeed.MotionSpeed(point, index, settings),
                processNote: "Needle=0.48mm Q=9.048mm3/s");

            string ls = File.ReadAllText(path);
            Assert.Contains("! Needle=0.48mm Q=9.048mm3/s ;", ls, StringComparison.Ordinal);
            Assert.Contains("L P[1] 200mm/sec CNT100", ls, StringComparison.Ordinal);
            Assert.Contains("L P[2] 144mm/sec FINE", ls, StringComparison.Ordinal);
            Assert.Contains("Speed=144", ls, StringComparison.Ordinal);
            Assert.DoesNotContain("100mm/sec", ls, StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
