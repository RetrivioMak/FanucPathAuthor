#nullable enable
namespace DispensePathAuthor.Export.Fanuc;

partial class FanucExportDialog
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
        InitializeFormShell();
        BuildLayout();
    }
}
