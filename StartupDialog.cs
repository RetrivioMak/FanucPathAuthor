using System.Drawing.Drawing2D;

namespace DispensePathAuthor;

public enum StartupAction
{
    NewCv,
    NewTeach,
    Open
}

public readonly record struct ProjectStart(StartupAction Action, string? FilePath);

public partial class StartupDialog : Form
{
    private static readonly Color HoverColor = Color.FromArgb(232, 240, 254);
    private static readonly Color RowColor = Color.White;

    private readonly Bitmap _cvImage;
    private readonly Bitmap _teachImage;
    private readonly Bitmap _openImage;

    public StartupAction Action { get; private set; }

    public string? FilePath { get; private set; }

    public StartupDialog()
    {
        InitializeComponent();
        Text = AppVersion.ProductName;
        _cvImage = CreateCvIcon();
        _teachImage = CreateTeachIcon();
        _openImage = CreateOpenIcon();
        _cvIcon.Image = _cvImage;
        _teachIcon.Image = _teachImage;
        _openIcon.Image = _openImage;
        BindRow(_cvRow, () => Accept(StartupAction.NewCv));
        BindRow(_teachRow, () => Accept(StartupAction.NewTeach));
        BindRow(_openRow, OpenProject);
        PaintRow(_cvRow);
        PaintRow(_teachRow);
        PaintRow(_openRow);
        FormClosed += (_, _) =>
        {
            _cvImage.Dispose();
            _teachImage.Dispose();
            _openImage.Dispose();
        };
        Shown += (_, _) => _cvRow.Focus();
    }

    private void Accept(StartupAction action)
    {
        Action = action;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OpenProject()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = Services.ProjectPackage.OpenFilter,
            Title = "Open project"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            Services.ProjectPackage.LoadFile(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Open project",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        FilePath = dialog.FileName;
        Accept(StartupAction.Open);
    }

    private void BindRow(Control row, Action activate)
    {
        row.KeyDown += (_, e) =>
        {
            if (e.KeyCode is Keys.Enter or Keys.Space)
            {
                activate();
                e.Handled = true;
            }
        };
        Bind(row);

        void Bind(Control control)
        {
            control.Click += (_, _) => activate();
            control.Cursor = Cursors.Hand;
            control.MouseEnter += (_, _) => row.BackColor = HoverColor;
            control.MouseLeave += (_, _) => RefreshRowColor(row);
            foreach (Control child in control.Controls)
            {
                Bind(child);
            }
        }

        RefreshRowColor(row);
    }

    private static void RefreshRowColor(Control row)
    {
        Point cursor = row.PointToClient(Cursor.Position);
        Color color = row.ClientRectangle.Contains(cursor) || row.ContainsFocus
            ? HoverColor
            : RowColor;
        ApplyColor(row, color);
    }

    private static void ApplyColor(Control control, Color color)
    {
        control.BackColor = color;
        foreach (Control child in control.Controls)
        {
            ApplyColor(child, color);
        }
    }

    private static void PaintRow(Control row)
    {
        row.Paint += (_, e) =>
        {
            Rectangle bounds = row.ClientRectangle;
            bounds.Width -= 1;
            bounds.Height -= 1;
            Color border = row.ContainsFocus
                ? Color.FromArgb(0, 120, 212)
                : Color.FromArgb(214, 214, 214);
            using var pen = new Pen(border);
            e.Graphics.DrawRectangle(pen, bounds);
        };
        row.GotFocus += (_, _) =>
        {
            RefreshRowColor(row);
            row.Invalidate();
        };
        row.LostFocus += (_, _) =>
        {
            RefreshRowColor(row);
            row.Invalidate();
        };
    }

    private static Bitmap CreateCvIcon() => DrawIcon(g =>
    {
        using var body = new Pen(Color.FromArgb(0, 120, 212), 2f);
        g.DrawRectangle(body, 6, 9, 20, 14);
        using var cross = new Pen(Color.FromArgb(46, 160, 67), 2f);
        g.DrawLine(cross, 16, 5, 16, 27);
        g.DrawLine(cross, 5, 16, 27, 16);
    });

    private static Bitmap CreateTeachIcon() => DrawIcon(g =>
    {
        using var brush = new SolidBrush(Color.FromArgb(0, 120, 212));
        for (int x = 8; x <= 24; x += 8)
        {
            for (int y = 8; y <= 24; y += 8)
            {
                g.FillEllipse(brush, x - 2, y - 2, 5, 5);
            }
        }
    });

    private static Bitmap CreateOpenIcon() => DrawIcon(g =>
    {
        using var brush = new SolidBrush(Color.FromArgb(242, 180, 68));
        g.FillRectangle(brush, 5, 12, 22, 13);
        g.FillRectangle(brush, 5, 8, 10, 5);
    });

    private static Bitmap DrawIcon(Action<Graphics> draw)
    {
        var bitmap = new Bitmap(32, 32);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);
        draw(graphics);
        return bitmap;
    }
}
