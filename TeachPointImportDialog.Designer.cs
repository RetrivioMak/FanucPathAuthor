#nullable enable
namespace DispensePathAuthor;

partial class TeachPointImportDialog
{
    private System.ComponentModel.IContainer? components = null;

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
        _unitRow.SuspendLayout();
        _manualRow.SuspendLayout();
        _buttonRow.SuspendLayout();
        SuspendLayout();
        // 
        // _unit
        // 
        _unit.DropDownStyle = ComboBoxStyle.DropDownList;
        _unit.Items.AddRange(new object[] { "µm", "mm", "cm", "m" });
        _unit.Location = new Point(75, 3);
        _unit.Name = "_unit";
        _unit.Size = new Size(120, 40);
        _unit.TabIndex = 1;
        // 
        // _name
        // 
        _name.Location = new Point(91, 3);
        _name.Name = "_name";
        _name.Size = new Size(140, 39);
        _name.TabIndex = 1;
        // 
        // _x
        // 
        _x.Location = new Point(283, 3);
        _x.Name = "_x";
        _x.Size = new Size(100, 39);
        _x.TabIndex = 3;
        // 
        // _y
        // 
        _y.Location = new Point(434, 3);
        _y.Name = "_y";
        _y.Size = new Size(100, 39);
        _y.TabIndex = 5;
        // 
        // _preview
        // 
        _preview.Dock = DockStyle.Fill;
        _preview.FullRowSelect = true;
        _preview.GridLines = true;
        _preview.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _preview.Location = new Point(0, 112);
        _preview.Name = "_preview";
        _preview.Size = new Size(1166, 576);
        _preview.TabIndex = 0;
        _preview.UseCompatibleStateImageBehavior = false;
        _preview.View = View.Details;
        // 
        // _unitRow
        // 
        _unitRow.AutoSize = true;
        _unitRow.Controls.Add(_unitLabel);
        _unitRow.Controls.Add(_unit);
        _unitRow.Controls.Add(_csvButton);
        _unitRow.Dock = DockStyle.Top;
        _unitRow.Location = new Point(0, 0);
        _unitRow.Name = "_unitRow";
        _unitRow.Padding = new Padding(0, 0, 0, 8);
        _unitRow.Size = new Size(1166, 56);
        _unitRow.TabIndex = 4;
        // 
        // _unitLabel
        // 
        _unitLabel.AutoSize = true;
        _unitLabel.Location = new Point(3, 0);
        _unitLabel.Name = "_unitLabel";
        _unitLabel.Padding = new Padding(0, 6, 8, 0);
        _unitLabel.Size = new Size(66, 38);
        _unitLabel.TabIndex = 0;
        _unitLabel.Text = "Unit";
        // 
        // _csvButton
        // 
        _csvButton.AutoSize = true;
        _csvButton.Location = new Point(201, 3);
        _csvButton.Name = "_csvButton";
        _csvButton.Size = new Size(140, 42);
        _csvButton.TabIndex = 2;
        _csvButton.Text = "Load CSV...";
        // 
        // _manualRow
        // 
        _manualRow.AutoSize = true;
        _manualRow.Controls.Add(_nameLabel);
        _manualRow.Controls.Add(_name);
        _manualRow.Controls.Add(_xLabel);
        _manualRow.Controls.Add(_x);
        _manualRow.Controls.Add(_yLabel);
        _manualRow.Controls.Add(_y);
        _manualRow.Controls.Add(_addButton);
        _manualRow.Dock = DockStyle.Top;
        _manualRow.Location = new Point(0, 56);
        _manualRow.Name = "_manualRow";
        _manualRow.Padding = new Padding(0, 0, 0, 8);
        _manualRow.Size = new Size(1166, 56);
        _manualRow.TabIndex = 3;
        // 
        // _nameLabel
        // 
        _nameLabel.AutoSize = true;
        _nameLabel.Location = new Point(3, 0);
        _nameLabel.Name = "_nameLabel";
        _nameLabel.Padding = new Padding(0, 6, 4, 0);
        _nameLabel.Size = new Size(82, 38);
        _nameLabel.TabIndex = 0;
        _nameLabel.Text = "Name";
        // 
        // _xLabel
        // 
        _xLabel.AutoSize = true;
        _xLabel.Location = new Point(237, 0);
        _xLabel.Name = "_xLabel";
        _xLabel.Padding = new Padding(8, 6, 4, 0);
        _xLabel.Size = new Size(40, 38);
        _xLabel.TabIndex = 2;
        _xLabel.Text = "X";
        // 
        // _yLabel
        // 
        _yLabel.AutoSize = true;
        _yLabel.Location = new Point(389, 0);
        _yLabel.Name = "_yLabel";
        _yLabel.Padding = new Padding(8, 6, 4, 0);
        _yLabel.Size = new Size(39, 38);
        _yLabel.TabIndex = 4;
        _yLabel.Text = "Y";
        // 
        // _addButton
        // 
        _addButton.AutoSize = true;
        _addButton.Location = new Point(540, 3);
        _addButton.Name = "_addButton";
        _addButton.Size = new Size(75, 42);
        _addButton.TabIndex = 6;
        _addButton.Text = "Add";
        // 
        // _buttonRow
        // 
        _buttonRow.AutoSize = true;
        _buttonRow.Controls.Add(_cancelButton);
        _buttonRow.Controls.Add(_importButton);
        _buttonRow.Controls.Add(_removeButton);
        _buttonRow.Dock = DockStyle.Bottom;
        _buttonRow.FlowDirection = FlowDirection.RightToLeft;
        _buttonRow.Location = new Point(0, 784);
        _buttonRow.Name = "_buttonRow";
        _buttonRow.Padding = new Padding(0, 8, 0, 0);
        _buttonRow.Size = new Size(1166, 56);
        _buttonRow.TabIndex = 2;
        // 
        // _cancelButton
        // 
        _cancelButton.AutoSize = true;
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Location = new Point(1068, 11);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(95, 42);
        _cancelButton.TabIndex = 0;
        _cancelButton.Text = "Cancel";
        // 
        // _importButton
        // 
        _importButton.AutoSize = true;
        _importButton.Location = new Point(967, 11);
        _importButton.Name = "_importButton";
        _importButton.Size = new Size(95, 42);
        _importButton.TabIndex = 1;
        _importButton.Text = "Import";
        // 
        // _removeButton
        // 
        _removeButton.AutoSize = true;
        _removeButton.Location = new Point(851, 11);
        _removeButton.Name = "_removeButton";
        _removeButton.Size = new Size(110, 42);
        _removeButton.TabIndex = 2;
        _removeButton.Text = "Remove";
        // 
        // _hintLabel
        // 
        _hintLabel.AutoSize = true;
        _hintLabel.Dock = DockStyle.Bottom;
        _hintLabel.ForeColor = Color.DimGray;
        _hintLabel.Location = new Point(0, 688);
        _hintLabel.MaximumSize = new Size(620, 0);
        _hintLabel.Name = "_hintLabel";
        _hintLabel.Size = new Size(620, 96);
        _hintLabel.TabIndex = 1;
        _hintLabel.Text = "CSV columns: name, x, y. Extra columns are ignored. One unit applies to the whole file. A second import appends points.";
        // 
        // TeachPointImportDialog
        // 
        AutoScaleDimensions = new SizeF(13F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(1166, 840);
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
        _unitRow.ResumeLayout(false);
        _unitRow.PerformLayout();
        _manualRow.ResumeLayout(false);
        _manualRow.PerformLayout();
        _buttonRow.ResumeLayout(false);
        _buttonRow.PerformLayout();
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
