namespace FanucPathAuthor.Controls;

/// <summary>
/// Vertical grip on the left edge of a right-docked panel; drag to resize width.
/// </summary>
public sealed class PanelResizeGrip : Control
{
    private bool _dragging;
    private int _startMouseX;
    private int _startWidth;

    public Panel? TargetPanel { get; set; }
    public int MinTargetWidth { get; set; } = 280;
    public int MaxTargetWidth { get; set; } = 900;

    public PanelResizeGrip()
    {
        Width = 6;
        Dock = DockStyle.Left;
        Cursor = Cursors.VSplit;
        BackColor = Color.FromArgb(70, 70, 70);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && TargetPanel != null)
        {
            _dragging = true;
            _startMouseX = PointToScreen(e.Location).X;
            _startWidth = TargetPanel.Width;
            Capture = true;
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_dragging && TargetPanel != null)
        {
            int screenX = PointToScreen(e.Location).X;
            int delta = _startMouseX - screenX;
            int newWidth = Math.Clamp(_startWidth + delta, MinTargetWidth, MaxTargetWidth);
            TargetPanel.Width = newWidth;
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
        int mid = Width / 2;
        using var pen = new Pen(Color.FromArgb(130, 130, 130));
        for (int y = 8; y < Height - 8; y += 6)
        {
            e.Graphics.DrawLine(pen, mid, y, mid, y + 2);
        }
    }
}
