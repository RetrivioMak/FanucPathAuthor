using DispensePathAuthor.Services;

namespace DispensePathAuthor;

public partial class GeometryImportDialog : Form
{
    private static readonly string[] UnitLabels = ["µm", "mm", "cm", "m", "in", "ft"];

    public IReadOnlyList<string> SelectedLayers { get; private set; } = Array.Empty<string>();

    public DrawingLengthUnit Unit { get; private set; } = DrawingLengthUnit.Millimeter;

    public double OriginX_mm { get; private set; }

    public double OriginY_mm { get; private set; }

    public GeometryImportDialog(GeometryImportSurvey survey)
    {
        InitializeComponent();
        _units.Items.AddRange(UnitLabels);
        _units.SelectedIndex = (int)survey.SuggestedUnit;
        _layers.Items.AddRange(survey.Layers.Cast<object>().ToArray());
        for (int i = 0; i < _layers.Items.Count; i++)
        {
            _layers.SetItemChecked(i, true);
        }

        if (!string.IsNullOrWhiteSpace(survey.Note))
        {
            _hint.Text = _hint.Text + " " + survey.Note;
        }

        if (_layers.Items.Count == 0)
        {
            _importButton.Enabled = false;
        }

        _importButton.Click += AcceptImport;
    }

    private void AcceptImport(object? sender, EventArgs e)
    {
        var names = new List<string>();
        foreach (object item in _layers.CheckedItems)
        {
            names.Add((string)item);
        }

        if (names.Count == 0)
        {
            MessageBox.Show(this, "Select at least one layer.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SelectedLayers = names;
        Unit = (DrawingLengthUnit)_units.SelectedIndex;
        OriginX_mm = (double)_originX.Value;
        OriginY_mm = (double)_originY.Value;
        DialogResult = DialogResult.OK;
    }
}
