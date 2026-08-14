using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class SnapPointHelper
{
    public const double DefaultPickRadiusScreenPx = 18;

    public static IReadOnlyList<(double SensorX, double SensorY)> CollectSensorPoints(PathDocument document)
    {
        var points = new List<(double SensorX, double SensorY)>();
        foreach (PathSegment segment in document.Segments)
        {
            foreach ((double localX, double localY) in GeometryHelper.ExpandSegment(segment))
            {
                var sensor = CoordinateTransform.LocalToSensor(localX, localY, document.Frame);
                AddUnique(points, sensor.X, sensor.Y);
            }
        }

        return points;
    }

    public static IReadOnlyList<(double SensorX, double SensorY)> CollectSensorPointsExcluding(
        PathDocument document,
        IEnumerable<(double LocalX, double LocalY)> excludeLocal)
    {
        var exclude = excludeLocal.ToList();
        var points = new List<(double SensorX, double SensorY)>();
        foreach (PathSegment segment in document.Segments)
        {
            foreach ((double localX, double localY) in GeometryHelper.ExpandSegment(segment))
            {
                if (IsExcluded(exclude, localX, localY))
                {
                    continue;
                }

                var sensor = CoordinateTransform.LocalToSensor(localX, localY, document.Frame);
                AddUnique(points, sensor.X, sensor.Y);
            }
        }

        return points;
    }

    public static bool ContainsSensorPoint(
        IReadOnlyList<(double SensorX, double SensorY)> points,
        double sensorX,
        double sensorY,
        double tolerancePx = 0.5)
    {
        foreach ((double x, double y) in points)
        {
            if (Math.Abs(x - sensorX) <= tolerancePx && Math.Abs(y - sensorY) <= tolerancePx)
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryFindNearestScreen(
        IReadOnlyList<(double SensorX, double SensorY)> points,
        double clickSensorX,
        double clickSensorY,
        Func<double, double, PointF> sensorToScreen,
        double pickRadiusScreenPx,
        out double snapSensorX,
        out double snapSensorY)
    {
        snapSensorX = clickSensorX;
        snapSensorY = clickSensorY;

        if (points.Count == 0)
        {
            return false;
        }

        PointF clickScreen = sensorToScreen(clickSensorX, clickSensorY);
        double bestDist = double.MaxValue;
        bool found = false;

        foreach ((double sensorX, double sensorY) in points)
        {
            PointF pointScreen = sensorToScreen(sensorX, sensorY);
            double dx = pointScreen.X - clickScreen.X;
            double dy = pointScreen.Y - clickScreen.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist > pickRadiusScreenPx || dist >= bestDist)
            {
                continue;
            }

            bestDist = dist;
            snapSensorX = sensorX;
            snapSensorY = sensorY;
            found = true;
        }

        return found;
    }

    private static void AddUnique(List<(double SensorX, double SensorY)> points, double sensorX, double sensorY)
    {
        foreach ((double x, double y) in points)
        {
            if (Math.Abs(x - sensorX) < 0.5 && Math.Abs(y - sensorY) < 0.5)
            {
                return;
            }
        }

        points.Add((sensorX, sensorY));
    }

    private static bool IsExcluded(
        IReadOnlyList<(double LocalX, double LocalY)> excludeLocal,
        double localX,
        double localY)
    {
        foreach ((double excludeX, double excludeY) in excludeLocal)
        {
            if (Math.Abs(excludeX - localX) < 0.5 && Math.Abs(excludeY - localY) < 0.5)
            {
                return true;
            }
        }

        return false;
    }
}
