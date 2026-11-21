#nullable enable

namespace DispensePathAuthor.Xyz;

partial class XyzSettingsPanel
{
    private System.ComponentModel.IContainer? components = null;

    private NumericUpDown _zHeight = null!;
    private NumericUpDown _valveOutput = null!;
    private NumericUpDown _chordTolerance = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _zHeight = CreateNumeric(-500, 500, 0.001m, 3);
        _valveOutput = CreateNumeric(0, 255, 1, 0);
        _chordTolerance = CreateNumeric(0.01m, 10, 0.01m, 3);
        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 4,
            Dock = DockStyle.Top,
            Margin = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        AddRow(layout, 0, "Z height, mm", _zHeight);
        AddRow(layout, 1, "Valve output", _valveOutput);
        AddRow(layout, 2, "Arc chord, mm", _chordTolerance);
        var hint = new Label
        {
            Text = "Z stays at this height until a dispense law fills the channel. Arcs are written as a polyline.",
            AutoSize = true,
            MaximumSize = new Size(520, 0),
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 6, 0, 0)
        };
        layout.Controls.Add(hint, 0, 3);
        layout.SetColumnSpan(hint, 2);
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Margin = new Padding(0);
        Controls.Add(layout);
    }

    private static void AddRow(TableLayoutPanel layout, int row, string caption, Control editor)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = true,
            Padding = new Padding(0, 6, 8, 0)
        }, 0, row);
        editor.Margin = new Padding(0, 2, 0, 2);
        layout.Controls.Add(editor, 1, row);
    }

    private static NumericUpDown CreateNumeric(decimal min, decimal max, decimal increment, int decimals) =>
        new()
        {
            Minimum = min,
            Maximum = max,
            DecimalPlaces = decimals,
            Increment = increment,
            Width = 120
        };
}
