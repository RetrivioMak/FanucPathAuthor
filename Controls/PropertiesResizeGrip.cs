namespace DispensePathAuthor.Controls;

/// <summary>
/// Horizontal grip on the top edge of the properties pane; drag to resize its height.
/// </summary>
public sealed class PropertiesResizeGrip : Control
{
    private bool _dragging;
    private int _startY;
    private int _startHeight;

    public int MinHostHeight { get; set; } = 140;

    public int MinListHeight { get; set; } = 140;

    public event EventHandler? UserSized;

    public PropertiesResizeGrip()
    {
        Height = 8;
        Dock = DockStyle.Top;
        Cursor = Cursors.HSplit;
        BackColor = Color.FromArgb(220, 220, 220);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && Parent is Panel host && host.Parent != null)
        {
            _dragging = true;
            _startY = PointToScreen(e.Location).Y;
            _startHeight = host.Height;
            Capture = true;
            UserSized?.Invoke(this, EventArgs.Empty);
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_dragging && Parent is Panel host && host.Parent != null)
        {
            int delta = _startY - PointToScreen(e.Location).Y;
            int max = Math.Max(MinHostHeight, host.Parent.ClientSize.Height - MinListHeight);
            host.Height = Math.Clamp(_startHeight + delta, MinHostHeight, max);
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (_dragging)
        {
            _dragging = false;
            Capture = false;
        }

        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        int center = Width / 2;
        int mid = Height / 2;
        using var pen = new Pen(Color.FromArgb(130, 130, 130));
        for (int x = center - 18; x <= center + 18; x += 6)
        {
            e.Graphics.DrawLine(pen, x, mid, x + 2, mid);
        }
    }
}
