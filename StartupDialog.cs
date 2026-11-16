namespace DispensePathAuthor;

public enum StartupAction
{
    NewCv,
    NewTeach,
    Open
}

public readonly record struct ProjectStart(StartupAction Action, string? FilePath);

public partial class StartupDialog : Form
{
    public StartupAction Action { get; private set; }

    public string? FilePath { get; private set; }

    public StartupDialog()
    {
        InitializeComponent();
        Text = AppVersion.ProductName;
        _newCvButton.Click += (_, _) => Accept(StartupAction.NewCv);
        _newTeachButton.Click += (_, _) => Accept(StartupAction.NewTeach);
        _openButton.Click += (_, _) => OpenProject();
    }

    private void Accept(StartupAction action)
    {
        Action = action;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OpenProject()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = Services.ProjectPackage.OpenFilter,
            Title = "Open project"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            Services.ProjectPackage.LoadFile(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Open project",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        FilePath = dialog.FileName;
        Accept(StartupAction.Open);
    }
}
