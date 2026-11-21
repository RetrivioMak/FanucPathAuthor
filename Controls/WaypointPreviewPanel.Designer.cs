#nullable enable
namespace DispensePathAuthor.Controls;

partial class WaypointPreviewPanel
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
        _summaryLabel = new Label();
        _waypointList = new ListView();
        _header = new Panel();
        SuspendLayout();
        _header.SuspendLayout();

        _summaryLabel.AutoSize = true;
        _summaryLabel.Dock = DockStyle.Top;
        _summaryLabel.ForeColor = Color.DimGray;
        _summaryLabel.Name = "summaryLabel";
        _summaryLabel.Text = "Export preview: —";

        _header.AutoSize = true;
        _header.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _header.Controls.Add(_summaryLabel);
        _header.Dock = DockStyle.Top;
        _header.Name = "previewHeader";
        _header.Padding = new Padding(0, 0, 0, 6);

        _waypointList.Columns.Add("#", 40);
        _waypointList.Columns.Add("Role", 50);
        _waypointList.Columns.Add("Export X", 90);
        _waypointList.Columns.Add("Export Y", 90);
        _waypointList.Columns.Add("Export R", 70);
        _waypointList.Columns.Add("Local X", 80);
        _waypointList.Columns.Add("Local Y", 80);
        _waypointList.Dock = DockStyle.Fill;
        _waypointList.FullRowSelect = true;
        _waypointList.Name = "waypointList";
        _waypointList.View = View.Details;

        Controls.Add(_waypointList);
        Controls.Add(_header);
        Name = "WaypointPreviewPanel";
        _header.ResumeLayout(false);
        _header.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private Label _summaryLabel = null!;
    private ListView _waypointList = null!;
    private Panel _header = null!;
}
