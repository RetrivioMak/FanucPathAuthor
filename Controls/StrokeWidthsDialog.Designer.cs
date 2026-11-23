#nullable enable
namespace DispensePathAuthor.Controls;

partial class StrokeWidthsDialog
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
        _layout = new TableLayoutPanel();
        _caption = new Label();
        _list = new ListBox();
        _commands = new FlowLayoutPanel();
        _addButton = new Button();
        _deleteButton = new Button();
        _editButton = new Button();
        _close = new FlowLayoutPanel();
        _cancelButton = new Button();
        _okButton = new Button();
        _layout.SuspendLayout();
        _commands.SuspendLayout();
        _close.SuspendLayout();
        SuspendLayout();
        // 
        // _caption
        // 
        _caption.AutoSize = true;
        _caption.Dock = DockStyle.Fill;
        _caption.Name = "_caption";
        _caption.Size = new Size(496, 28);
        _caption.TabIndex = 0;
        _caption.Text = "Minimum";
        _caption.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _list
        // 
        _list.Dock = DockStyle.Fill;
        _list.IntegralHeight = false;
        _list.Name = "_list";
        _list.Size = new Size(496, 300);
        _list.TabIndex = 1;
        // 
        // _addButton
        // 
        _addButton.AutoSize = true;
        _addButton.Margin = new Padding(0, 8, 8, 0);
        _addButton.MinimumSize = new Size(110, 36);
        _addButton.Name = "_addButton";
        _addButton.Size = new Size(110, 36);
        _addButton.TabIndex = 0;
        _addButton.Text = "Add";
        // 
        // _deleteButton
        // 
        _deleteButton.AutoSize = true;
        _deleteButton.Margin = new Padding(0, 8, 8, 0);
        _deleteButton.MinimumSize = new Size(110, 36);
        _deleteButton.Name = "_deleteButton";
        _deleteButton.Size = new Size(110, 36);
        _deleteButton.TabIndex = 1;
        _deleteButton.Text = "Delete";
        // 
        // _editButton
        // 
        _editButton.AutoSize = true;
        _editButton.Margin = new Padding(0, 8, 0, 0);
        _editButton.MinimumSize = new Size(110, 36);
        _editButton.Name = "_editButton";
        _editButton.Size = new Size(110, 36);
        _editButton.TabIndex = 2;
        _editButton.Text = "Edit";
        // 
        // _commands
        // 
        _commands.AutoSize = true;
        _commands.Controls.Add(_addButton);
        _commands.Controls.Add(_deleteButton);
        _commands.Controls.Add(_editButton);
        _commands.Dock = DockStyle.Fill;
        _commands.Name = "_commands";
        _commands.Size = new Size(496, 44);
        _commands.TabIndex = 2;
        _commands.WrapContents = false;
        // 
        // _okButton
        // 
        _okButton.AutoSize = true;
        _okButton.DialogResult = DialogResult.OK;
        _okButton.Margin = new Padding(0, 8, 0, 0);
        _okButton.MinimumSize = new Size(110, 36);
        _okButton.Name = "_okButton";
        _okButton.Size = new Size(110, 36);
        _okButton.TabIndex = 1;
        _okButton.Text = "OK";
        // 
        // _cancelButton
        // 
        _cancelButton.AutoSize = true;
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Margin = new Padding(0, 8, 8, 0);
        _cancelButton.MinimumSize = new Size(110, 36);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(110, 36);
        _cancelButton.TabIndex = 0;
        _cancelButton.Text = "Cancel";
        // 
        // _close
        // 
        _close.AutoSize = true;
        _close.Controls.Add(_okButton);
        _close.Controls.Add(_cancelButton);
        _close.Dock = DockStyle.Fill;
        _close.FlowDirection = FlowDirection.RightToLeft;
        _close.Name = "_close";
        _close.Size = new Size(496, 44);
        _close.TabIndex = 3;
        _close.WrapContents = false;
        // 
        // _layout
        // 
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Controls.Add(_caption, 0, 0);
        _layout.Controls.Add(_list, 0, 1);
        _layout.Controls.Add(_commands, 0, 2);
        _layout.Controls.Add(_close, 0, 3);
        _layout.Dock = DockStyle.Fill;
        _layout.Name = "_layout";
        _layout.RowCount = 4;
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Size = new Size(496, 436);
        _layout.TabIndex = 0;
        // 
        // StrokeWidthsDialog
        // 
        AcceptButton = _okButton;
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(520, 460);
        Controls.Add(_layout);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(520, 360);
        Name = "StrokeWidthsDialog";
        Padding = new Padding(12);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Stroke widths";
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        _commands.ResumeLayout(false);
        _commands.PerformLayout();
        _close.ResumeLayout(false);
        _close.PerformLayout();
        ResumeLayout(false);
    }

    private TableLayoutPanel _layout = null!;
    private Label _caption = null!;
    private ListBox _list = null!;
    private FlowLayoutPanel _commands = null!;
    private Button _addButton = null!;
    private Button _deleteButton = null!;
    private Button _editButton = null!;
    private FlowLayoutPanel _close = null!;
    private Button _cancelButton = null!;
    private Button _okButton = null!;
}
