#nullable enable

namespace DispensePathAuthor.Export.Xyz;

partial class XyzSettingsPanel
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
        components = new System.ComponentModel.Container();
        _layout = new TableLayoutPanel();
        _zHeightLabel = new Label();
        _zHeight = new NumericUpDown();
        _valveLabel = new Label();
        _valveOutput = new NumericUpDown();
        _pointSpacingLabel = new Label();
        _pointSpacing = new NumericUpDown();
        _chordLabel = new Label();
        _chordTolerance = new NumericUpDown();
        _hint = new Label();
        _buttonBar = new Panel();
        _cancelButton = new Button();
        _okButton = new Button();
        _layout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_zHeight).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_valveOutput).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_pointSpacing).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_chordTolerance).BeginInit();
        _buttonBar.SuspendLayout();
        SuspendLayout();
        // 
        // _layout
        // 
        _layout.ColumnCount = 2;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Controls.Add(_zHeightLabel, 0, 0);
        _layout.Controls.Add(_zHeight, 1, 0);
        _layout.Controls.Add(_valveLabel, 0, 1);
        _layout.Controls.Add(_valveOutput, 1, 1);
        _layout.Controls.Add(_pointSpacingLabel, 0, 2);
        _layout.Controls.Add(_pointSpacing, 1, 2);
        _layout.Controls.Add(_chordLabel, 0, 3);
        _layout.Controls.Add(_chordTolerance, 1, 3);
        _layout.Controls.Add(_hint, 0, 4);
        _layout.Dock = DockStyle.Fill;
        _layout.Location = new Point(12, 12);
        _layout.Name = "_layout";
        _layout.RowCount = 5;
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.Size = new Size(616, 392);
        _layout.TabIndex = 0;
        // 
        // _zHeightLabel
        // 
        _zHeightLabel.AutoSize = true;
        _zHeightLabel.Dock = DockStyle.Fill;
        _zHeightLabel.Location = new Point(3, 0);
        _zHeightLabel.Name = "_zHeightLabel";
        _zHeightLabel.Size = new Size(214, 64);
        _zHeightLabel.TabIndex = 0;
        _zHeightLabel.Text = "Z height, mm";
        _zHeightLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _zHeight
        // 
        _zHeight.DecimalPlaces = 3;
        _zHeight.Increment = 0.001m;
        _zHeight.Location = new Point(223, 3);
        _zHeight.Maximum = 500m;
        _zHeight.Minimum = -500m;
        _zHeight.Name = "_zHeight";
        _zHeight.Size = new Size(120, 39);
        _zHeight.TabIndex = 1;
        _zHeight.Value = 0m;
        // 
        // _valveLabel
        // 
        _valveLabel.AutoSize = true;
        _valveLabel.Dock = DockStyle.Fill;
        _valveLabel.Location = new Point(3, 64);
        _valveLabel.Name = "_valveLabel";
        _valveLabel.Size = new Size(214, 64);
        _valveLabel.TabIndex = 2;
        _valveLabel.Text = "Valve output";
        _valveLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _valveOutput
        // 
        _valveOutput.Location = new Point(223, 67);
        _valveOutput.Maximum = 255m;
        _valveOutput.Name = "_valveOutput";
        _valveOutput.Size = new Size(120, 39);
        _valveOutput.TabIndex = 3;
        _valveOutput.Value = 1m;
        // 
        // _pointSpacingLabel
        // 
        _pointSpacingLabel.AutoSize = true;
        _pointSpacingLabel.Dock = DockStyle.Fill;
        _pointSpacingLabel.Location = new Point(3, 128);
        _pointSpacingLabel.Name = "_pointSpacingLabel";
        _pointSpacingLabel.Size = new Size(214, 64);
        _pointSpacingLabel.TabIndex = 4;
        _pointSpacingLabel.Text = "Point spacing, mm";
        _pointSpacingLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _pointSpacing
        // 
        _pointSpacing.DecimalPlaces = 2;
        _pointSpacing.Increment = 0.05m;
        _pointSpacing.Location = new Point(223, 131);
        _pointSpacing.Maximum = 20m;
        _pointSpacing.Minimum = 0.05m;
        _pointSpacing.Name = "_pointSpacing";
        _pointSpacing.Size = new Size(120, 39);
        _pointSpacing.TabIndex = 5;
        _pointSpacing.Value = 0.5m;
        // 
        // _chordLabel
        // 
        _chordLabel.AutoSize = true;
        _chordLabel.Dock = DockStyle.Fill;
        _chordLabel.Location = new Point(3, 192);
        _chordLabel.Name = "_chordLabel";
        _chordLabel.Size = new Size(214, 64);
        _chordLabel.TabIndex = 6;
        _chordLabel.Text = "Arc chord, mm";
        _chordLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _chordTolerance
        // 
        _chordTolerance.DecimalPlaces = 3;
        _chordTolerance.Increment = 0.01m;
        _chordTolerance.Location = new Point(223, 195);
        _chordTolerance.Maximum = 10m;
        _chordTolerance.Minimum = 0.01m;
        _chordTolerance.Name = "_chordTolerance";
        _chordTolerance.Size = new Size(120, 39);
        _chordTolerance.TabIndex = 7;
        _chordTolerance.Value = 0.2m;
        // 
        // _hint
        // 
        _hint.AutoSize = true;
        _layout.SetColumnSpan(_hint, 2);
        _hint.Dock = DockStyle.Top;
        _hint.ForeColor = SystemColors.GrayText;
        _hint.Location = new Point(3, 256);
        _hint.Margin = new Padding(3, 12, 3, 3);
        _hint.MaximumSize = new Size(600, 0);
        _hint.Name = "_hint";
        _hint.Size = new Size(600, 96);
        _hint.TabIndex = 8;
        _hint.Text = "Z stays at this height until a dispense law fills the channel. Arcs and circles are written as points. Point spacing is the longest step along the curve. Arc chord adds points when a straight step would leave the circle.";
        // 
        // _buttonBar
        // 
        _buttonBar.Controls.Add(_cancelButton);
        _buttonBar.Controls.Add(_okButton);
        _buttonBar.Dock = DockStyle.Bottom;
        _buttonBar.Location = new Point(12, 404);
        _buttonBar.Name = "_buttonBar";
        _buttonBar.Size = new Size(616, 64);
        _buttonBar.TabIndex = 1;
        // 
        // _cancelButton
        // 
        _cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Location = new Point(482, 10);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(132, 44);
        _cancelButton.TabIndex = 1;
        _cancelButton.Text = "Cancel";
        _cancelButton.UseVisualStyleBackColor = true;
        // 
        // _okButton
        // 
        _okButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _okButton.DialogResult = DialogResult.OK;
        _okButton.Location = new Point(344, 10);
        _okButton.Name = "_okButton";
        _okButton.Size = new Size(132, 44);
        _okButton.TabIndex = 0;
        _okButton.Text = "OK";
        _okButton.UseVisualStyleBackColor = true;
        // 
        // XyzSettingsPanel
        // 
        AcceptButton = _okButton;
        AutoScaleDimensions = new SizeF(13F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(640, 480);
        Controls.Add(_layout);
        Controls.Add(_buttonBar);
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(640, 480);
        Name = "XyzSettingsPanel";
        Padding = new Padding(12);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "PU2 ZYX";
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_zHeight).EndInit();
        ((System.ComponentModel.ISupportInitialize)_valveOutput).EndInit();
        ((System.ComponentModel.ISupportInitialize)_pointSpacing).EndInit();
        ((System.ComponentModel.ISupportInitialize)_chordTolerance).EndInit();
        _buttonBar.ResumeLayout(false);
        ResumeLayout(false);
    }

    private TableLayoutPanel _layout = null!;
    private Label _zHeightLabel = null!;
    private NumericUpDown _zHeight = null!;
    private Label _valveLabel = null!;
    private NumericUpDown _valveOutput = null!;
    private Label _pointSpacingLabel = null!;
    private NumericUpDown _pointSpacing = null!;
    private Label _chordLabel = null!;
    private NumericUpDown _chordTolerance = null!;
    private Label _hint = null!;
    private Panel _buttonBar = null!;
    private Button _okButton = null!;
    private Button _cancelButton = null!;
}
