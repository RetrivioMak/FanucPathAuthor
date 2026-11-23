namespace DispensePathAuthor.Controls;

public sealed partial class ValuePromptDialog : Form
{
    public ValuePromptDialog()
    {
        InitializeComponent();
        Icon = AppIcon.Load();
    }

    public string Caption
    {
        get => _caption.Text;
        set => _caption.Text = value;
    }

    public string AcceptText
    {
        get => _okButton.Text;
        set => _okButton.Text = value;
    }

    public decimal Minimum
    {
        get => _input.Minimum;
        set => _input.Minimum = value;
    }

    public decimal Maximum
    {
        get => _input.Maximum;
        set => _input.Maximum = value;
    }

    public decimal Value
    {
        get => _input.Value;
        set => _input.Value = Math.Min(_input.Maximum, Math.Max(_input.Minimum, value));
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _input.Focus();
        _input.Select(0, _input.Text.Length);
    }
}
