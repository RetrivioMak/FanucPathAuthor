#nullable enable
namespace DispensePathAuthor.Controls;

partial class ValuePromptDialog
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
        _layout = new TableLayoutPanel();
        _caption = new Label();
        _input = new NumericUpDown();
        _buttons = new FlowLayoutPanel();
        _cancelButton = new Button();
        _okButton = new Button();
        _layout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_input).BeginInit();
        _buttons.SuspendLayout();
        SuspendLayout();
        // 
        // _caption
        // 
        _caption.AutoSize = true;
        _caption.Dock = DockStyle.Fill;
        _caption.Margin = new Padding(0, 0, 0, 8);
        _caption.Name = "_caption";
        _caption.Size = new Size(448, 15);
        _caption.TabIndex = 0;
        _caption.Text = "Value";
        // 
        // _input
        // 
        _input.DecimalPlaces = 3;
        _input.Dock = DockStyle.Fill;
        _input.Increment = 0.1m;
        _input.Location = new Point(0, 23);
        _input.Margin = new Padding(0, 0, 0, 16);
        _input.Maximum = 1000000m;
        _input.Minimum = 0m;
        _input.Name = "_input";
        _input.Size = new Size(448, 23);
        _input.TabIndex = 1;
        _input.TextAlign = HorizontalAlignment.Left;
        // 
        // _cancelButton
        // 
        _cancelButton.AutoSize = true;
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Margin = new Padding(0, 0, 8, 0);
        _cancelButton.MinimumSize = new Size(110, 36);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(110, 36);
        _cancelButton.TabIndex = 0;
        _cancelButton.Text = "Cancel";
        // 
        // _okButton
        // 
        _okButton.AutoSize = true;
        _okButton.DialogResult = DialogResult.OK;
        _okButton.Margin = new Padding(0);
        _okButton.MinimumSize = new Size(110, 36);
        _okButton.Name = "_okButton";
        _okButton.Size = new Size(110, 36);
        _okButton.TabIndex = 1;
        _okButton.Text = "OK";
        // 
        // _buttons
        // 
        _buttons.AutoSize = true;
        _buttons.Controls.Add(_okButton);
        _buttons.Controls.Add(_cancelButton);
        _buttons.Dock = DockStyle.Fill;
        _buttons.FlowDirection = FlowDirection.RightToLeft;
        _buttons.Margin = new Padding(0);
        _buttons.Name = "_buttons";
        _buttons.Size = new Size(448, 36);
        _buttons.TabIndex = 2;
        _buttons.WrapContents = false;
        // 
        // _layout
        // 
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Controls.Add(_caption, 0, 0);
        _layout.Controls.Add(_input, 0, 1);
        _layout.Controls.Add(_buttons, 0, 2);
        _layout.Dock = DockStyle.Fill;
        _layout.Name = "_layout";
        _layout.RowCount = 3;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Size = new Size(448, 148);
        _layout.TabIndex = 0;
        // 
        // ValuePromptDialog
        // 
        AcceptButton = _okButton;
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(480, 180);
        Controls.Add(_layout);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(480, 180);
        Name = "ValuePromptDialog";
        Padding = new Padding(16);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Edit value";
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_input).EndInit();
        _buttons.ResumeLayout(false);
        _buttons.PerformLayout();
        ResumeLayout(false);
    }

    private TableLayoutPanel _layout = null!;
    private Label _caption = null!;
    private NumericUpDown _input = null!;
    private FlowLayoutPanel _buttons = null!;
    private Button _cancelButton = null!;
    private Button _okButton = null!;
}
