#nullable enable
namespace DispensePathAuthor.Import.Dxf;

partial class DxfImportDialog
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DxfImportDialog));
        _layout = new TableLayoutPanel();
        _heading = new Label();
        _limits = new Label();
        _fileLabel = new Label();
        _fileRow = new TableLayoutPanel();
        _filePath = new TextBox();
        _browseButton = new Button();
        _note = new Label();
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
        _fileRow.SuspendLayout();
        _unitRow.SuspendLayout();
        _originRow.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_originX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_originY).BeginInit();
        _buttonRow.SuspendLayout();
        SuspendLayout();
        // 
        // _layout
        // 
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Controls.Add(_heading, 0, 0);
        _layout.Controls.Add(_limits, 0, 1);
        _layout.Controls.Add(_fileLabel, 0, 2);
        _layout.Controls.Add(_fileRow, 0, 3);
        _layout.Controls.Add(_note, 0, 4);
        _layout.Controls.Add(_layerLabel, 0, 5);
        _layout.Controls.Add(_layers, 0, 6);
        _layout.Controls.Add(_unitRow, 0, 7);
        _layout.Controls.Add(_originRow, 0, 8);
        _layout.Controls.Add(_buttonRow, 0, 9);
        _layout.Dock = DockStyle.Fill;
        _layout.Location = new Point(0, 0);
        _layout.Name = "_layout";
        _layout.Padding = new Padding(12);
        _layout.RowCount = 10;
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.Size = new Size(746, 912);
        _layout.TabIndex = 0;
        // 
        // _heading
        // 
        _heading.AutoSize = true;
        _heading.Dock = DockStyle.Fill;
        _heading.Location = new Point(12, 12);
        _heading.Margin = new Padding(0, 0, 0, 4);
        _heading.Name = "_heading";
        _heading.Size = new Size(722, 32);
        _heading.TabIndex = 0;
        _heading.Text = "Limitations";
        // 
        // _limits
        // 
        _limits.AutoSize = true;
        _limits.Dock = DockStyle.Fill;
        _limits.Location = new Point(15, 48);
        _limits.MaximumSize = new Size(560, 0);
        _limits.Name = "_limits";
        _limits.Size = new Size(560, 320);
        _limits.TabIndex = 1;
        _limits.Text = resources.GetString("_limits.Text");
        // 
        // _fileLabel
        // 
        _fileLabel.AutoSize = true;
        _fileLabel.Dock = DockStyle.Fill;
        _fileLabel.Location = new Point(12, 380);
        _fileLabel.Margin = new Padding(0, 12, 0, 4);
        _fileLabel.Name = "_fileLabel";
        _fileLabel.Size = new Size(722, 32);
        _fileLabel.TabIndex = 2;
        _fileLabel.Text = "Drawing";
        // 
        // _fileRow
        // 
        _fileRow.ColumnCount = 2;
        _fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _fileRow.ColumnStyles.Add(new ColumnStyle());
        _fileRow.Controls.Add(_filePath, 0, 0);
        _fileRow.Controls.Add(_browseButton, 1, 0);
        _fileRow.Dock = DockStyle.Fill;
        _fileRow.Location = new Point(12, 416);
        _fileRow.Margin = new Padding(0);
        _fileRow.Name = "_fileRow";
        _fileRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        _fileRow.Size = new Size(722, 50);
        _fileRow.TabIndex = 3;
        // 
        // _filePath
        // 
        _filePath.Dock = DockStyle.Fill;
        _filePath.Location = new Point(3, 3);
        _filePath.Name = "_filePath";
        _filePath.ReadOnly = true;
        _filePath.Size = new Size(593, 39);
        _filePath.TabIndex = 0;
        // 
        // _browseButton
        // 
        _browseButton.AutoSize = true;
        _browseButton.Location = new Point(607, 0);
        _browseButton.Margin = new Padding(8, 0, 0, 0);
        _browseButton.Name = "_browseButton";
        _browseButton.Size = new Size(115, 42);
        _browseButton.TabIndex = 1;
        _browseButton.Text = "Browse...";
        _browseButton.Click += Browse;
        // 
        // _note
        // 
        _note.AutoSize = true;
        _note.Dock = DockStyle.Fill;
        _note.Location = new Point(12, 474);
        _note.Margin = new Padding(0, 8, 0, 0);
        _note.MaximumSize = new Size(560, 0);
        _note.Name = "_note";
        _note.Size = new Size(560, 32);
        _note.TabIndex = 4;
        _note.Visible = false;
        // 
        // _layerLabel
        // 
        _layerLabel.AutoSize = true;
        _layerLabel.Dock = DockStyle.Fill;
        _layerLabel.Location = new Point(12, 518);
        _layerLabel.Margin = new Padding(0, 12, 0, 4);
        _layerLabel.Name = "_layerLabel";
        _layerLabel.Size = new Size(722, 32);
        _layerLabel.TabIndex = 5;
        _layerLabel.Text = "Layers";
        // 
        // _layers
        // 
        _layers.CheckOnClick = true;
        _layers.Dock = DockStyle.Fill;
        _layers.IntegralHeight = false;
        _layers.Location = new Point(15, 557);
        _layers.Name = "_layers";
        _layers.Size = new Size(716, 173);
        _layers.TabIndex = 6;
        // 
        // _unitRow
        // 
        _unitRow.AutoSize = true;
        _unitRow.Controls.Add(_unitLabel);
        _unitRow.Controls.Add(_units);
        _unitRow.Dock = DockStyle.Fill;
        _unitRow.Location = new Point(12, 741);
        _unitRow.Margin = new Padding(0, 8, 0, 0);
        _unitRow.Name = "_unitRow";
        _unitRow.Size = new Size(722, 46);
        _unitRow.TabIndex = 7;
        // 
        // _unitLabel
        // 
        _unitLabel.AutoSize = true;
        _unitLabel.Location = new Point(0, 6);
        _unitLabel.Margin = new Padding(0, 6, 8, 0);
        _unitLabel.Name = "_unitLabel";
        _unitLabel.Size = new Size(68, 32);
        _unitLabel.TabIndex = 0;
        _unitLabel.Text = "Units";
        // 
        // _units
        // 
        _units.DropDownStyle = ComboBoxStyle.DropDownList;
        _units.Items.AddRange(new object[] { "µm", "mm", "cm", "m", "in", "ft" });
        _units.Location = new Point(79, 3);
        _units.Name = "_units";
        _units.Size = new Size(100, 40);
        _units.TabIndex = 1;
        // 
        // _originRow
        // 
        _originRow.AutoSize = true;
        _originRow.Controls.Add(_originLabel);
        _originRow.Controls.Add(_originXLabel);
        _originRow.Controls.Add(_originX);
        _originRow.Controls.Add(_originYLabel);
        _originRow.Controls.Add(_originY);
        _originRow.Dock = DockStyle.Fill;
        _originRow.Location = new Point(12, 795);
        _originRow.Margin = new Padding(0, 8, 0, 0);
        _originRow.Name = "_originRow";
        _originRow.Size = new Size(722, 45);
        _originRow.TabIndex = 8;
        // 
        // _originLabel
        // 
        _originLabel.AutoSize = true;
        _originLabel.Location = new Point(0, 6);
        _originLabel.Margin = new Padding(0, 6, 8, 0);
        _originLabel.Name = "_originLabel";
        _originLabel.Size = new Size(218, 32);
        _originLabel.TabIndex = 0;
        _originLabel.Text = "Drawing zero (mm)";
        // 
        // _originXLabel
        // 
        _originXLabel.AutoSize = true;
        _originXLabel.Location = new Point(234, 6);
        _originXLabel.Margin = new Padding(8, 6, 4, 0);
        _originXLabel.Name = "_originXLabel";
        _originXLabel.Size = new Size(28, 32);
        _originXLabel.TabIndex = 1;
        _originXLabel.Text = "X";
        // 
        // _originX
        // 
        _originX.DecimalPlaces = 3;
        _originX.Location = new Point(269, 3);
        _originX.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        _originX.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
        _originX.Name = "_originX";
        _originX.Size = new Size(110, 39);
        _originX.TabIndex = 2;
        // 
        // _originYLabel
        // 
        _originYLabel.AutoSize = true;
        _originYLabel.Location = new Point(394, 6);
        _originYLabel.Margin = new Padding(12, 6, 8, 0);
        _originYLabel.Name = "_originYLabel";
        _originYLabel.Size = new Size(27, 32);
        _originYLabel.TabIndex = 3;
        _originYLabel.Text = "Y";
        // 
        // _originY
        // 
        _originY.DecimalPlaces = 3;
        _originY.Location = new Point(432, 3);
        _originY.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        _originY.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
        _originY.Name = "_originY";
        _originY.Size = new Size(110, 39);
        _originY.TabIndex = 4;
        // 
        // _buttonRow
        // 
        _buttonRow.AutoSize = true;
        _buttonRow.Controls.Add(_cancelButton);
        _buttonRow.Controls.Add(_importButton);
        _buttonRow.Dock = DockStyle.Fill;
        _buttonRow.FlowDirection = FlowDirection.RightToLeft;
        _buttonRow.Location = new Point(12, 852);
        _buttonRow.Margin = new Padding(0, 12, 0, 0);
        _buttonRow.Name = "_buttonRow";
        _buttonRow.Size = new Size(722, 48);
        _buttonRow.TabIndex = 9;
        // 
        // _cancelButton
        // 
        _cancelButton.AutoSize = true;
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Location = new Point(624, 3);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(95, 42);
        _cancelButton.TabIndex = 0;
        _cancelButton.Text = "Cancel";
        // 
        // _importButton
        // 
        _importButton.AutoSize = true;
        _importButton.Enabled = false;
        _importButton.Location = new Point(523, 3);
        _importButton.Name = "_importButton";
        _importButton.Size = new Size(95, 42);
        _importButton.TabIndex = 1;
        _importButton.Text = "Import";
        _importButton.Click += AcceptImport;
        // 
        // DxfImportDialog
        // 
        AcceptButton = _importButton;
        AutoScaleDimensions = new SizeF(13F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(746, 912);
        Controls.Add(_layout);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "DxfImportDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Import DXF";
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        _fileRow.ResumeLayout(false);
        _fileRow.PerformLayout();
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
    private Label _heading = null!;
    private Label _limits = null!;
    private Label _fileLabel = null!;
    private TableLayoutPanel _fileRow = null!;
    private TextBox _filePath = null!;
    private Button _browseButton = null!;
    private Label _note = null!;
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
