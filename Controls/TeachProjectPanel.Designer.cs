#nullable enable
namespace DispensePathAuthor.Controls;

partial class TeachProjectPanel
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
        _pointList = new ListView();
        _stack = new TableLayoutPanel();
        _buttons = new FlowLayoutPanel();
        _importButton = new Button();
        _removeButton = new Button();
        _alignButton = new Button();
        _hint = new Label();
        _stack.SuspendLayout();
        _buttons.SuspendLayout();
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
        _stack.Dock = DockStyle.Bottom;
        _stack.Location = new Point(8, 367);
        _stack.Name = "_stack";
        _stack.Padding = new Padding(0, 8, 0, 0);
        _stack.RowCount = 2;
        _stack.RowStyles.Add(new RowStyle());
        _stack.RowStyles.Add(new RowStyle());
        _stack.Size = new Size(468, 180);
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
        _hint.Size = new Size(430, 96);
        _hint.TabIndex = 1;
        _hint.Text = "The arrow marks a point with a dashed circle, and a line, arc, or circle with color. Right-click a point, including the green frame origin, and choose Set as grid origin. Scrollbars and right-drag on empty space move the view. Snp snaps to points and the grid. Grid step and Show are on the toolbar and in View.";
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
}
