using DispensePathAuthor.Contracts;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public sealed class ExportHost
{
    public required PathDocument Document { get; init; }

    public required RobotCalibrationFile? Calibration { get; init; }
}

public readonly record struct FrameTrajectoryResult(IReadOnlyList<FrameMove> Moves, string? Error);

public static class FrameTrajectory
{
    public static FrameTrajectoryResult Build(PathDocument document, RobotCalibrationFile? calibration)
    {
        bool storedMillimeters = document.Workspace == WorkspaceKind.Teach ||
                                 FrameMillimeters.StoresMillimeters(document);
        FrameAxisScale scale = default;
        bool convert = false;
        if (!storedMillimeters && document.Segments.Count > 0)
        {
            if (!FrameMillimeters.TryGetScale(document.Frame, calibration, out scale))
            {
                return new FrameTrajectoryResult(
                    [],
                    "Load calibration before exporting this CV path. It is still stored in pixels.");
            }

            convert = true;
        }

        var moves = new List<FrameMove>(document.Segments.Count);
        foreach (PathSegment segment in document.Segments)
        {
            if (segment.Kind is not (SegmentKind.Dot or SegmentKind.Line or SegmentKind.Arc))
            {
                continue;
            }

            (double x1, double y1) = Map(segment.X1, segment.Y1, convert, scale);
            (double x2, double y2) = Map(segment.X2, segment.Y2, convert, scale);
            (double x3, double y3) = Map(segment.X3, segment.Y3, convert, scale);
            moves.Add(new FrameMove
            {
                Kind = segment.Kind switch
                {
                    SegmentKind.Dot => FrameMoveKind.Dot,
                    SegmentKind.Line => FrameMoveKind.Line,
                    _ => FrameMoveKind.Arc
                },
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                X3 = x3,
                Y3 = y3,
                Dispense = segment.GlueOn,
                SpeedMmPerS = segment.Speed_mm_s
            });
        }

        return new FrameTrajectoryResult(moves, null);
    }

    private static (double X, double Y) Map(double x, double y, bool convert, FrameAxisScale scale) =>
        convert ? FrameMillimeters.ToMillimeters(x, y, scale) : (x, y);
}
