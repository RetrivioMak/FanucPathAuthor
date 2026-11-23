using System.Globalization;
using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor;

public sealed partial class ProjectSettingsDialog : Form
{
    private readonly ProcessDefaults _source;
    private readonly bool _showReference;

    public ProcessDefaults Settings { get; private set; }

    public ProjectSettingsDialog(ProcessDefaults current, bool showReferenceScale)
    {
        InitializeComponent();
        _source = current;
        _showReference = showReferenceScale;
        Settings = ProcessDefaultsCopy.Clone(current);
        _grid.DataError += (_, e) => e.ThrowException = false;
        _okButton.Click += (_, _) => AcceptSettings();

        AddRow("Z mm", Format(current.Z_mm));
        AddRow("W deg", Format(current.W_deg));
        AddRow("P deg", Format(current.P_deg));
        AddRow("R deg", Format(current.R_deg));
        AddGlueRow(current.GlueOn);
        AddRow("Min width mm", Format(StrokeWidths.Floor(current.MinWidth_mm)));
        if (showReferenceScale)
        {
            AddRow("Reference mm/px", FormatReference(current.ReferenceMmPerPx));
        }

    }

    private void AcceptSettings()
    {
        if (_grid.IsCurrentCellInEditMode && !_grid.EndEdit())
        {
            DialogResult = DialogResult.None;
            return;
        }

        if (!TryRead(out ProcessDefaults settings))
        {
            DialogResult = DialogResult.None;
            return;
        }

        Settings = settings;
        DialogResult = DialogResult.OK;
    }

    private bool TryRead(out ProcessDefaults settings)
    {
        settings = ProcessDefaultsCopy.Clone(_source);
        if (!TryNumber(0, -10000, 10000, out double z) ||
            !TryNumber(1, -360, 360, out double w) ||
            !TryNumber(2, -360, 360, out double p) ||
            !TryNumber(3, -360, 360, out double r) ||
            !TryNumber(5, StrokeWidths.MinMm, StrokeWidths.MaxMm, out double minWidth))
        {
            return false;
        }

        settings.Z_mm = z;
        settings.W_deg = w;
        settings.P_deg = p;
        settings.R_deg = r;
        settings.GlueOn = string.Equals(CellText(4), "On", StringComparison.Ordinal);
        settings.MinWidth_mm = StrokeWidths.Floor(minWidth);
        if (_showReference)
        {
            if (!TryNumber(6, StrokeWidths.ReferenceMin, StrokeWidths.ReferenceMax, out double reference))
            {
                return false;
            }

            settings.ReferenceMmPerPx = reference;
        }

        StrokeWidths.EnsureStored(settings);
        return true;
    }

    private bool TryNumber(int row, double minimum, double maximum, out double value)
    {
        _grid.Rows[row].ErrorText = string.Empty;
        if (!TryParse(CellText(row), out value) || value < minimum || value > maximum)
        {
            _grid.Rows[row].ErrorText = $"Enter a number from {FormatLimit(minimum)} to {FormatLimit(maximum)}";
            value = 0;
            return false;
        }

        return true;
    }

    private string CellText(int row) =>
        Convert.ToString(_grid.Rows[row].Cells[1].Value, CultureInfo.CurrentCulture) ?? string.Empty;

    private void AddRow(string name, string value)
    {
        int index = _grid.Rows.Add(name, value);
        _grid.Rows[index].Cells[0].ReadOnly = true;
    }

    private void AddGlueRow(bool glueOn)
    {
        int index = _grid.Rows.Add();
        DataGridViewRow row = _grid.Rows[index];
        row.Cells[0].Value = "Glue";
        row.Cells[0].ReadOnly = true;
        var combo = new DataGridViewComboBoxCell
        {
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            FlatStyle = FlatStyle.Flat
        };
        combo.Items.Add("On");
        combo.Items.Add("Off");
        row.Cells[1] = combo;
        row.Cells[1].Value = glueOn ? "On" : "Off";
    }

    private static string Format(double value) =>
        value.ToString("0.###", CultureInfo.CurrentCulture);

    private static string FormatReference(double value) =>
        value.ToString("0.######", CultureInfo.CurrentCulture);

    private static string FormatLimit(double value) =>
        value.ToString("0.######", CultureInfo.CurrentCulture);

    private static bool TryParse(string? text, out double value)
    {
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
        {
            return true;
        }

        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
