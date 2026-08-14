using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class FrameTransformHelper
{
    /// <summary>
    /// Recomputes frame-local coordinates so sensor-pixel positions stay fixed
    /// after the frame definition changes.
    /// </summary>
    public static void RebindSegmentsToFrame(
        IEnumerable<PathSegment> segments,
        FrameDefinition oldFrame,
        FrameDefinition newFrame)
    {
        foreach (PathSegment segment in segments)
        {
            (segment.X1, segment.Y1) = RebindPoint(segment.X1, segment.Y1, oldFrame, newFrame);

            if (segment.Kind is SegmentKind.Line or SegmentKind.Arc)
            {
                (segment.X2, segment.Y2) = RebindPoint(segment.X2, segment.Y2, oldFrame, newFrame);
            }

            if (segment.Kind == SegmentKind.Arc)
            {
                (segment.X3, segment.Y3) = RebindPoint(segment.X3, segment.Y3, oldFrame, newFrame);
            }
        }
    }

    private static (double X, double Y) RebindPoint(
        double localX,
        double localY,
        FrameDefinition oldFrame,
        FrameDefinition newFrame)
    {
        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(localX, localY, oldFrame);
        return CoordinateTransform.SensorToLocal(sensorX, sensorY, newFrame);
    }
}
