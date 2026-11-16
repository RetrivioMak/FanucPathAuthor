#nullable enable
namespace DispensePathAuthor;

partial class StartupDialog
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
        _heading = new Label();
        _hint = new Label();
        _newCvButton = new Button();
        _newTeachButton = new Button();
        _openButton = new Button();
        _cancelButton = new Button();
        SuspendLayout();

        _heading.AutoSize = true;
        _heading.Font = new Font(Font, FontStyle.Bold);
        _heading.Location = new Point(24, 20);
        _heading.Name = "headingLabel";
        _heading.Text = "Choose a project";

        _hint.Location = new Point(24, 48);
        _hint.Name = "hintLabel";
        _hint.Size = new Size(392, 72);
        _hint.Text = "CV keeps a camera image and a sensor frame. Teach keeps reference points and a millimeter grid. The type stays fixed for the life of the project.";

        _newCvButton.Location = new Point(24, 128);
        _newCvButton.Name = "newCvButton";
        _newCvButton.Size = new Size(392, 32);
        _newCvButton.Text = "New CV project";

        _newTeachButton.Location = new Point(24, 168);
        _newTeachButton.Name = "newTeachButton";
        _newTeachButton.Size = new Size(392, 32);
        _newTeachButton.Text = "New Teach project";

        _openButton.Location = new Point(24, 208);
        _openButton.Name = "openButton";
        _openButton.Size = new Size(392, 32);
        _openButton.Text = "Open project...";

        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Location = new Point(296, 256);
        _cancelButton.Name = "cancelButton";
        _cancelButton.Size = new Size(120, 28);
        _cancelButton.Text = "Cancel";

        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(440, 304);
        Controls.Add(_heading);
        Controls.Add(_hint);
        Controls.Add(_newCvButton);
        Controls.Add(_newTeachButton);
        Controls.Add(_openButton);
        Controls.Add(_cancelButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "StartupDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Dispense Path Author";
        ResumeLayout(false);
        PerformLayout();
    }

    private Label _heading = null!;
    private Label _hint = null!;
    private Button _newCvButton = null!;
    private Button _newTeachButton = null!;
    private Button _openButton = null!;
    private Button _cancelButton = null!;
}
