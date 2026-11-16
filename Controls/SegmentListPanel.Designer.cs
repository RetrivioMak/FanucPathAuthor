#nullable enable
namespace DispensePathAuthor.Controls;

partial class SegmentListPanel
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
        _segmentList = new ListView();
        _moveUpButton = new Button();
        _moveDownButton = new Button();
        _gapSummaryLabel = new Label();
        _toolbar = new FlowLayoutPanel();
        SuspendLayout();
        _toolbar.SuspendLayout();

        _segmentList.Columns.Add("#", 32);
        _segmentList.Columns.Add("Type", 60);
        _segmentList.Columns.Add("Description", 180);
        _segmentList.Columns.Add("Next gap", 72);
        _segmentList.Dock = DockStyle.Fill;
        _segmentList.FullRowSelect = true;
        _segmentList.HideSelection = false;
        _segmentList.Name = "segmentList";
        _segmentList.View = View.Details;

        _moveUpButton.AutoSize = true;
        _moveUpButton.Enabled = false;
        _moveUpButton.Name = "moveUpButton";
        _moveUpButton.Text = "Move up";

        _moveDownButton.AutoSize = true;
        _moveDownButton.Enabled = false;
        _moveDownButton.Name = "moveDownButton";
        _moveDownButton.Text = "Move down";

        _gapSummaryLabel.AutoSize = true;
        _gapSummaryLabel.ForeColor = Color.DimGray;
        _gapSummaryLabel.Name = "gapSummaryLabel";
        _gapSummaryLabel.Padding = new Padding(8, 6, 0, 0);
        _gapSummaryLabel.Text = "Sequence: ¡ª";

        _toolbar.AutoSize = true;
        _toolbar.Controls.Add(_moveUpButton);
        _toolbar.Controls.Add(_moveDownButton);
        _toolbar.Controls.Add(_gapSummaryLabel);
        _toolbar.Dock = DockStyle.Bottom;
        _toolbar.Name = "segmentToolbar";
        _toolbar.Padding = new Padding(0, 6, 0, 0);
        _toolbar.WrapContents = false;

        Controls.Add(_segmentList);
        Controls.Add(_toolbar);
        Name = "SegmentListPanel";
        _toolbar.ResumeLayout(false);
        _toolbar.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private ListView _segmentList = null!;
    private Button _moveUpButton = null!;
    private Button _moveDownButton = null!;
    private Label _gapSummaryLabel = null!;
    private FlowLayoutPanel _toolbar = null!;
}
