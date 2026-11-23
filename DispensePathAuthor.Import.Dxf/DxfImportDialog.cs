using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Import.Dxf;

/// <summary>
/// Import settings for one DXF drawing. The file dialog opens from this form.
/// </summary>
public partial class DxfImportDialog : Form
{
    private DxfImporter? _importer;
    private string? _sourcePath;

    public DxfImportDialog()
    {
        InitializeComponent();
    }

    public DxfImportDialog(DxfImporter importer)
        : this()
    {
        _importer = importer;
    }

    public GeometryImportResult? Result { get; private set; }

    private void Browse(object? sender, EventArgs e)
    {
        DxfImporter? importer = _importer;
        if (importer == null)
        {
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = DxfImporter.FileFilter,
            Title = "Open DXF"
        };
        if (!string.IsNullOrEmpty(_sourcePath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(_sourcePath);
            dialog.FileName = Path.GetFileName(_sourcePath);
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            ApplySurvey(dialog.FileName, importer.Survey(dialog.FileName));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplySurvey(string path, GeometryImportSurvey survey)
    {
        _sourcePath = path;
        _filePath.Text = path;
        _layers.Items.Clear();
        _layers.Items.AddRange(survey.Layers.Cast<object>().ToArray());
        for (int i = 0; i < _layers.Items.Count; i++)
        {
            _layers.SetItemChecked(i, true);
        }

        _units.SelectedIndex = (int)survey.SuggestedUnit;
        _note.Text = survey.Note ?? string.Empty;
        _note.Visible = !string.IsNullOrWhiteSpace(survey.Note);
        _importButton.Enabled = _layers.Items.Count > 0;
    }

    private void AcceptImport(object? sender, EventArgs e)
    {
        DxfImporter? importer = _importer;
        if (importer == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_sourcePath))
        {
            MessageBox.Show(this, "Choose a DXF file.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

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

        try
        {
            Result = importer.Import(
                _sourcePath,
                new GeometryImportOptions
                {
                    Layers = names,
                    Unit = (DrawingLengthUnit)_units.SelectedIndex,
                    OriginX_mm = (double)_originX.Value,
                    OriginY_mm = (double)_originY.Value
                });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        DialogResult = DialogResult.OK;
    }
}
