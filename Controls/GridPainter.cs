using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Controls;

internal sealed class GridPainter : IDisposable
{
    // Hide the whole grid when neighboring points are closer than this on screen.
    private const double MinScreenSpacing = 8;

    // Every Nth intersection is drawn larger so the grid stays readable when zoomed in.
    private const int MajorEvery = 5;

    // Pixel radius of a normal intersection and of every Nth one.
    private const int MinorPointRadius = 1;
    private const int MajorPointRadius = 2;

    // Extra pixels around the window. Panning inside this margin reuses the bitmap.
    private const int PanMargin = 320;

    private static readonly byte[] MinorColor = Premultiply(150, 176, 176, 176);
    private static readonly byte[] MajorColor = Premultiply(220, 230, 230, 230);
    private static readonly (int X, int Y)[] MinorStamp = Circle(MinorPointRadius);
    private static readonly (int X, int Y)[] MajorStamp = Circle(MajorPointRadius);

    private Bitmap? _bitmap;
    private byte[]? _pixels;
    private float _zoom;
    private float _panX;
    private float _panY;
    private int _width;
    private int _height;
    private double _step;
    private double _originX;
    private double _originY;
    private double _frameX;
    private double _frameY;
    private double _frameAngle;
    private bool _frameInvertY;
    private bool _ready;

    public void Draw(
        Graphics graphics,
        TeachGridSettings grid,
        FrameDefinition frame,
        Size clientSize,
        float zoom,
        PointF pan,
        Func<double, double, PointF> toScreen,
        Func<Point, (double X, double Y)> toGrid)
    {
        if (!grid.Enabled || grid.Step_mm <= 0 || clientSize.Width <= 0 || clientSize.Height <= 0)
        {
            _ready = false;
            return;
        }

        double screenStep = ScreenStep(toScreen, grid.OriginX_mm, grid.OriginY_mm, grid.Step_mm);
        if (screenStep < MinScreenSpacing || double.IsNaN(screenStep) || double.IsInfinity(screenStep))
        {
            _ready = false;
            return;
        }

        if (!CanReuse(grid, frame, clientSize, zoom, pan))
        {
            Rebuild(grid, frame, clientSize, zoom, pan, toScreen, toGrid);
        }

        if (_bitmap == null || !_ready)
        {
            return;
        }

        int dx = (int)Math.Round(pan.X - _panX) - PanMargin;
        int dy = (int)Math.Round(pan.Y - _panY) - PanMargin;
        graphics.DrawImageUnscaled(_bitmap, dx, dy);
    }

    public void Dispose()
    {
        _bitmap?.Dispose();
        _bitmap = null;
        _pixels = null;
        _ready = false;
    }

    private bool CanReuse(TeachGridSettings grid, FrameDefinition frame, Size clientSize, float zoom, PointF pan) =>
        _ready &&
        _bitmap != null &&
        _width == clientSize.Width &&
        _height == clientSize.Height &&
        _zoom.Equals(zoom) &&
        Math.Abs(pan.X - _panX) <= PanMargin &&
        Math.Abs(pan.Y - _panY) <= PanMargin &&
        _step.Equals(grid.Step_mm) &&
        _originX.Equals(grid.OriginX_mm) &&
        _originY.Equals(grid.OriginY_mm) &&
        _frameX.Equals(frame.OriginX_px) &&
        _frameY.Equals(frame.OriginY_px) &&
        _frameAngle.Equals(frame.AngleDeg) &&
        _frameInvertY == frame.InvertY;

    private void Rebuild(
        TeachGridSettings grid,
        FrameDefinition frame,
        Size clientSize,
        float zoom,
        PointF pan,
        Func<double, double, PointF> toScreen,
        Func<Point, (double X, double Y)> toGrid)
    {
        _ready = false;
        int bitmapWidth = clientSize.Width + PanMargin * 2;
        int bitmapHeight = clientSize.Height + PanMargin * 2;
        EnsureBitmap(bitmapWidth, bitmapHeight);
        if (_bitmap == null)
        {
            return;
        }

        (double minX, double maxX, double minY, double maxY) = VisibleBounds(toGrid, clientSize, PanMargin);
        if (double.IsNaN(minX) || double.IsInfinity(maxX) || double.IsNaN(minY) || double.IsInfinity(maxY))
        {
            return;
        }

        double step = grid.Step_mm;
        double firstX = Math.Floor((minX - grid.OriginX_mm) / step) - 1;
        double lastX = Math.Ceiling((maxX - grid.OriginX_mm) / step) + 1;
        double firstY = Math.Floor((minY - grid.OriginY_mm) / step) - 1;
        double lastY = Math.Ceiling((maxY - grid.OriginY_mm) / step) + 1;
        if (firstX < int.MinValue / 4 || lastX > int.MaxValue / 4 ||
            firstY < int.MinValue / 4 || lastY > int.MaxValue / 4)
        {
            return;
        }

        var bits = _bitmap.LockBits(
            new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppPArgb);
        try
        {
            int stride = bits.Stride;
            int length = stride * _bitmap.Height;
            if (_pixels == null || _pixels.Length != length)
            {
                _pixels = new byte[length];
            }
            else
            {
                Array.Clear(_pixels);
            }

            for (int i = (int)firstX; i <= (int)lastX; i++)
            {
                double x = grid.OriginX_mm + i * step;
                bool majorX = i % MajorEvery == 0;
                for (int j = (int)firstY; j <= (int)lastY; j++)
                {
                    PointF screen = toScreen(x, grid.OriginY_mm + j * step);
                    bool isMajor = majorX && j % MajorEvery == 0;
                    Plot(
                        stride,
                        (int)Math.Round(screen.X) + PanMargin,
                        (int)Math.Round(screen.Y) + PanMargin,
                        isMajor);
                }
            }

            Marshal.Copy(_pixels, 0, bits.Scan0, length);
        }
        finally
        {
            _bitmap.UnlockBits(bits);
        }

        _zoom = zoom;
        _panX = pan.X;
        _panY = pan.Y;
        _width = clientSize.Width;
        _height = clientSize.Height;
        _step = grid.Step_mm;
        _originX = grid.OriginX_mm;
        _originY = grid.OriginY_mm;
        _frameX = frame.OriginX_px;
        _frameY = frame.OriginY_px;
        _frameAngle = frame.AngleDeg;
        _frameInvertY = frame.InvertY;
        _ready = true;
    }

    private void EnsureBitmap(int width, int height)
    {
        if (_bitmap != null && _bitmap.Width == width && _bitmap.Height == height)
        {
            return;
        }

        _bitmap?.Dispose();
        _bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        _pixels = null;
        _ready = false;
    }

    private void Plot(int stride, int x, int y, bool major)
    {
        if (_bitmap == null || _pixels == null)
        {
            return;
        }

        (int X, int Y)[] stamp = major ? MajorStamp : MinorStamp;
        byte[] color = major ? MajorColor : MinorColor;
        int width = _bitmap.Width;
        int height = _bitmap.Height;
        foreach ((int dx, int dy) in stamp)
        {
            int px = x + dx;
            int py = y + dy;
            if ((uint)px >= (uint)width || (uint)py >= (uint)height)
            {
                continue;
            }

            int offset = py * stride + px * 4;
            _pixels[offset] = color[0];
            _pixels[offset + 1] = color[1];
            _pixels[offset + 2] = color[2];
            _pixels[offset + 3] = color[3];
        }
    }

    private static byte[] Premultiply(byte alpha, byte red, byte green, byte blue) =>
    [
        (byte)(blue * alpha / 255),
        (byte)(green * alpha / 255),
        (byte)(red * alpha / 255),
        alpha
    ];

    private static (int X, int Y)[] Circle(int radius)
    {
        int limit = radius * radius;
        var points = new List<(int X, int Y)>();
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= limit)
                {
                    points.Add((x, y));
                }
            }
        }

        return points.ToArray();
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
        Size clientSize,
        int margin)
    {
        double minX = double.MaxValue;
        double maxX = double.MinValue;
        double minY = double.MaxValue;
        double maxY = double.MinValue;
        foreach (Point corner in new[]
                 {
                     new Point(-margin, -margin),
                     new Point(clientSize.Width + margin, -margin),
                     new Point(-margin, clientSize.Height + margin),
                     new Point(clientSize.Width + margin, clientSize.Height + margin)
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

    private static double Distance(PointF a, PointF b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
