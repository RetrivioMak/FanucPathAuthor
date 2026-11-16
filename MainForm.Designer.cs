#nullable enable
namespace DispensePathAuthor;

partial class MainForm
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
        _menuStrip = new MenuStrip();
        _fileMenu = new ToolStripMenuItem();
        _newProjectMenuItem = new ToolStripMenuItem();
        _loadPathMenuItem = new ToolStripMenuItem();
        _savePathMenuItem = new ToolStripMenuItem();
        _savePathAsMenuItem = new ToolStripMenuItem();
        _loadCalibrationMenuItem = new ToolStripMenuItem();
        _exportSettingsMenuItem = new ToolStripMenuItem();
        _exportCsvMenuItem = new ToolStripMenuItem();
        _exportLsMenuItem = new ToolStripMenuItem();
        _editMenu = new ToolStripMenuItem();
        _undoMenuItem = new ToolStripMenuItem();
        _redoMenuItem = new ToolStripMenuItem();
        _cancelShapeMenuItem = new ToolStripMenuItem();
        _deleteSegmentMenuItem = new ToolStripMenuItem();
        _clearSegmentsMenuItem = new ToolStripMenuItem();
        _viewMenu = new ToolStripMenuItem();
        _fitViewMenuItem = new ToolStripMenuItem();
        _refreshPreviewMenuItem = new ToolStripMenuItem();
        _simulatePlayMenuItem = new ToolStripMenuItem();
        _simulatePauseMenuItem = new ToolStripMenuItem();
        _simulateStopMenuItem = new ToolStripMenuItem();
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel();
        _cursorLabel = new ToolStripStatusLabel();
        _hintLabel = new ToolStripStatusLabel();
        _canvasHost = new Panel();
        _rightPanel = new Panel();
        _rightLayout = new TableLayoutPanel();
        _projectHost = new Panel();
        _commonTabs = new TabControl();
        _segmentsPage = new TabPage();
        _segmentListPanel = new Controls.SegmentListPanel();
        _previewPage = new TabPage();
        _waypointPreviewPanel = new Controls.WaypointPreviewPanel();
        _topPanel = new Panel();
        _paletteHost = new Panel();
        _menuStrip.SuspendLayout();
        _statusStrip.SuspendLayout();
        _rightPanel.SuspendLayout();
        _rightLayout.SuspendLayout();
        _commonTabs.SuspendLayout();
        _segmentsPage.SuspendLayout();
        _previewPage.SuspendLayout();
        _topPanel.SuspendLayout();
        SuspendLayout();

        _newProjectMenuItem.Name = "newProjectMenuItem";
        _newProjectMenuItem.Text = "New project";
        _loadPathMenuItem.Name = "loadPathMenuItem";
        _loadPathMenuItem.Text = "Open project...";
        _savePathMenuItem.Name = "savePathMenuItem";
        _savePathMenuItem.Text = "Save project";
        _savePathAsMenuItem.Name = "savePathAsMenuItem";
        _savePathAsMenuItem.Text = "Save project as...";
        _loadCalibrationMenuItem.Name = "loadCalibrationMenuItem";
        _loadCalibrationMenuItem.Text = "Load calibration JSON...";
        _exportSettingsMenuItem.Name = "exportSettingsMenuItem";
        _exportSettingsMenuItem.Text = "Export settings...";
        _exportCsvMenuItem.Name = "exportCsvMenuItem";
        _exportCsvMenuItem.Text = "Export CSV...";
        _exportLsMenuItem.Name = "exportLsMenuItem";
        _exportLsMenuItem.Text = "Export Fanuc .LS...";
        _fileMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            _newProjectMenuItem,
            _loadPathMenuItem,
            _savePathMenuItem,
            _savePathAsMenuItem,
            new ToolStripSeparator(),
            _loadCalibrationMenuItem,
            new ToolStripSeparator(),
            _exportSettingsMenuItem,
            _exportCsvMenuItem,
            _exportLsMenuItem
        });
        _fileMenu.Name = "fileMenu";
        _fileMenu.Text = "File";

        _undoMenuItem.Name = "undoMenuItem";
        _undoMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
        _undoMenuItem.Text = "Undo";
        _redoMenuItem.Name = "redoMenuItem";
        _redoMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
        _redoMenuItem.Text = "Redo";
        _cancelShapeMenuItem.Name = "cancelShapeMenuItem";
        _cancelShapeMenuItem.Text = "Cancel shape (Esc)";
        _deleteSegmentMenuItem.Name = "deleteSegmentMenuItem";
        _deleteSegmentMenuItem.Text = "Delete selected segment";
        _clearSegmentsMenuItem.Name = "clearSegmentsMenuItem";
        _clearSegmentsMenuItem.Text = "Clear all segments";
        _editMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            _undoMenuItem,
            _redoMenuItem,
            new ToolStripSeparator(),
            _cancelShapeMenuItem,
            new ToolStripSeparator(),
            _deleteSegmentMenuItem,
            _clearSegmentsMenuItem
        });
        _editMenu.Name = "editMenu";
        _editMenu.Text = "Edit";

        _fitViewMenuItem.Name = "fitViewMenuItem";
        _fitViewMenuItem.Text = "Fit view";
        _refreshPreviewMenuItem.Name = "refreshPreviewMenuItem";
        _refreshPreviewMenuItem.Text = "Refresh preview";
        _simulatePlayMenuItem.Name = "simulatePlayMenuItem";
        _simulatePlayMenuItem.ShortcutKeys = Keys.F5;
        _simulatePlayMenuItem.Text = "Simulate Play";
        _simulatePauseMenuItem.Name = "simulatePauseMenuItem";
        _simulatePauseMenuItem.Text = "Simulate Pause";
        _simulateStopMenuItem.Name = "simulateStopMenuItem";
        _simulateStopMenuItem.Text = "Simulate Stop";
        _viewMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            _fitViewMenuItem,
            _refreshPreviewMenuItem,
            new ToolStripSeparator(),
            _simulatePlayMenuItem,
            _simulatePauseMenuItem,
            _simulateStopMenuItem
        });
        _viewMenu.Name = "viewMenu";
        _viewMenu.Text = "View";

        _menuStrip.Items.AddRange(new ToolStripItem[] { _fileMenu, _editMenu, _viewMenu });
        _menuStrip.Dock = DockStyle.Top;
        _menuStrip.Name = "menuStrip";

        _statusLabel.Name = "statusLabel";
        _statusLabel.Spring = true;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _cursorLabel.AutoSize = false;
        _cursorLabel.Name = "cursorLabel";
        _cursorLabel.TextAlign = ContentAlignment.MiddleLeft;
        _cursorLabel.Width = 420;
        _hintLabel.Name = "hintLabel";
        _hintLabel.Text = "Ctrl+wheel: zoom | Ctrl+drag: pan | Esc: cancel";
        _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, _cursorLabel, _hintLabel });
        _statusStrip.Dock = DockStyle.Bottom;
        _statusStrip.Name = "statusStrip";

        _segmentListPanel.Dock = DockStyle.Fill;
        _segmentListPanel.Name = "segmentListPanel";
        _segmentsPage.Controls.Add(_segmentListPanel);
        _segmentsPage.Name = "segmentsPage";
        _segmentsPage.Padding = new Padding(4);
        _segmentsPage.Text = "Segments";

        _waypointPreviewPanel.Dock = DockStyle.Fill;
        _waypointPreviewPanel.Name = "waypointPreviewPanel";
        _previewPage.Controls.Add(_waypointPreviewPanel);
        _previewPage.Name = "previewPage";
        _previewPage.Padding = new Padding(4);
        _previewPage.Text = "Robot Preview";

        _commonTabs.Controls.Add(_segmentsPage);
        _commonTabs.Controls.Add(_previewPage);
        _commonTabs.Dock = DockStyle.Fill;
        _commonTabs.Name = "commonTabs";

        _projectHost.Dock = DockStyle.Fill;
        _projectHost.Name = "projectHost";

        _rightLayout.ColumnCount = 1;
        _rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rightLayout.Controls.Add(_projectHost, 0, 0);
        _rightLayout.Controls.Add(_commonTabs, 0, 1);
        _rightLayout.Dock = DockStyle.Fill;
        _rightLayout.Name = "rightLayout";
        _rightLayout.RowCount = 2;
        _rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 46F));
        _rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 54F));

        _rightPanel.Controls.Add(_rightLayout);
        _rightPanel.Dock = DockStyle.Right;
        _rightPanel.Name = "rightPanel";
        _rightPanel.Width = 640;

        _canvasHost.Dock = DockStyle.Fill;
        _canvasHost.Name = "canvasHost";

        _paletteHost.AutoSize = true;
        _paletteHost.Dock = DockStyle.Top;
        _paletteHost.Name = "paletteHost";
        _topPanel.AutoSize = true;
        _topPanel.Controls.Add(_paletteHost);
        _topPanel.Controls.Add(_menuStrip);
        _topPanel.Dock = DockStyle.Top;
        _topPanel.Name = "topPanel";

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1400, 900);
        Controls.Add(_canvasHost);
        Controls.Add(_rightPanel);
        Controls.Add(_statusStrip);
        Controls.Add(_topPanel);
        KeyPreview = true;
        MainMenuStrip = _menuStrip;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;

        _previewPage.ResumeLayout(false);
        _segmentsPage.ResumeLayout(false);
        _commonTabs.ResumeLayout(false);
        _rightLayout.ResumeLayout(false);
        _rightPanel.ResumeLayout(false);
        _statusStrip.ResumeLayout(false);
        _statusStrip.PerformLayout();
        _topPanel.ResumeLayout(false);
        _topPanel.PerformLayout();
        _menuStrip.ResumeLayout(false);
        _menuStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private MenuStrip _menuStrip = null!;
    private ToolStripMenuItem _fileMenu = null!;
    private ToolStripMenuItem _newProjectMenuItem = null!;
    private ToolStripMenuItem _loadPathMenuItem = null!;
    private ToolStripMenuItem _savePathMenuItem = null!;
    private ToolStripMenuItem _savePathAsMenuItem = null!;
    private ToolStripMenuItem _loadCalibrationMenuItem = null!;
    private ToolStripMenuItem _exportSettingsMenuItem = null!;
    private ToolStripMenuItem _exportCsvMenuItem = null!;
    private ToolStripMenuItem _exportLsMenuItem = null!;
    private ToolStripMenuItem _editMenu = null!;
    private ToolStripMenuItem _undoMenuItem = null!;
    private ToolStripMenuItem _redoMenuItem = null!;
    private ToolStripMenuItem _cancelShapeMenuItem = null!;
    private ToolStripMenuItem _deleteSegmentMenuItem = null!;
    private ToolStripMenuItem _clearSegmentsMenuItem = null!;
    private ToolStripMenuItem _viewMenu = null!;
    private ToolStripMenuItem _fitViewMenuItem = null!;
    private ToolStripMenuItem _refreshPreviewMenuItem = null!;
    private ToolStripMenuItem _simulatePlayMenuItem = null!;
    private ToolStripMenuItem _simulatePauseMenuItem = null!;
    private ToolStripMenuItem _simulateStopMenuItem = null!;
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _statusLabel = null!;
    private ToolStripStatusLabel _cursorLabel = null!;
    private ToolStripStatusLabel _hintLabel = null!;
    private Panel _canvasHost = null!;
    private Panel _rightPanel = null!;
    private TableLayoutPanel _rightLayout = null!;
    private Panel _projectHost = null!;
    private TabControl _commonTabs = null!;
    private TabPage _segmentsPage = null!;
    private Controls.SegmentListPanel _segmentListPanel = null!;
    private TabPage _previewPage = null!;
    private Controls.WaypointPreviewPanel _waypointPreviewPanel = null!;
    private Panel _topPanel = null!;
    private Panel _paletteHost = null!;
}
