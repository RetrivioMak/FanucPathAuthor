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
        _segmentList = new ListView();
        _moveUpButton = new Button();
        _moveDownButton = new Button();
        _gapSummaryLabel = new Label();
        _toolbar = new FlowLayoutPanel();
        _toolbar.SuspendLayout();
        SuspendLayout();
        // 
        // _segmentList
        // 
        _segmentList.Columns.Add("#", 32);
        _segmentList.Columns.Add("Type", 60);
        _segmentList.Columns.Add("Description", 180);
        _segmentList.Columns.Add("Next gap", 72);
        _segmentList.Dock = DockStyle.Fill;
        _segmentList.FullRowSelect = true;
        _segmentList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _segmentList.HideSelection = false;
        _segmentList.MultiSelect = false;
        _segmentList.Name = "_segmentList";
        _segmentList.TabIndex = 0;
        _segmentList.UseCompatibleStateImageBehavior = false;
        _segmentList.View = View.Details;
        // 
        // _moveUpButton
        // 
        _moveUpButton.AutoSize = true;
        _moveUpButton.Enabled = false;
        _moveUpButton.Location = new Point(3, 9);
        _moveUpButton.Name = "_moveUpButton";
        _moveUpButton.Size = new Size(120, 42);
        _moveUpButton.TabIndex = 0;
        _moveUpButton.Text = "Move up";
        // 
        // _moveDownButton
        // 
        _moveDownButton.AutoSize = true;
        _moveDownButton.Enabled = false;
        _moveDownButton.Location = new Point(129, 9);
        _moveDownButton.Name = "_moveDownButton";
        _moveDownButton.Size = new Size(151, 42);
        _moveDownButton.TabIndex = 1;
        _moveDownButton.Text = "Move down";
        // 
        // _gapSummaryLabel
        // 
        _gapSummaryLabel.AutoSize = true;
        _gapSummaryLabel.ForeColor = Color.DimGray;
        _gapSummaryLabel.Location = new Point(286, 6);
        _gapSummaryLabel.Name = "_gapSummaryLabel";
        _gapSummaryLabel.Padding = new Padding(8, 6, 0, 0);
        _gapSummaryLabel.Size = new Size(168, 38);
        _gapSummaryLabel.TabIndex = 2;
        _gapSummaryLabel.Text = "Sequence: —";
        // 
        // _toolbar
        // 
        _toolbar.AutoSize = true;
        _toolbar.Controls.Add(_moveUpButton);
        _toolbar.Controls.Add(_moveDownButton);
        _toolbar.Controls.Add(_gapSummaryLabel);
        _toolbar.Dock = DockStyle.Bottom;
        _toolbar.Location = new Point(0, 341);
        _toolbar.Name = "_toolbar";
        _toolbar.Padding = new Padding(0, 6, 0, 0);
        _toolbar.TabIndex = 1;
        _toolbar.WrapContents = true;
        // 
        // SegmentListPanel
        // 
        Controls.Add(_segmentList);
        Controls.Add(_toolbar);
        Name = "SegmentListPanel";
        Size = new Size(528, 395);
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
