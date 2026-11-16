#nullable enable
namespace DispensePathAuthor;

partial class TeachPointImportDialog
{
    private System.ComponentModel.IContainer? components;

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
        _unit = new ComboBox();
        _name = new TextBox();
        _x = new TextBox();
        _y = new TextBox();
        _preview = new ListView();
        _unitRow = new FlowLayoutPanel();
        _unitLabel = new Label();
        _csvButton = new Button();
        _manualRow = new FlowLayoutPanel();
        _nameLabel = new Label();
        _xLabel = new Label();
        _yLabel = new Label();
        _addButton = new Button();
        _buttonRow = new FlowLayoutPanel();
        _cancelButton = new Button();
        _importButton = new Button();
        _removeButton = new Button();
        _hintLabel = new Label();
        SuspendLayout();

        _unit.DropDownStyle = ComboBoxStyle.DropDownList;
        _unit.Name = "unitCombo";
        _unit.Width = 120;
        _unit.Items.AddRange(new object[] { "?m", "mm", "cm", "m" });
        _unit.SelectedIndex = 1;

        _name.Name = "nameBox";
        _name.Width = 140;
        _x.Name = "xBox";
        _x.Width = 100;
        _y.Name = "yBox";
        _y.Width = 100;

        _preview.Columns.Add("Name", 160);
        _preview.Columns.Add("X", 100);
        _preview.Columns.Add("Y", 100);
        _preview.Columns.Add("Unit", 60);
        _preview.Dock = DockStyle.Fill;
        _preview.FullRowSelect = true;
        _preview.HideSelection = false;
        _preview.Name = "previewList";
        _preview.View = View.Details;

        _unitLabel.AutoSize = true;
        _unitLabel.Padding = new Padding(0, 6, 8, 0);
        _unitLabel.Text = "Unit";
        _csvButton.AutoSize = true;
        _csvButton.Name = "csvButton";
        _csvButton.Text = "Load CSV...";
        _unitRow.AutoSize = true;
        _unitRow.Controls.Add(_unitLabel);
        _unitRow.Controls.Add(_unit);
        _unitRow.Controls.Add(_csvButton);
        _unitRow.Dock = DockStyle.Top;
        _unitRow.Name = "unitRow";
        _unitRow.Padding = new Padding(0, 0, 0, 8);

        _nameLabel.AutoSize = true;
        _nameLabel.Padding = new Padding(0, 6, 4, 0);
        _nameLabel.Text = "Name";
        _xLabel.AutoSize = true;
        _xLabel.Padding = new Padding(8, 6, 4, 0);
        _xLabel.Text = "X";
        _yLabel.AutoSize = true;
        _yLabel.Padding = new Padding(8, 6, 4, 0);
        _yLabel.Text = "Y";
        _addButton.AutoSize = true;
        _addButton.Name = "addButton";
        _addButton.Text = "Add";
        _manualRow.AutoSize = true;
        _manualRow.Controls.Add(_nameLabel);
        _manualRow.Controls.Add(_name);
        _manualRow.Controls.Add(_xLabel);
        _manualRow.Controls.Add(_x);
        _manualRow.Controls.Add(_yLabel);
        _manualRow.Controls.Add(_y);
        _manualRow.Controls.Add(_addButton);
        _manualRow.Dock = DockStyle.Top;
        _manualRow.Name = "manualRow";
        _manualRow.Padding = new Padding(0, 0, 0, 8);

        _cancelButton.AutoSize = true;
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Name = "cancelButton";
        _cancelButton.Text = "Cancel";
        _importButton.AutoSize = true;
        _importButton.Name = "importButton";
        _importButton.Text = "Import";
        _removeButton.AutoSize = true;
        _removeButton.Name = "removeButton";
        _removeButton.Text = "Remove";
        _buttonRow.AutoSize = true;
        _buttonRow.Controls.Add(_cancelButton);
        _buttonRow.Controls.Add(_importButton);
        _buttonRow.Controls.Add(_removeButton);
        _buttonRow.Dock = DockStyle.Bottom;
        _buttonRow.FlowDirection = FlowDirection.RightToLeft;
        _buttonRow.Name = "buttonRow";
        _buttonRow.Padding = new Padding(0, 8, 0, 0);

        _hintLabel.AutoSize = true;
        _hintLabel.Dock = DockStyle.Bottom;
        _hintLabel.ForeColor = Color.DimGray;
        _hintLabel.MaximumSize = new Size(620, 0);
        _hintLabel.Name = "hintLabel";
        _hintLabel.Text = "CSV columns: name, x, y. Extra columns are ignored. One unit applies to the whole file. A second import appends points.";

        AcceptButton = _importButton;
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(640, 480);
        Controls.Add(_preview);
        Controls.Add(_hintLabel);
        Controls.Add(_buttonRow);
        Controls.Add(_manualRow);
        Controls.Add(_unitRow);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "TeachPointImportDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Import teach points";
        ResumeLayout(false);
        PerformLayout();
    }

    private ComboBox _unit = null!;
    private TextBox _name = null!;
    private TextBox _x = null!;
    private TextBox _y = null!;
    private ListView _preview = null!;
    private FlowLayoutPanel _unitRow = null!;
    private Label _unitLabel = null!;
    private Button _csvButton = null!;
    private FlowLayoutPanel _manualRow = null!;
    private Label _nameLabel = null!;
    private Label _xLabel = null!;
    private Label _yLabel = null!;
    private Button _addButton = null!;
    private FlowLayoutPanel _buttonRow = null!;
    private Button _cancelButton = null!;
    private Button _importButton = null!;
    private Button _removeButton = null!;
    private Label _hintLabel = null!;
}
