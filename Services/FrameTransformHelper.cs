using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class FrameTransformHelper
{
    /// <summary>
    /// Recomputes frame-local coordinates so sensor-pixel positions stay fixed
    /// after the frame definition changes.
    /// </summary>
    public static void RebindSegmentsToFrame(
        IEnumerable<PathSegment> segments,
        FrameDefinition oldFrame,
        FrameDefinition newFrame,
        FrameAxisScale? fromMillimeters = null,
        FrameAxisScale? toMillimeters = null)
    {
        foreach (PathSegment segment in segments)
        {
            (segment.X1, segment.Y1) = RebindPoint(segment.X1, segment.Y1, oldFrame, newFrame, fromMillimeters, toMillimeters);

            if (segment.Kind is SegmentKind.Line or SegmentKind.Arc)
            {
                (segment.X2, segment.Y2) = RebindPoint(segment.X2, segment.Y2, oldFrame, newFrame, fromMillimeters, toMillimeters);
            }

            if (segment.Kind == SegmentKind.Arc)
            {
                (segment.X3, segment.Y3) = RebindPoint(segment.X3, segment.Y3, oldFrame, newFrame, fromMillimeters, toMillimeters);
            }
        }
    }

    private static (double X, double Y) RebindPoint(
        double localX,
        double localY,
        FrameDefinition oldFrame,
        FrameDefinition newFrame,
        FrameAxisScale? fromMillimeters,
        FrameAxisScale? toMillimeters)
    {
        if (fromMillimeters is FrameAxisScale fromScale)
        {
            (localX, localY) = FrameMillimeters.ToLocalPixels(localX, localY, fromScale);
        }

        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(localX, localY, oldFrame);
        (localX, localY) = CoordinateTransform.SensorToLocal(sensorX, sensorY, newFrame);
        if (toMillimeters is FrameAxisScale toScale)
        {
            (localX, localY) = FrameMillimeters.ToMillimeters(localX, localY, toScale);
        }

        return (localX, localY);
    }
}
