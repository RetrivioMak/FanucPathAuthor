using System.Globalization;
using FanucPathAuthor.Models;
using FanucPathAuthor.Services;

namespace FanucPathAuthor;

public sealed class TeachPointImportDialog : Form
{
    private readonly record struct PendingPoint(string? Name, double X, double Y, LengthUnit Unit);

    private readonly HashSet<string> _existingNames;
    private readonly HashSet<string> _usedNames;
    private readonly List<PendingPoint> _pending = new();
    private readonly ComboBox _unit = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
    private readonly TextBox _name = new() { Width = 140 };
    private readonly TextBox _x = new() { Width = 100 };
    private readonly TextBox _y = new() { Width = 100 };
    private readonly ListView _preview = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        HideSelection = false
    };

    public IReadOnlyList<ReferencePoint> Points { get; private set; } = Array.Empty<ReferencePoint>();

    public TeachPointImportDialog(IEnumerable<string> existingNames)
    {
        _existingNames = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
        _usedNames = new HashSet<string>(_existingNames, StringComparer.OrdinalIgnoreCase);
        Text = "Import teach points";
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(640, 480);

        _unit.Items.AddRange(new object[] { "?m", "mm", "cm", "m" });
        _unit.SelectedIndex = 1;

        _preview.Columns.Add("Name", 160);
        _preview.Columns.Add("X", 100);
        _preview.Columns.Add("Y", 100);
        _preview.Columns.Add("Unit", 60);

        var unitRow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 8) };
        unitRow.Controls.Add(new Label { Text = "Unit", AutoSize = true, Padding = new Padding(0, 6, 8, 0) });
        unitRow.Controls.Add(_unit);
        var csvButton = new Button { Text = "Load CSV...", AutoSize = true };
        csvButton.Click += (_, _) => LoadCsv();
        unitRow.Controls.Add(csvButton);

        var manual = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 8) };
        manual.Controls.Add(new Label { Text = "Name", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        manual.Controls.Add(_name);
        manual.Controls.Add(new Label { Text = "X", AutoSize = true, Padding = new Padding(8, 6, 4, 0) });
        manual.Controls.Add(_x);
        manual.Controls.Add(new Label { Text = "Y", AutoSize = true, Padding = new Padding(8, 6, 4, 0) });
        manual.Controls.Add(_y);
        var addButton = new Button { Text = "Add", AutoSize = true };
        addButton.Click += (_, _) => AddManual();
        manual.Controls.Add(addButton);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 0)
        };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        var ok = new Button { Text = "Import", AutoSize = true };
        ok.Click += (_, _) => Accept();
        var remove = new Button { Text = "Remove", AutoSize = true };
        remove.Click += (_, _) => RemoveSelected();
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(ok);
        buttons.Controls.Add(remove);

        var hint = new Label
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            ForeColor = Color.DimGray,
            MaximumSize = new Size(620, 0),
            Text = "CSV columns: name, x, y. Extra columns are ignored. One unit applies to the whole file. A second import appends points."
        };

        Controls.Add(_preview);
        Controls.Add(hint);
        Controls.Add(buttons);
        Controls.Add(manual);
        Controls.Add(unitRow);
        CancelButton = cancel;
    }

    private LengthUnit SelectedUnit => _unit.SelectedIndex switch
    {
        0 => LengthUnit.Micrometer,
        2 => LengthUnit.Centimeter,
        3 => LengthUnit.Meter,
        _ => LengthUnit.Millimeter
    };

    private void LoadCsv()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "CSV|*.csv|All files|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        TeachPointCsvResult parsed = TeachPointCsv.Parse(File.ReadAllText(dialog.FileName));
        if (!parsed.Success)
        {
            MessageBox.Show(this, parsed.Error ?? "Could not read the CSV file.", "Import teach points", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LengthUnit unit = SelectedUnit;
        foreach (TeachPointCsvRow row in parsed.Rows)
        {
            AddPending(row.Name, row.X, row.Y, unit);
        }
    }

    private void AddManual()
    {
        if (!TryParseLength(_x.Text, out double x) || !TryParseLength(_y.Text, out double y))
        {
            MessageBox.Show(this, "Enter numeric X and Y.", "Import teach points", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string? name = string.IsNullOrWhiteSpace(_name.Text) ? null : _name.Text.Trim();
        AddPending(name, x, y, SelectedUnit);
        _name.Clear();
        _x.Clear();
        _y.Clear();
        _name.Focus();
    }

    private void AddPending(string? name, double x, double y, LengthUnit unit)
    {
        string resolved = string.IsNullOrWhiteSpace(name) ? TeachPointCsv.NextName(_usedNames) : name.Trim();
        _usedNames.Add(resolved);
        _pending.Add(new PendingPoint(resolved, x, y, unit));
        var item = new ListViewItem(resolved);
        item.SubItems.Add(x.ToString("0.###", CultureInfo.InvariantCulture));
        item.SubItems.Add(y.ToString("0.###", CultureInfo.InvariantCulture));
        item.SubItems.Add(LengthUnits.Label(unit));
        item.Tag = _pending.Count - 1;
        _preview.Items.Add(item);
    }

    private void RemoveSelected()
    {
        if (_preview.SelectedItems.Count == 0)
        {
            return;
        }

        var selected = new HashSet<int>();
        foreach (ListViewItem item in _preview.SelectedItems)
        {
            if (item.Tag is int index)
            {
                selected.Add(index);
            }
        }

        var keep = new List<PendingPoint>();
        for (int i = 0; i < _pending.Count; i++)
        {
            if (!selected.Contains(i))
            {
                keep.Add(_pending[i]);
            }
        }

        _pending.Clear();
        _preview.Items.Clear();
        _usedNames.Clear();
        _usedNames.UnionWith(_existingNames);
        foreach (PendingPoint point in keep)
        {
            AddPending(point.Name, point.X, point.Y, point.Unit);
        }
    }

    private void Accept()
    {
        if (_pending.Count == 0)
        {
            MessageBox.Show(this, "Add at least one point.", "Import teach points", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Points = _pending.Select(static point => new ReferencePoint
        {
            Name = point.Name ?? string.Empty,
            X_mm = LengthUnits.ToMillimeters(point.X, point.Unit),
            Y_mm = LengthUnits.ToMillimeters(point.Y, point.Unit)
        }).ToList();
        DialogResult = DialogResult.OK;
        Close();
    }

    private static bool TryParseLength(string text, out double value)
    {
        string trimmed = text.Trim().Replace(',', '.');
        return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
