using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public abstract class CanvasSpace
{
    public abstract bool HasSurfaceWithoutImage { get; }

    public abstract string FrameLabel { get; }

    public abstract bool AllowsFrameEdit { get; }

    public abstract bool ScreenYIncreasesUp { get; }

    public abstract float MinZoom { get; }

    public abstract float MaxZoom { get; }

    public abstract bool ClampFittedZoom { get; }

    public abstract bool ConfirmEveryDot { get; }

    public abstract double PointTolerance { get; }

    public abstract bool HandlesPointer { get; }

    public abstract bool FollowDrawPointer { get; }

    public abstract bool InvertArcSweep { get; }

    public abstract bool HasGrid { get; }

    public abstract bool HasReferencePoints { get; }

    public virtual string FrameEditBlockedMessage => "Teach coordinates stay in the robot frame.";

    public abstract double SegmentPickRadius(float zoom);

    public abstract double FrameAxisLength(float zoom);

    public abstract string FormatCursor(double sensorX, double sensorY, double localX, double localY);

    public virtual bool TryMeasureBackground(PathDocument document, out RectangleF bounds)
    {
        bounds = RectangleF.Empty;
        return false;
    }

    public virtual PointerHit AdjustPointer(
        PathDocument document,
        double x,
        double y,
        IEnumerable<(double LocalX, double LocalY)>? exclude,
        Func<double, double, PointF> toScreen) => new(x, y, false);

    public virtual void DrawBackground(
        Graphics graphics,
        PathDocument document,
        float zoom,
        Size clientSize,
        Func<double, double, PointF> toScreen,
        Func<Point, PointF> toWorld)
    {
    }

    public virtual void DrawForeground(
        Graphics graphics,
        PathDocument document,
        Font font,
        Func<double, double, PointF> toScreen)
    {
    }

    public virtual bool TryHandleRightClick(
        Control owner,
        PathDocument document,
        Point screen,
        Func<double, double, PointF> toScreen,
        Action<double, double> alignGrid) => false;

    public readonly record struct PointerHit(double X, double Y, bool SnappedToPoint);
}

public sealed class CvCanvasSpace : CanvasSpace
{
    public override bool HasSurfaceWithoutImage => false;

    public override string FrameLabel => "Frame";

    public override bool AllowsFrameEdit => true;

    public override bool ScreenYIncreasesUp => false;

    public override float MinZoom => 0.05f;

    public override float MaxZoom => 20f;

    public override bool ClampFittedZoom => false;

    public override bool ConfirmEveryDot => false;

    public override double PointTolerance => 0.5;

    public override bool HandlesPointer => false;

    public override bool FollowDrawPointer => false;

    public override bool InvertArcSweep => false;

    public override bool HasGrid => false;

    public override bool HasReferencePoints => false;

    public override double SegmentPickRadius(float zoom) => 12;

    public override double FrameAxisLength(float zoom) => 80;

    public override string FormatCursor(double sensorX, double sensorY, double localX, double localY) =>
        $"sensor ({sensorX:F0},{sensorY:F0})  local ({localX:F0},{localY:F0})";
}

public sealed class TeachCanvasSpace : CanvasSpace
{
    public override bool HasSurfaceWithoutImage => true;

    public override string FrameLabel => "Robot";

    public override bool AllowsFrameEdit => false;

    public override bool ScreenYIncreasesUp => true;

    public override float MinZoom => 0.2f;

    public override float MaxZoom => 8000f;

    public override bool ClampFittedZoom => true;

    public override bool ConfirmEveryDot => true;

    public override double PointTolerance => 1e-4;

    public override bool HandlesPointer => true;

    public override bool FollowDrawPointer => true;

    public override bool InvertArcSweep => true;

    public override bool HasGrid => true;

    public override bool HasReferencePoints => true;

    public override double SegmentPickRadius(float zoom) => 12.0 / Math.Max(zoom, 0.001f);

    public override double FrameAxisLength(float zoom) => 48.0 / Math.Max(zoom, 0.001f);

    public override string FormatCursor(double sensorX, double sensorY, double localX, double localY) =>
        $"XY mm ({localX:0.000}, {localY:0.000})";

    public override bool TryMeasureBackground(PathDocument document, out RectangleF bounds)
    {
        var xs = new List<double>();
        var ys = new List<double>();
        foreach (ReferencePoint point in document.ReferencePoints)
        {
            xs.Add(point.X_mm);
            ys.Add(point.Y_mm);
        }

        foreach (PathSegment segment in document.Segments)
        {
            foreach ((double x, double y) in GeometryHelper.ExpandSegment(segment))
            {
                xs.Add(x);
                ys.Add(y);
            }
        }

        if (xs.Count == 0)
        {
            bounds = new RectangleF(-10, -10, 20, 20);
            return true;
        }

        double minX = xs.Min();
        double maxX = xs.Max();
        double minY = ys.Min();
        double maxY = ys.Max();
        double spanX = Math.Max(maxX - minX, 1);
        double spanY = Math.Max(maxY - minY, 1);
        double padX = Math.Max(spanX * 0.25, 5);
        double padY = Math.Max(spanY * 0.25, 5);
        bounds = new RectangleF(
            (float)(minX - padX),
            (float)(minY - padY),
            (float)(spanX + padX * 2),
            (float)(spanY + padY * 2));
        return true;
    }

    public override PointerHit AdjustPointer(
        PathDocument document,
        double x,
        double y,
        IEnumerable<(double LocalX, double LocalY)>? exclude,
        Func<double, double, PointF> toScreen)
    {
        const double tolerance = 1e-4;
        var points = new List<(double X, double Y)>();
        foreach (ReferencePoint reference in document.ReferencePoints)
        {
            points.Add((reference.X_mm, reference.Y_mm));
        }

        IReadOnlyList<(double SensorX, double SensorY)> vertices = exclude == null
            ? SnapPointHelper.CollectSensorPoints(document, tolerance)
            : SnapPointHelper.CollectSensorPointsExcluding(document, exclude, tolerance);
        points.AddRange(vertices.Select(static point => (point.SensorX, point.SensorY)));
        (double resolvedX, double resolvedY, bool snappedToPoint) = TeachPointer.Resolve(
            x,
            y,
            points,
            document.Grid,
            toScreen,
            SnapPointHelper.DefaultPickRadiusScreenPx);
        return new PointerHit(resolvedX, resolvedY, snappedToPoint);
    }

    public override void DrawBackground(
        Graphics graphics,
        PathDocument document,
        float zoom,
        Size clientSize,
        Func<double, double, PointF> toScreen,
        Func<Point, PointF> toWorld) =>
        DrawGrid(graphics, document, zoom, clientSize, toScreen, toWorld);

    public override void DrawForeground(
        Graphics graphics,
        PathDocument document,
        Font font,
        Func<double, double, PointF> toScreen) =>
        DrawReferencePoints(graphics, document, font, toScreen);

    public override bool TryHandleRightClick(
        Control owner,
        PathDocument document,
        Point screen,
        Func<double, double, PointF> toScreen,
        Action<double, double> alignGrid)
    {
        ReferencePoint? point = HitReference(document, screen, toScreen);
        if (point == null)
        {
            return true;
        }

        double x = point.X_mm;
        double y = point.Y_mm;
        string name = point.Name;
        var menu = new ContextMenuStrip();
        menu.Items.Add($"Align grid to {name}", null, (_, _) => alignGrid(x, y));
        menu.Closed += (_, _) => menu.Dispose();
        menu.Show(owner, screen);
        return true;
    }

    private static ReferencePoint? HitReference(
        PathDocument document,
        Point screen,
        Func<double, double, PointF> toScreen)
    {
        ReferencePoint? best = null;
        double bestDistance = SnapPointHelper.DefaultPickRadiusScreenPx;
        foreach (ReferencePoint point in document.ReferencePoints)
        {
            PointF marker = toScreen(point.X_mm, point.Y_mm);
            double dx = marker.X - screen.X;
            double dy = marker.Y - screen.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                best = point;
            }
        }

        return best;
    }

    private static void DrawGrid(
        Graphics graphics,
        PathDocument document,
        float zoom,
        Size clientSize,
        Func<double, double, PointF> toScreen,
        Func<Point, PointF> toWorld)
    {
        TeachGridSettings grid = document.Grid;
        if (!grid.Enabled || grid.Step_mm <= 0 || clientSize.Width <= 0 || clientSize.Height <= 0)
        {
            return;
        }

        PointF topLeft = toWorld(new Point(0, 0));
        PointF bottomRight = toWorld(new Point(clientSize.Width, clientSize.Height));
        double minX = Math.Min(topLeft.X, bottomRight.X);
        double maxX = Math.Max(topLeft.X, bottomRight.X);
        double minY = Math.Min(topLeft.Y, bottomRight.Y);
        double maxY = Math.Max(topLeft.Y, bottomRight.Y);
        double step = grid.Step_mm;
        double screenStep = step * zoom;
        int stride = 1;
        while (screenStep * stride < 8 && stride < 1_000_000)
        {
            stride++;
        }

        double drawStep = step * stride;
        int i0 = (int)Math.Floor((minX - grid.OriginX_mm) / drawStep) - 1;
        int i1 = (int)Math.Ceiling((maxX - grid.OriginX_mm) / drawStep) + 1;
        int j0 = (int)Math.Floor((minY - grid.OriginY_mm) / drawStep) - 1;
        int j1 = (int)Math.Ceiling((maxY - grid.OriginY_mm) / drawStep) + 1;
        if (i1 - i0 > 400)
        {
            i1 = i0 + 400;
        }

        if (j1 - j0 > 400)
        {
            j1 = j0 + 400;
        }

        using var minor = new SolidBrush(Color.FromArgb(150, 176, 176, 176));
        using var major = new SolidBrush(Color.FromArgb(220, 230, 230, 230));
        for (int i = i0; i <= i1; i++)
        {
            double x = grid.OriginX_mm + i * drawStep;
            bool majorX = IsMajorGridIndex(x, grid.OriginX_mm, step);
            for (int j = j0; j <= j1; j++)
            {
                double y = grid.OriginY_mm + j * drawStep;
                bool isMajor = majorX && IsMajorGridIndex(y, grid.OriginY_mm, step);
                PointF screen = toScreen(x, y);
                float radius = isMajor ? 2.4f : 1.3f;
                graphics.FillEllipse(isMajor ? major : minor, screen.X - radius, screen.Y - radius, radius * 2, radius * 2);
            }
        }
    }

    private static void DrawReferencePoints(
        Graphics graphics,
        PathDocument document,
        Font font,
        Func<double, double, PointF> toScreen)
    {
        using var pen = new Pen(Color.DeepSkyBlue, 2f);
        using var brush = new SolidBrush(Color.DeepSkyBlue);
        foreach (ReferencePoint point in document.ReferencePoints)
        {
            PointF screen = toScreen(point.X_mm, point.Y_mm);
            const float radius = 6f;
            graphics.DrawEllipse(pen, screen.X - radius, screen.Y - radius, radius * 2, radius * 2);
            graphics.FillEllipse(brush, screen.X - 2, screen.Y - 2, 4, 4);
            graphics.DrawString(point.Name, font, brush, screen.X + 8, screen.Y - 8);
        }
    }

    private static bool IsMajorGridIndex(double value, double origin, double step)
    {
        double index = (value - origin) / step;
        double nearest = Math.Round(index);
        return Math.Abs(index - nearest) < 1e-3 && Math.Abs(nearest % 5) < 1e-3;
    }
}
