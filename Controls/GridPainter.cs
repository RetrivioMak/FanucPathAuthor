using DispensePathAuthor.Models;

namespace DispensePathAuthor.Controls;

internal static class GridPainter
{
    private const double MinScreenSpacing = 8;

    public static void Draw(
        Graphics graphics,
        TeachGridSettings grid,
        Size clientSize,
        Func<double, double, PointF> toScreen,
        Func<Point, (double X, double Y)> toGrid)
    {
        if (!grid.Enabled || grid.Step_mm <= 0 || clientSize.Width <= 0 || clientSize.Height <= 0)
        {
            return;
        }

        double step = grid.Step_mm;
        double screenStep = ScreenStep(toScreen, grid.OriginX_mm, grid.OriginY_mm, step);
        if (screenStep < 1e-9 || double.IsNaN(screenStep) || double.IsInfinity(screenStep))
        {
            return;
        }

        const int majorEvery = 5;
        if (screenStep * majorEvery < MinScreenSpacing)
        {
            return;
        }

        bool showMinor = screenStep >= MinScreenSpacing;

        (double minX, double maxX, double minY, double maxY) = VisibleBounds(toGrid, clientSize);
        if (double.IsNaN(minX) || double.IsInfinity(maxX) || double.IsNaN(minY) || double.IsInfinity(maxY))
        {
            return;
        }

        double firstX = Math.Floor((minX - grid.OriginX_mm) / step) - 1;
        double lastX = Math.Ceiling((maxX - grid.OriginX_mm) / step) + 1;
        double firstY = Math.Floor((minY - grid.OriginY_mm) / step) - 1;
        double lastY = Math.Ceiling((maxY - grid.OriginY_mm) / step) + 1;
        if (firstX < int.MinValue / 4 || lastX > int.MaxValue / 4 ||
            firstY < int.MinValue / 4 || lastY > int.MaxValue / 4)
        {
            return;
        }

        int stride = showMinor ? 1 : majorEvery;
        int i0 = SnapDown((int)firstX, stride);
        int i1 = (int)lastX;
        int j0 = SnapDown((int)firstY, stride);
        int j1 = (int)lastY;
        double spacing = step;

        using var minor = new SolidBrush(Color.FromArgb(150, 176, 176, 176));
        using var major = new SolidBrush(Color.FromArgb(220, 230, 230, 230));
        for (int i = i0; i <= i1; i += stride)
        {
            double x = grid.OriginX_mm + i * spacing;
            bool majorX = i % majorEvery == 0;
            for (int j = j0; j <= j1; j += stride)
            {
                double y = grid.OriginY_mm + j * spacing;
                bool isMajor = majorX && j % majorEvery == 0;
                PointF screen = toScreen(x, y);
                float radius = isMajor ? 2.4f : 1.3f;
                graphics.FillEllipse(isMajor ? major : minor, screen.X - radius, screen.Y - radius, radius * 2, radius * 2);
            }
        }
    }

    private static double ScreenStep(
        Func<double, double, PointF> toScreen,
        double originX,
        double originY,
        double step)
    {
        double alongX = AxisScreenStep(toScreen, originX, originY, step, alongX: true);
        double alongY = AxisScreenStep(toScreen, originX, originY, step, alongX: false);
        if (alongX <= 1e-9)
        {
            return alongY;
        }

        if (alongY <= 1e-9)
        {
            return alongX;
        }

        return Math.Min(alongX, alongY);
    }

    private static double AxisScreenStep(
        Func<double, double, PointF> toScreen,
        double originX,
        double originY,
        double step,
        bool alongX)
    {
        PointF origin = toScreen(originX, originY);
        for (double probe = 1; probe <= 1_000_000; probe *= 10)
        {
            PointF next = alongX
                ? toScreen(originX + step * probe, originY)
                : toScreen(originX, originY + step * probe);
            double distance = Distance(origin, next);
            if (distance >= 0.5)
            {
                return distance / probe;
            }
        }

        return 0;
    }

    private static (double MinX, double MaxX, double MinY, double MaxY) VisibleBounds(
        Func<Point, (double X, double Y)> toGrid,
        Size clientSize)
    {
        double minX = double.MaxValue;
        double maxX = double.MinValue;
        double minY = double.MaxValue;
        double maxY = double.MinValue;
        foreach (Point corner in new[]
                 {
                     new Point(0, 0),
                     new Point(clientSize.Width, 0),
                     new Point(0, clientSize.Height),
                     new Point(clientSize.Width, clientSize.Height)
                 })
        {
            (double x, double y) = toGrid(corner);
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        return (minX, maxX, minY, maxY);
    }

    private static int SnapDown(int index, int stride)
    {
        int remainder = index % stride;
        if (remainder < 0)
        {
            remainder += stride;
        }

        return index - remainder;
    }

    private static double Distance(PointF a, PointF b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
