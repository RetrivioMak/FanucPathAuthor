namespace FanucPathAuthor.Controls;

internal static class ToolIconFactory
{
    private const int Size = 32;

    public static Image Select => Draw(g =>
    {
        Point[] cursor =
        [
            new(7, 3),
            new(7, 24),
            new(12, 19),
            new(15, 27),
            new(18, 26),
            new(15, 18),
            new(22, 18)
        ];

        g.FillPolygon(Brushes.White, cursor);
        using var pen = new Pen(Color.Black, 1.8f);
        g.DrawPolygon(pen, cursor);
    });

    public static Image Move => Draw(g =>
    {
        using var pen = new Pen(Color.FromArgb(60, 60, 60), 2.5f);
        g.DrawLine(pen, 16, 4, 16, 28);
        g.DrawLine(pen, 4, 16, 28, 16);
        g.DrawLine(pen, 16, 4, 12, 8);
        g.DrawLine(pen, 16, 4, 20, 8);
        g.DrawLine(pen, 16, 28, 12, 24);
        g.DrawLine(pen, 16, 28, 20, 24);
        g.DrawLine(pen, 4, 16, 8, 12);
        g.DrawLine(pen, 4, 16, 8, 20);
        g.DrawLine(pen, 28, 16, 24, 12);
        g.DrawLine(pen, 28, 16, 24, 20);
    });

    public static Image Frame => Draw(g =>
    {
        using var pen = new Pen(Color.LimeGreen, 2.5f);
        g.DrawLine(pen, 16, 8, 16, 24);
        g.DrawLine(pen, 8, 16, 24, 16);
        g.FillEllipse(Brushes.LimeGreen, 14, 14, 4, 4);
    });

    public static Image Dot => Draw(g =>
    {
        g.FillEllipse(Brushes.DeepSkyBlue, 12, 12, 8, 8);
    });

    public static Image Line => Draw(g =>
    {
        using var pen = new Pen(Color.DeepSkyBlue, 2.5f);
        g.DrawLine(pen, 7, 23, 25, 9);
    });

    public static Image Arc => Draw(g =>
    {
        using var pen = new Pen(Color.DeepSkyBlue, 2.5f);
        g.DrawArc(pen, 5, 8, 22, 22, 180, 135);
    });

    public static Image Rect => Draw(g =>
    {
        using var pen = new Pen(Color.DeepSkyBlue, 2.5f);
        g.DrawRectangle(pen, 8, 9, 16, 14);
    });

    public static Image Circle => Draw(g =>
    {
        using var pen = new Pen(Color.DeepSkyBlue, 2.5f);
        g.DrawEllipse(pen, 7, 7, 18, 18);
    });

    public static Image Fit => Draw(g =>
    {
        using var pen = new Pen(Color.FromArgb(60, 60, 60), 2.5f);
        g.DrawRectangle(pen, 7, 7, 18, 18);
        g.DrawLine(pen, 3, 16, 29, 16);
        g.DrawLine(pen, 16, 3, 16, 29);
    });

    public static Image Snap => Draw(g =>
    {
        using var pen = new Pen(Color.FromArgb(220, 120, 0), 2.5f);
        g.DrawEllipse(pen, 10, 10, 12, 12);
        g.DrawLine(pen, 16, 4, 16, 10);
        g.DrawLine(pen, 16, 22, 16, 28);
        g.DrawLine(pen, 4, 16, 10, 16);
        g.DrawLine(pen, 22, 16, 28, 16);
        g.FillEllipse(new SolidBrush(Color.FromArgb(220, 120, 0)), 14, 14, 4, 4);
    });

    public static Image SimPlay => Draw(g =>
    {
        g.FillPolygon(new SolidBrush(Color.FromArgb(40, 167, 69)), new[]
        {
            new Point(11, 8),
            new Point(11, 24),
            new Point(24, 16)
        });
    });

    public static Image SimPause => Draw(g =>
    {
        using var brush = new SolidBrush(Color.FromArgb(60, 60, 60));
        g.FillRectangle(brush, 10, 8, 5, 16);
        g.FillRectangle(brush, 17, 8, 5, 16);
    });

    public static Image SimStop => Draw(g =>
    {
        g.FillRectangle(new SolidBrush(Color.FromArgb(220, 53, 69)), 10, 10, 12, 12);
    });

    private static Bitmap Draw(Action<Graphics> draw)
    {
        var bitmap = new Bitmap(Size, Size);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        draw(g);
        return bitmap;
    }
}
