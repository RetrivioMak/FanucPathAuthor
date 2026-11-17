#nullable enable
namespace DispensePathAuthor;

partial class GeometryImportDialog
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
        _layout = new TableLayoutPanel();
        _hint = new Label();
        _layerLabel = new Label();
        _layers = new CheckedListBox();
        _unitRow = new FlowLayoutPanel();
        _unitLabel = new Label();
        _units = new ComboBox();
        _originRow = new FlowLayoutPanel();
        _originLabel = new Label();
        _originXLabel = new Label();
        _originX = new NumericUpDown();
        _originYLabel = new Label();
        _originY = new NumericUpDown();
        _buttonRow = new FlowLayoutPanel();
        _cancelButton = new Button();
        _importButton = new Button();
        _layout.SuspendLayout();
        _unitRow.SuspendLayout();
        _originRow.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_originX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_originY).BeginInit();
        _buttonRow.SuspendLayout();
        SuspendLayout();

        _hint.AutoSize = true;
        _hint.Dock = DockStyle.Fill;
        _hint.MaximumSize = new Size(520, 0);
        _hint.Name = "hintLabel";
        _hint.Text = "Appends lines, arcs, circles, and points to the open Teach project. Text, dimensions, and hatches are skipped.";

        _layerLabel.AutoSize = true;
        _layerLabel.Dock = DockStyle.Fill;
        _layerLabel.Name = "layerLabel";
        _layerLabel.Text = "Layers";

        _layers.CheckOnClick = true;
        _layers.Dock = DockStyle.Fill;
        _layers.IntegralHeight = false;
        _layers.Name = "layerList";

        _unitLabel.AutoSize = true;
        _unitLabel.Margin = new Padding(0, 6, 8, 0);
        _unitLabel.Name = "unitLabel";
        _unitLabel.Text = "Units";
        _units.DropDownStyle = ComboBoxStyle.DropDownList;
        _units.Name = "unitCombo";
        _units.Width = 100;
        _unitRow.AutoSize = true;
        _unitRow.Controls.Add(_unitLabel);
        _unitRow.Controls.Add(_units);
        _unitRow.Dock = DockStyle.Fill;
        _unitRow.Name = "unitRow";

        _originLabel.AutoSize = true;
        _originLabel.Margin = new Padding(0, 6, 8, 0);
        _originLabel.Name = "originLabel";
        _originLabel.Text = "Drawing zero (mm)";
        _originXLabel.AutoSize = true;
        _originXLabel.Margin = new Padding(8, 6, 4, 0);
        _originXLabel.Name = "originXLabel";
        _originXLabel.Text = "X";
        _originX.DecimalPlaces = 3;
        _originX.Maximum = 1000000;
        _originX.Minimum = -1000000;
        _originX.Name = "originX";
        _originX.Width = 110;
        _originYLabel.AutoSize = true;
        _originYLabel.Margin = new Padding(12, 6, 8, 0);
        _originYLabel.Name = "originYLabel";
        _originYLabel.Text = "Y";
        _originY.DecimalPlaces = 3;
        _originY.Maximum = 1000000;
        _originY.Minimum = -1000000;
        _originY.Name = "originY";
        _originY.Width = 110;
        _originRow.AutoSize = true;
        _originRow.Controls.Add(_originLabel);
        _originRow.Controls.Add(_originXLabel);
        _originRow.Controls.Add(_originX);
        _originRow.Controls.Add(_originYLabel);
        _originRow.Controls.Add(_originY);
        _originRow.Dock = DockStyle.Fill;
        _originRow.Name = "originRow";

        _cancelButton.AutoSize = true;
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Name = "cancelButton";
        _cancelButton.Text = "Cancel";
        _importButton.AutoSize = true;
        _importButton.Name = "importButton";
        _importButton.Text = "Import";
        _buttonRow.AutoSize = true;
        _buttonRow.Controls.Add(_cancelButton);
        _buttonRow.Controls.Add(_importButton);
        _buttonRow.Dock = DockStyle.Fill;
        _buttonRow.FlowDirection = FlowDirection.RightToLeft;
        _buttonRow.Name = "buttonRow";

        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Controls.Add(_hint, 0, 0);
        _layout.Controls.Add(_layerLabel, 0, 1);
        _layout.Controls.Add(_layers, 0, 2);
        _layout.Controls.Add(_unitRow, 0, 3);
        _layout.Controls.Add(_originRow, 0, 4);
        _layout.Controls.Add(_buttonRow, 0, 5);
        _layout.Dock = DockStyle.Fill;
        _layout.Name = "importLayout";
        _layout.Padding = new Padding(12);
        _layout.RowCount = 6;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AcceptButton = _importButton;
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(460, 480);
        Controls.Add(_layout);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "GeometryImportDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Import DXF";
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        _unitRow.ResumeLayout(false);
        _unitRow.PerformLayout();
        _originRow.ResumeLayout(false);
        _originRow.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_originX).EndInit();
        ((System.ComponentModel.ISupportInitialize)_originY).EndInit();
        _buttonRow.ResumeLayout(false);
        _buttonRow.PerformLayout();
        ResumeLayout(false);
    }

    private TableLayoutPanel _layout = null!;
    private Label _hint = null!;
    private Label _layerLabel = null!;
    private CheckedListBox _layers = null!;
    private FlowLayoutPanel _unitRow = null!;
    private Label _unitLabel = null!;
    private ComboBox _units = null!;
    private FlowLayoutPanel _originRow = null!;
    private Label _originLabel = null!;
    private Label _originXLabel = null!;
    private NumericUpDown _originX = null!;
    private Label _originYLabel = null!;
    private NumericUpDown _originY = null!;
    private FlowLayoutPanel _buttonRow = null!;
    private Button _cancelButton = null!;
    private Button _importButton = null!;
}
