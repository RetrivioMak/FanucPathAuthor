#nullable enable
namespace DispensePathAuthor.Controls;

partial class TeachProjectPanel
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
        _pointList = new ListView();
        _stack = new TableLayoutPanel();
        _buttons = new FlowLayoutPanel();
        _importButton = new Button();
        _removeButton = new Button();
        _alignButton = new Button();
        _hint = new Label();
        _gridGroup = new GroupBox();
        _gridLayout = new TableLayoutPanel();
        _gridEnabled = new CheckBox();
        _stepLabel = new Label();
        _gridStep = new NumericUpDown();
        _originLabel = new Label();
        _stack.SuspendLayout();
        _buttons.SuspendLayout();
        _gridGroup.SuspendLayout();
        _gridLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_gridStep).BeginInit();
        SuspendLayout();
        // 
        // _pointList
        // 
        _pointList.Columns.Add("Name", 120);
        _pointList.Columns.Add("X mm", 90);
        _pointList.Columns.Add("Y mm", 90);
        _pointList.Dock = DockStyle.Fill;
        _pointList.FullRowSelect = true;
        _pointList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _pointList.HideSelection = false;
        _pointList.Name = "_pointList";
        _pointList.TabIndex = 0;
        _pointList.UseCompatibleStateImageBehavior = false;
        _pointList.View = View.Details;
        // 
        // _stack
        // 
        _stack.AutoSize = true;
        _stack.ColumnCount = 1;
        _stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _stack.Controls.Add(_buttons, 0, 0);
        _stack.Controls.Add(_hint, 0, 1);
        _stack.Controls.Add(_gridGroup, 0, 2);
        _stack.Dock = DockStyle.Bottom;
        _stack.Location = new Point(8, 367);
        _stack.Name = "_stack";
        _stack.Padding = new Padding(0, 8, 0, 0);
        _stack.RowCount = 3;
        _stack.RowStyles.Add(new RowStyle());
        _stack.RowStyles.Add(new RowStyle());
        _stack.RowStyles.Add(new RowStyle());
        _stack.Size = new Size(468, 308);
        _stack.TabIndex = 1;
        // 
        // _buttons
        // 
        _buttons.AutoSize = true;
        _buttons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _buttons.Controls.Add(_importButton);
        _buttons.Controls.Add(_removeButton);
        _buttons.Controls.Add(_alignButton);
        _buttons.Location = new Point(0, 8);
        _buttons.Margin = new Padding(0, 0, 0, 6);
        _buttons.Name = "_buttons";
        _buttons.TabIndex = 0;
        _buttons.WrapContents = true;
        // 
        // _importButton
        // 
        _importButton.AutoSize = true;
        _importButton.Location = new Point(3, 3);
        _importButton.Name = "_importButton";
        _importButton.Size = new Size(183, 42);
        _importButton.TabIndex = 0;
        _importButton.Text = "Import points...";
        // 
        // _removeButton
        // 
        _removeButton.AutoSize = true;
        _removeButton.Enabled = false;
        _removeButton.Location = new Point(192, 3);
        _removeButton.Name = "_removeButton";
        _removeButton.Size = new Size(110, 42);
        _removeButton.TabIndex = 1;
        _removeButton.Text = "Remove";
        // 
        // _alignButton
        // 
        _alignButton.AutoSize = true;
        _alignButton.Enabled = false;
        _alignButton.Location = new Point(308, 3);
        _alignButton.Name = "_alignButton";
        _alignButton.Size = new Size(128, 42);
        _alignButton.TabIndex = 2;
        _alignButton.Text = "Align grid";
        // 
        // _hint
        // 
        _hint.AutoSize = true;
        _hint.Dock = DockStyle.Fill;
        _hint.ForeColor = Color.DimGray;
        _hint.Location = new Point(0, 62);
        _hint.Margin = new Padding(0, 0, 0, 6);
        _hint.MaximumSize = new Size(430, 0);
        _hint.Name = "_hint";
        _hint.Size = new Size(430, 128);
        _hint.TabIndex = 1;
        _hint.Text = "Right-click a point to align the grid. With the grid on, new points snap to it. A reference or vertex still wins when the cursor is close.";
        // 
        // _gridGroup
        // 
        _gridGroup.AutoSize = true;
        _gridGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _gridGroup.Controls.Add(_gridLayout);
        _gridGroup.Dock = DockStyle.Fill;
        _gridGroup.Margin = new Padding(0);
        _gridGroup.Name = "_gridGroup";
        _gridGroup.Padding = new Padding(8);
        _gridGroup.TabIndex = 2;
        _gridGroup.TabStop = false;
        _gridGroup.Text = "Grid";
        // 
        // _gridLayout
        // 
        _gridLayout.AutoSize = true;
        _gridLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _gridLayout.ColumnCount = 2;
        _gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _gridLayout.Controls.Add(_gridEnabled, 0, 0);
        _gridLayout.Controls.Add(_stepLabel, 0, 1);
        _gridLayout.Controls.Add(_gridStep, 1, 1);
        _gridLayout.Controls.Add(_originLabel, 0, 2);
        _gridLayout.Dock = DockStyle.Top;
        _gridLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        _gridLayout.Name = "_gridLayout";
        _gridLayout.Padding = new Padding(0, 4, 0, 0);
        _gridLayout.RowCount = 3;
        _gridLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _gridLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _gridLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _gridLayout.TabIndex = 0;
        // 
        // _gridEnabled
        // 
        _gridEnabled.Anchor = AnchorStyles.Left;
        _gridEnabled.AutoSize = true;
        _gridEnabled.Checked = true;
        _gridEnabled.CheckState = CheckState.Checked;
        _gridLayout.SetColumnSpan(_gridEnabled, 2);
        _gridEnabled.Margin = new Padding(0, 4, 0, 4);
        _gridEnabled.Name = "_gridEnabled";
        _gridEnabled.TabIndex = 0;
        _gridEnabled.Text = "Snap to grid";
        // 
        // _stepLabel
        // 
        _stepLabel.Anchor = AnchorStyles.Left;
        _stepLabel.AutoSize = true;
        _stepLabel.Margin = new Padding(0, 4, 12, 4);
        _stepLabel.Name = "_stepLabel";
        _stepLabel.TabIndex = 1;
        _stepLabel.Text = "Step mm";
        // 
        // _gridStep
        // 
        _gridStep.Anchor = AnchorStyles.Left;
        _gridStep.DecimalPlaces = 3;
        _gridStep.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
        _gridStep.Margin = new Padding(0, 2, 0, 2);
        _gridStep.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        _gridStep.Minimum = new decimal(new int[] { 1, 0, 0, 196608 });
        _gridStep.Name = "_gridStep";
        _gridStep.TabIndex = 2;
        _gridStep.Value = new decimal(new int[] { 2, 0, 0, 65536 });
        _gridStep.Width = 120;
        // 
        // _originLabel
        // 
        _originLabel.Anchor = AnchorStyles.Left;
        _originLabel.AutoSize = true;
        _gridLayout.SetColumnSpan(_originLabel, 2);
        _originLabel.Margin = new Padding(0, 4, 0, 0);
        _originLabel.MaximumSize = new Size(560, 0);
        _originLabel.Name = "_originLabel";
        _originLabel.TabIndex = 3;
        _originLabel.Text = "Grid origin: (0, 0) mm";
        // 
        // TeachProjectPanel
        // 
        Controls.Add(_pointList);
        Controls.Add(_stack);
        Name = "TeachProjectPanel";
        Padding = new Padding(8);
        Size = new Size(484, 683);
        _stack.ResumeLayout(false);
        _stack.PerformLayout();
        _buttons.ResumeLayout(false);
        _buttons.PerformLayout();
        _gridGroup.ResumeLayout(false);
        _gridGroup.PerformLayout();
        _gridLayout.ResumeLayout(false);
        _gridLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_gridStep).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private ListView _pointList = null!;
    private TableLayoutPanel _stack = null!;
    private FlowLayoutPanel _buttons = null!;
    private Button _importButton = null!;
    private Button _removeButton = null!;
    private Button _alignButton = null!;
    private Label _hint = null!;
    private GroupBox _gridGroup = null!;
    private TableLayoutPanel _gridLayout = null!;
    private CheckBox _gridEnabled = null!;
    private Label _stepLabel = null!;
    private NumericUpDown _gridStep = null!;
    private Label _originLabel = null!;
}
