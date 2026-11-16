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
        components = new System.ComponentModel.Container();
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
        SuspendLayout();
        _stack.SuspendLayout();
        _buttons.SuspendLayout();
        _gridGroup.SuspendLayout();
        _gridLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_gridStep).BeginInit();

        _pointList.Columns.Add("Name", 120);
        _pointList.Columns.Add("X mm", 90);
        _pointList.Columns.Add("Y mm", 90);
        _pointList.Dock = DockStyle.Fill;
        _pointList.FullRowSelect = true;
        _pointList.HideSelection = false;
        _pointList.Name = "teachPointList";
        _pointList.View = View.Details;

        _importButton.AutoSize = true;
        _importButton.Name = "importButton";
        _importButton.Text = "Import points...";
        _removeButton.AutoSize = true;
        _removeButton.Enabled = false;
        _removeButton.Name = "removeButton";
        _removeButton.Text = "Remove";
        _alignButton.AutoSize = true;
        _alignButton.Enabled = false;
        _alignButton.Name = "alignButton";
        _alignButton.Text = "Align grid";
        _buttons.AutoSize = true;
        _buttons.Controls.Add(_importButton);
        _buttons.Controls.Add(_removeButton);
        _buttons.Controls.Add(_alignButton);
        _buttons.Margin = new Padding(0, 0, 0, 6);
        _buttons.Name = "teachButtons";
        _buttons.WrapContents = true;

        _hint.AutoSize = true;
        _hint.Dock = DockStyle.Fill;
        _hint.ForeColor = Color.DimGray;
        _hint.Margin = new Padding(0, 0, 0, 6);
        _hint.MaximumSize = new Size(430, 0);
        _hint.Name = "teachHint";
        _hint.Text = "Right-click a point to align the grid. With the grid on, new points snap to it. A reference or vertex still wins when the cursor is close.";

        _gridEnabled.AutoSize = true;
        _gridEnabled.Checked = true;
        _gridEnabled.CheckState = CheckState.Checked;
        _gridEnabled.Name = "gridEnabled";
        _gridEnabled.Text = "Snap to grid";
        _stepLabel.AutoSize = true;
        _stepLabel.Text = "Step mm";
        _gridStep.DecimalPlaces = 3;
        _gridStep.Increment = 0.1m;
        _gridStep.Maximum = 1000;
        _gridStep.Minimum = 0.001m;
        _gridStep.Name = "gridStep";
        _gridStep.Value = 0.2m;
        _gridStep.Width = 120;
        _originLabel.AutoSize = true;
        _originLabel.Name = "gridOriginLabel";
        _originLabel.Text = "Grid origin: (0, 0) mm";
        _gridLayout.AutoSize = true;
        _gridLayout.ColumnCount = 2;
        _gridLayout.Controls.Add(_gridEnabled, 0, 0);
        _gridLayout.Controls.Add(_stepLabel, 0, 1);
        _gridLayout.Controls.Add(_gridStep, 1, 1);
        _gridLayout.Controls.Add(_originLabel, 0, 2);
        _gridLayout.Dock = DockStyle.Fill;
        _gridLayout.Location = new Point(8, 18);
        _gridLayout.Name = "gridLayout";
        _gridLayout.Padding = new Padding(0, 4, 0, 0);
        _gridLayout.SetColumnSpan(_gridEnabled, 2);
        _gridLayout.SetColumnSpan(_originLabel, 2);

        _gridGroup.AutoSize = true;
        _gridGroup.Controls.Add(_gridLayout);
        _gridGroup.Margin = new Padding(0);
        _gridGroup.Name = "gridGroup";
        _gridGroup.Padding = new Padding(8);
        _gridGroup.Text = "Grid";

        _stack.AutoSize = true;
        _stack.ColumnCount = 1;
        _stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _stack.Controls.Add(_buttons, 0, 0);
        _stack.Controls.Add(_hint, 0, 1);
        _stack.Controls.Add(_gridGroup, 0, 2);
        _stack.Dock = DockStyle.Bottom;
        _stack.Name = "teachStack";
        _stack.Padding = new Padding(0, 8, 0, 0);
        _stack.RowCount = 3;
        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Controls.Add(_pointList);
        Controls.Add(_stack);
        Name = "TeachProjectPanel";
        Padding = new Padding(8);

        ((System.ComponentModel.ISupportInitialize)_gridStep).EndInit();
        _gridLayout.ResumeLayout(false);
        _gridLayout.PerformLayout();
        _gridGroup.ResumeLayout(false);
        _gridGroup.PerformLayout();
        _buttons.ResumeLayout(false);
        _buttons.PerformLayout();
        _stack.ResumeLayout(false);
        _stack.PerformLayout();
        ResumeLayout(false);
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
