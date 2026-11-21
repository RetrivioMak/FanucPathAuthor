using System.Globalization;
using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor;

public partial class TeachPointImportDialog : Form
{
    private readonly record struct PendingPoint(string? Name, double X, double Y, LengthUnit Unit);

    private readonly HashSet<string> _existingNames;
    private readonly HashSet<string> _usedNames;
    private readonly List<PendingPoint> _pending = new();

    public IReadOnlyList<ReferencePoint> Points { get; private set; } = Array.Empty<ReferencePoint>();

    public TeachPointImportDialog(IEnumerable<string> existingNames)
    {
        _existingNames = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
        _usedNames = new HashSet<string>(_existingNames, StringComparer.OrdinalIgnoreCase);
        InitializeComponent();
        EnsurePreviewColumns();
        _unit.SelectedIndex = 1;
        _csvButton.Click += (_, _) => LoadCsv();
        _addButton.Click += (_, _) => AddManual();
        _importButton.Click += (_, _) => Accept();
        _removeButton.Click += (_, _) => RemoveSelected();
        _name.KeyDown += AddOnEnter;
        _x.KeyDown += AddOnEnter;
        _y.KeyDown += AddOnEnter;
    }

    private void EnsurePreviewColumns()
    {
        if (_preview.Columns.Count > 0)
        {
            return;
        }

        _preview.View = View.Details;
        _preview.FullRowSelect = true;
        _preview.GridLines = true;
        _preview.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _preview.HideSelection = false;
        _preview.Columns.Add("Name", 220);
        _preview.Columns.Add("X", 140);
        _preview.Columns.Add("Y", 140);
        _preview.Columns.Add("Unit", 80);
    }

    private void AddOnEnter(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.Handled = true;
        e.SuppressKeyPress = true;
        AddManual();
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
        item.Selected = true;
        item.EnsureVisible();
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
