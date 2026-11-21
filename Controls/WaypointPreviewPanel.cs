using DispensePathAuthor.Models;

namespace DispensePathAuthor.Controls;

public partial class WaypointPreviewPanel : UserControl
{
    public WaypointPreviewPanel()
    {
        InitializeComponent();
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        int textWidth = Math.Max(120, ClientSize.Width - 8);
        var limit = new Size(textWidth, 0);
        if (_summaryLabel.MaximumSize != limit)
        {
            _summaryLabel.MaximumSize = limit;
        }

        base.OnLayout(e);
    }

    public void Show(string summary, IReadOnlyList<RobotWaypoint> waypoints)
    {
        _summaryLabel.Text = summary;
        _waypointList.Items.Clear();
        foreach (RobotWaypoint waypoint in waypoints)
        {
            var item = new ListViewItem(waypoint.Index.ToString());
            item.SubItems.Add(waypoint.Role);
            item.SubItems.Add(double.IsNaN(waypoint.ExportX_mm) ? "-" : waypoint.ExportX_mm.ToString("0.###"));
            item.SubItems.Add(double.IsNaN(waypoint.ExportY_mm) ? "-" : waypoint.ExportY_mm.ToString("0.###"));
            item.SubItems.Add(double.IsNaN(waypoint.ExportR_deg) ? "-" : waypoint.ExportR_deg.ToString("0.###"));
            item.SubItems.Add(waypoint.LocalX_px.ToString("0.###"));
            item.SubItems.Add(waypoint.LocalY_px.ToString("0.###"));
            if (waypoint.IsPreviewOnly)
            {
                item.ForeColor = Color.DimGray;
            }

            _waypointList.Items.Add(item);
        }
    }
}
