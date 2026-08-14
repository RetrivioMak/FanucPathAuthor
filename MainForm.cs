using FanucPathAuthor.Controls;
using FanucPathAuthor.Models;
using FanucPathAuthor.Services;

namespace FanucPathAuthor;

public sealed class MainForm : Form
{
    private readonly PathCanvas _canvas = new() { Dock = DockStyle.Fill };
    private readonly ListView _segmentList = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        HideSelection = false
    };
    private readonly Button _btnMoveSegmentUp = new() { Text = "Move up", AutoSize = true, Enabled = false };
    private readonly Button _btnMoveSegmentDown = new() { Text = "Move down", AutoSize = true, Enabled = false };
    private readonly Label _segmentGapSummaryLabel = new()
    {
        AutoSize = true,
        Padding = new Padding(8, 6, 0, 0),
        ForeColor = Color.DimGray,
        Text = "Sequence: —"
    };
    private readonly Label _previewExportModeLabel = new()
    {
        AutoSize = true,
        ForeColor = Color.DimGray,
        Text = "Export preview: —"
    };
    private readonly ListView _waypointPreview = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true
    };
    private readonly ListView _tileList = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        HideSelection = false
    };
    private readonly NumericUpDown _tileRobotX = CreateNumeric(-5000, 5000, 0.1m, decimals: 2);
    private readonly NumericUpDown _tileRobotY = CreateNumeric(-5000, 5000, 0.1m, decimals: 2);
    private readonly NumericUpDown _tileRobotR = CreateNumeric(-360, 360, 0.1m, decimals: 2);
    private readonly Label _tileOffsetLabel = new() { AutoSize = true, Text = "Canvas offset: —" };
    private readonly Label _tileWorldLabel = new() { AutoSize = true, Text = "Tile (0,0) world: —" };
    private readonly Label _tileCaptureHint = new()
    {
        AutoSize = true,
        MaximumSize = new Size(430, 0),
        ForeColor = Color.DimGray,
        Text = "Open the anchor FOV image, define Frame, then teach the path in sensor space."
    };
    private Button? _tileOpenImageButton;
    private Button? _tileAddFovButton;
    private Button? _tileResetCapturePoseButton;
    private readonly Label _tileModeLabel = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        Text = "Mode: Single FOV"
    };
    private readonly ToolStripStatusLabel _statusLabel = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly NumericUpDown _frameOriginX = CreateNumeric(0, 10000, 0);
    private readonly NumericUpDown _frameOriginY = CreateNumeric(0, 10000, 0);
    private readonly NumericUpDown _frameAngle = CreateNumeric(-180, 180, 1);
    private readonly Button _btnEditFrame = new() { Text = "Edit Frame", AutoSize = true };
    private readonly Button _btnApplyFrame = new() { Text = "Apply", AutoSize = true, Enabled = false };
    private readonly Button _btnCancelFrame = new() { Text = "Cancel", AutoSize = true, Enabled = false };
    private readonly RadioButton _radioMoveShapes = new()
    {
        Text = "Move shapes with frame (keep sensor px)",
        AutoSize = true,
        Checked = true,
        Enabled = false
    };
    private readonly RadioButton _radioFrameOnly = new()
    {
        Text = "Update frame only (keep local px)",
        AutoSize = true,
        Enabled = false
    };
    private readonly Panel _rightPanel = new() { Dock = DockStyle.Right, Width = 640, Padding = new Padding(0) };
    private readonly Label _calibrationLabel = new() { AutoSize = true, Text = "Calibration: not loaded" };
    private readonly ToolPalettePanel _toolPalette = new();
    private readonly PathSimulationController _simulation = new();

    private ToolStripMenuItem? _undoMenuItem;
    private ToolStripMenuItem? _redoMenuItem;
    private readonly ToolStripStatusLabel _cursorLabel = new() { AutoSize = false, Width = 280, TextAlign = ContentAlignment.MiddleLeft };

    private RobotCalibrationFile? _calibration;
    private string? _calibrationPath;
    private string? _currentPathFile;
    private bool _updatingUi;
    private bool _frameEditing;

    public MainForm()
    {
        Text = AppVersion.WindowTitle;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        KeyPreview = true;

        InitializeLayout();
        WireEvents();
        ExportSettingsPersistence.TryLoadInto(_canvas.Document.Defaults);
        RefreshSegmentList();
        RefreshWaypointPreview();
        UpdateUndoMenuState();
        SetActiveTool(DrawTool.Dot);
        RefreshTileList();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        ExportSettingsPersistence.Save(_canvas.Document.Defaults);
        base.OnFormClosing(e);
    }

    private void InitializeLayout()
    {
        var menu = new MenuStrip();
        menu.Items.Add(CreateMenu("File", [
            CreateMenuItem("Load path JSON...", LoadPath),
            CreateMenuItem("Save path JSON...", SavePath),
            CreateMenuItem("Save path JSON as...", SavePathAs),
            new ToolStripSeparator(),
            CreateMenuItem("Load calibration JSON...", LoadCalibration),
            new ToolStripSeparator(),
            CreateMenuItem("Export settings...", OpenExportSettings),
            CreateMenuItem("Export CSV...", ExportCsv),
            CreateMenuItem("Export Fanuc .LS...", ExportLs)
        ]));
        _undoMenuItem = CreateMenuItem("Undo", UndoAction, Keys.Control | Keys.Z);
        _redoMenuItem = CreateMenuItem("Redo", RedoAction, Keys.Control | Keys.Y);
        menu.Items.Add(CreateMenu("Edit", [
            _undoMenuItem,
            _redoMenuItem,
            new ToolStripSeparator(),
            CreateMenuItem("Cancel shape (Esc)", CancelPending),
            new ToolStripSeparator(),
            CreateMenuItem("Delete selected segment", DeleteSelectedSegment),
            CreateMenuItem("Clear all segments", ClearSegments)
        ]));
        menu.Items.Add(CreateMenu("View", [
            CreateMenuItem("Fit image", FitImage),
            CreateMenuItem("Refresh preview", RefreshWaypointPreview),
            new ToolStripSeparator(),
            CreateMenuItem("Simulate Play", SimulationPlay, Keys.F5),
            CreateMenuItem("Simulate Pause", SimulationPause),
            CreateMenuItem("Simulate Stop", SimulationStop)
        ]));
        MainMenuStrip = menu;

        var resizeGrip = new PanelResizeGrip
        {
            TargetPanel = _rightPanel,
            MinTargetWidth = 320,
            MaxTargetWidth = 1200
        };

        var rightInner = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        var rightLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        rightLayout.Controls.Add(BuildFrameGroup(), 0, 0);
        rightLayout.Controls.Add(BuildDataTabs(), 0, 1);
        rightLayout.Controls.Add(_calibrationLabel, 0, 2);
        rightInner.Controls.Add(rightLayout);

        _rightPanel.Controls.Add(rightInner);
        _rightPanel.Controls.Add(resizeGrip);

        var statusStrip = new StatusStrip();
        statusStrip.Items.Add(_statusLabel);
        statusStrip.Items.Add(_cursorLabel);
        statusStrip.Items.Add(new ToolStripStatusLabel("Ctrl+wheel: zoom | Ctrl+drag: pan | Esc: cancel"));

        // Dock order: Fill/sides first, then top bars (last top control = uppermost).
        Controls.Add(_canvas);
        Controls.Add(_rightPanel);
        Controls.Add(statusStrip);
        Controls.Add(_toolPalette);
        Controls.Add(menu);

        FormClosed += (_, _) => _simulation.Dispose();
        UpdateSimulationUi();
    }

    private GroupBox BuildFrameGroup()
    {
        SetFrameEditorsEnabled(false);

        var group = new GroupBox { Text = "Frame (sensor px)", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
        layout.Controls.Add(new Label { Text = "Origin X", AutoSize = true }, 0, 0);
        layout.Controls.Add(_frameOriginX, 1, 0);
        layout.Controls.Add(new Label { Text = "Origin Y", AutoSize = true }, 0, 1);
        layout.Controls.Add(_frameOriginY, 1, 1);
        layout.Controls.Add(new Label { Text = "Angle deg", AutoSize = true }, 0, 2);
        layout.Controls.Add(_frameAngle, 1, 2);

        var applyModePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0, 8, 0, 0)
        };
        applyModePanel.Controls.Add(_radioMoveShapes);
        applyModePanel.Controls.Add(_radioFrameOnly);
        layout.SetColumnSpan(applyModePanel, 2);
        layout.Controls.Add(applyModePanel, 0, 3);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 8, 0, 0)
        };
        buttons.Controls.Add(_btnEditFrame);
        buttons.Controls.Add(_btnApplyFrame);
        buttons.Controls.Add(_btnCancelFrame);
        layout.SetColumnSpan(buttons, 2);
        layout.Controls.Add(buttons, 0, 4);

        group.Controls.Add(layout);
        return group;
    }

    private TabControl BuildDataTabs()
    {
        _segmentList.Columns.Add("#", 32);
        _segmentList.Columns.Add("Type", 60);
        _segmentList.Columns.Add("Description", 180);
        _segmentList.Columns.Add("Next gap", 72);

        _waypointPreview.Columns.Add("#", 40);
        _waypointPreview.Columns.Add("Role", 50);
        _waypointPreview.Columns.Add("Export X", 90);
        _waypointPreview.Columns.Add("Export Y", 90);
        _waypointPreview.Columns.Add("Export R", 70);
        _waypointPreview.Columns.Add("Local X", 80);
        _waypointPreview.Columns.Add("Local Y", 80);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        var segmentsPage = new TabPage("Segments") { Padding = new Padding(4) };
        var segmentToolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0)
        };
        segmentToolbar.Controls.Add(_btnMoveSegmentUp);
        segmentToolbar.Controls.Add(_btnMoveSegmentDown);
        segmentToolbar.Controls.Add(_segmentGapSummaryLabel);
        segmentsPage.Controls.Add(_segmentList);
        segmentsPage.Controls.Add(segmentToolbar);
        var previewPage = new TabPage("Robot Preview") { Padding = new Padding(4) };
        var previewHeader = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 0, 0, 6)
        };
        previewHeader.Controls.Add(_previewExportModeLabel);
        previewPage.Controls.Add(_waypointPreview);
        previewPage.Controls.Add(previewHeader);
        tabs.TabPages.Add(segmentsPage);
        tabs.TabPages.Add(previewPage);
        tabs.TabPages.Add(BuildTilesTab());
        return tabs;
    }

    private TabPage BuildTilesTab()
    {
        _tileList.Columns.Add("Name", 100);
        _tileList.Columns.Add("Capture X", 70);
        _tileList.Columns.Add("Capture Y", 70);
        _tileList.Columns.Add("R", 45);
        _tileList.Columns.Add("Active", 55);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 0, 0, 6)
        };
        _tileOpenImageButton = new Button { Text = "Open image...", AutoSize = true, Tag = "open" };
        _tileAddFovButton = new Button { Text = "Add FOV image...", AutoSize = true, Tag = "add" };
        buttons.Controls.Add(_tileOpenImageButton);
        buttons.Controls.Add(_tileAddFovButton);
        buttons.Controls.Add(new Button { Text = "Remove", AutoSize = true, Tag = "remove" });
        buttons.Controls.Add(new Button { Text = "Focus", AutoSize = true, Tag = "activate" });
        foreach (Control control in buttons.Controls)
        {
            if (control is Button button)
            {
                button.Click += OnTileButtonClick;
            }
        }

        var poseLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(0, 0, 0, 6)
        };
        poseLayout.Controls.Add(new Label { Text = "Capture X mm", AutoSize = true }, 0, 0);
        poseLayout.Controls.Add(_tileRobotX, 1, 0);
        poseLayout.Controls.Add(new Label { Text = "Capture Y mm", AutoSize = true }, 0, 1);
        poseLayout.Controls.Add(_tileRobotY, 1, 1);
        poseLayout.Controls.Add(new Label { Text = "Capture R deg", AutoSize = true }, 0, 2);
        poseLayout.Controls.Add(_tileRobotR, 1, 2);

        _tileResetCapturePoseButton = new Button
        {
            Text = "Use calib. reference",
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 0)
        };
        _tileResetCapturePoseButton.Click += OnResetCapturePoseToCalibrationReference;

        var poseGroup = new GroupBox
        {
            Text = "Capture pose (robot ref. pose)",
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 8),
            Margin = new Padding(0, 0, 0, 6)
        };
        var poseGroupLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true
        };
        poseGroupLayout.Controls.Add(poseLayout);
        poseGroupLayout.Controls.Add(_tileResetCapturePoseButton);
        poseGroup.Controls.Add(poseGroupLayout);

        var infoPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        infoPanel.Controls.Add(_tileModeLabel);
        infoPanel.Controls.Add(_tileCaptureHint);
        infoPanel.Controls.Add(_tileOffsetLabel);
        infoPanel.Controls.Add(_tileWorldLabel);

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(0, 6, 0, 0) };
        bottomPanel.Controls.Add(infoPanel);
        bottomPanel.Controls.Add(poseGroup);
        bottomPanel.Controls.Add(buttons);

        var page = new TabPage("FOV Tiles") { Padding = new Padding(4) };
        page.Controls.Add(_tileList);
        page.Controls.Add(bottomPanel);

        _tileList.SelectedIndexChanged += (_, _) => SyncTileEditorsFromSelection();
        _tileRobotX.ValueChanged += (_, _) => ApplyTilePoseFromEditors();
        _tileRobotY.ValueChanged += (_, _) => ApplyTilePoseFromEditors();
        _tileRobotR.ValueChanged += (_, _) => ApplyTilePoseFromEditors();

        SetTileEditorsEnabled(false);
        return page;
    }

    private void SetFrameEditorsEnabled(bool enabled)
    {
        _frameOriginX.Enabled = enabled;
        _frameOriginY.Enabled = enabled;
        _frameAngle.Enabled = enabled;
        _radioMoveShapes.Enabled = enabled;
        _radioFrameOnly.Enabled = enabled;
        _btnApplyFrame.Enabled = enabled;
        _btnCancelFrame.Enabled = enabled;
        _btnEditFrame.Enabled = !enabled;
    }

    private void WireEvents()
    {
        _canvas.DocumentChanged += (_, _) =>
        {
            if (_simulation.State != SimulationState.Stopped)
            {
                StopSimulation();
            }

            SyncUiFromDocument();
            RefreshSegmentList();
            RefreshWaypointPreview();
            RefreshTileList();
            UpdateSimulationUi();
        };
        _canvas.SelectionChanged += (_, _) =>
        {
            SyncSelectionFromCanvas();
            UpdateSegmentReorderButtons();
        };
        _canvas.StatusMessage += (_, message) => _statusLabel.Text = message;
        _canvas.UndoStateChanged += (_, _) => UpdateUndoMenuState();
        _canvas.CursorMoved += (_, cursor) =>
            _cursorLabel.Text = $"sensor ({cursor.SensorX:F0},{cursor.SensorY:F0})  local ({cursor.LocalX:F0},{cursor.LocalY:F0})";
        _canvas.FrameOriginPicked += (_, pick) => OnFrameOriginPicked(pick.SensorX, pick.SensorY);

        _frameOriginX.ValueChanged += (_, _) => OnFrameDraftChanged();
        _frameOriginY.ValueChanged += (_, _) => OnFrameDraftChanged();
        _frameAngle.ValueChanged += (_, _) => OnFrameDraftChanged();

        _btnEditFrame.Click += (_, _) => BeginFrameEdit();
        _btnApplyFrame.Click += (_, _) => ApplyFrameEdit();
        _btnCancelFrame.Click += (_, _) => CancelFrameEdit();

        _toolPalette.ToolSelected += (_, tool) => SetActiveTool(tool);
        _toolPalette.SnapToPointChanged += (_, enabled) =>
        {
            _canvas.SnapToPointEnabled = enabled;
            _canvas.Invalidate();
        };
        _toolPalette.FitRequested += (_, _) => FitImage();
        _toolPalette.SimulationPlayRequested += (_, _) => OnSimulationPlay();
        _toolPalette.SimulationPauseRequested += (_, _) =>
        {
            _simulation.Pause();
            UpdateSimulationUi();
        };
        _toolPalette.SimulationStopRequested += (_, _) => StopSimulation();
        _toolPalette.SimulationSpeedMultiplierChanged += (_, speed) => _simulation.SpeedMultiplier = speed;

        _segmentList.SelectedIndexChanged += (_, _) =>
        {
            if (_updatingUi)
            {
                return;
            }

            UpdateSegmentReorderButtons();

            if (_segmentList.SelectedIndices.Count == 1 &&
                _segmentList.SelectedItems[0].Tag is int index)
            {
                _canvas.SelectSegment(index);
            }
        };

        _btnMoveSegmentUp.Click += MoveSelectedSegmentUp;
        _btnMoveSegmentDown.Click += MoveSelectedSegmentDown;

        _simulation.FrameAdvanced += (_, _) =>
        {
            UpdateSimulationOverlay();
            UpdateSimulationUi();
        };
        _simulation.StateChanged += (_, _) => UpdateSimulationUi();

        KeyDown += OnFormKeyDown;
    }

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Z)
        {
            UndoAction(sender, e);
            e.Handled = true;
            return;
        }

        if (e.Control && e.KeyCode == Keys.Y)
        {
            RedoAction(sender, e);
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.Escape)
        {
            CancelPending(sender, e);
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.Delete)
        {
            DeleteSelectedSegment();
        }

        if (e.KeyCode == Keys.Space && ActiveControl is not NumericUpDown)
        {
            if (_simulation.State == SimulationState.Stopped)
            {
                StartSimulation();
            }
            else
            {
                _simulation.TogglePlayPause();
            }

            UpdateSimulationUi();
            e.Handled = true;
        }
    }

    private void SimulationPlay(object? sender, EventArgs e) => OnSimulationPlay();

    private void OnSimulationPlay()
    {
        if (_simulation.State == SimulationState.Paused)
        {
            _canvas.SimulationMode = true;
            _simulation.Play();
            UpdateSimulationUi();
            return;
        }

        StartSimulation();
    }

    private void SimulationPause(object? sender, EventArgs e)
    {
        _simulation.Pause();
        UpdateSimulationUi();
    }

    private void SimulationStop(object? sender, EventArgs e) => StopSimulation();

    private void StartSimulation()
    {
        if (_canvas.Document.Segments.Count == 0)
        {
            _statusLabel.Text = "Nothing to simulate — add segments first";
            UpdateSimulationUi();
            return;
        }

        PathDocument snapshot = _canvas.CreateDocumentSnapshot();
        PathSimulationTimeline timeline = PathSimulationService.BuildTimeline(snapshot);
        _simulation.LoadTimeline(timeline);
        _canvas.SimulationMode = true;
        UpdateSimulationOverlay();
        _simulation.Play();
        UpdateSimulationUi();
    }

    private void StopSimulation()
    {
        _simulation.Stop();
        _canvas.SimulationMode = false;
        _canvas.ClearSimulationOverlay();
        UpdateSimulationUi();
    }

    private void UpdateSimulationOverlay()
    {
        SimulationPose pose = _simulation.CurrentPose;
        _canvas.SimulationOverlay = new SimulationOverlay
        {
            LocalX = pose.LocalX,
            LocalY = pose.LocalY,
            GlueOn = pose.GlueOn,
            Trail = _simulation.Trail.ToList()
        };
        _canvas.Invalidate();
    }

    private void UpdateSimulationUi()
    {
        int segmentCount = _canvas.Document.Segments.Count;

        _toolPalette.SetSimulationState(new SimulationUiState
        {
            CanPlay = _simulation.State != SimulationState.Playing && segmentCount > 0,
            CanPause = _simulation.State == SimulationState.Playing,
            CanStop = _simulation.State != SimulationState.Stopped
        });
    }

    private void UndoAction(object? sender, EventArgs e)
    {
        _canvas.Undo();
        UpdateUndoMenuState();
    }

    private void RedoAction(object? sender, EventArgs e)
    {
        _canvas.Redo();
        UpdateUndoMenuState();
    }

    private void CancelPending(object? sender, EventArgs e) => _canvas.CancelPending();

    private void UpdateUndoMenuState()
    {
        if (_undoMenuItem != null)
        {
            _undoMenuItem.Enabled = _canvas.CanUndo;
        }

        if (_redoMenuItem != null)
        {
            _redoMenuItem.Enabled = _canvas.CanRedo;
        }
    }

    private void OpenAnchorImage()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All files|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        LoadImageIntoDocument(dialog.FileName, isAdditionalFov: false);
    }

    private void TryAddFovImage()
    {
        PathDocument document = _canvas.Document;
        if (document.Tiles.Count == 1 && !document.FrameIsDefined)
        {
            MessageBox.Show(
                this,
                "Define the Frame on the anchor FOV before adding another image.\n\n" +
                "Use the Frame tool on the canvas, then click Apply.",
                "Multi-FOV mosaic",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All files|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        LoadImageIntoDocument(dialog.FileName, isAdditionalFov: true);
    }

    private void LoadImageIntoDocument(string imagePath, bool isAdditionalFov)
    {
        PathDocument document = _canvas.Document;
        if (!isAdditionalFov && document.Tiles.Count == 0)
        {
            FovTile tile = TileDocumentHelper.AddTile(document, imagePath);
            if (_calibration != null)
            {
                CapturePoseHelper.ApplyCalibrationReference(tile, _calibration);
            }
        }
        else if (isAdditionalFov)
        {
            FovTile tile = TileDocumentHelper.AddTile(document, imagePath, $"FOV {document.Tiles.Count + 1}");
            if (_calibration != null)
            {
                CapturePoseHelper.ApplyCalibrationReference(tile, _calibration);
            }
        }
        else
        {
            FovTile? active = TileDocumentHelper.GetActive(document);
            if (active != null)
            {
                active.ImagePath = imagePath;
                TileDocumentHelper.SyncImagePath(document);
            }
            else
            {
                TileDocumentHelper.AddTile(document, imagePath);
            }
        }

        TileDocumentHelper.EnsureLegacyMigrated(document);
        _canvas.LoadActiveTileImage();
        RefreshTileList();
        SyncUiFromDocument();
        if (TileDocumentHelper.IsMultiFovMode(document) && _calibration?.PixelToRobot == null)
        {
            _statusLabel.Text = $"Loaded image: {imagePath} (load calibration for mosaic view)";
        }
        else
        {
            _statusLabel.Text = TileDocumentHelper.IsMultiFovMode(document)
                ? $"Added FOV image: {imagePath}"
                : $"Loaded anchor image: {imagePath}";
        }
    }

    private void LoadPath(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Path JSON|*.json|All files|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        PathDocument document = PathSerializer.Load(dialog.FileName);
        CancelFrameEdit();
        TileDocumentHelper.EnsureLegacyMigrated(document);
        _canvas.LoadDocument(document);
        _currentPathFile = dialog.FileName;
        TryLoadCalibrationFromDocument(document);
        SyncUiFromDocument();
        RefreshSegmentList();
        RefreshWaypointPreview();
        RefreshTileList();
        _statusLabel.Text = $"Loaded path: {dialog.FileName}";
    }

    private void SavePath(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentPathFile))
        {
            SavePathAs(sender, e);
            return;
        }

        WritePathFile(_currentPathFile);
    }

    private void SavePathAs(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Path JSON|*.json",
            FileName = string.IsNullOrWhiteSpace(_currentPathFile) ? "glue_path.json" : Path.GetFileName(_currentPathFile)
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        WritePathFile(dialog.FileName);
        _currentPathFile = dialog.FileName;
    }

    private void WritePathFile(string path)
    {
        PathDocument snapshot = _canvas.CreateDocumentSnapshot();
        if (!string.IsNullOrWhiteSpace(_calibrationPath))
        {
            snapshot.CalibrationPath = _calibrationPath;
        }

        PathSerializer.Save(snapshot, path);
        _canvas.Document.CalibrationPath = snapshot.CalibrationPath;
        _statusLabel.Text = $"Saved path: {path}";
    }

    private void OpenExportSettings(object? sender, EventArgs e)
    {
        if (!TryApplyExportSettings())
        {
            return;
        }

        _statusLabel.Text = "Export settings updated";
    }

    private bool TryApplyExportSettings()
    {
        using var dialog = new ExportSettingsDialog(
            _canvas.Document.Defaults,
            _calibration);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        ApplyProcessDefaults(dialog.Result);
        return true;
    }

    private void ApplyProcessDefaults(ProcessDefaults defaults)
    {
        ProcessDefaults target = _canvas.Document.Defaults;
        target.Z_mm = defaults.Z_mm;
        target.W_deg = defaults.W_deg;
        target.P_deg = defaults.P_deg;
        target.R_deg = defaults.R_deg;
        target.Speed_mm_s = defaults.Speed_mm_s;
        target.GlueOn = defaults.GlueOn;
        target.CoordinateMode = defaults.CoordinateMode;
        target.UseFovCenterNeedleOffset = defaults.UseFovCenterNeedleOffset;
        target.UseFixedCameraInversion = defaults.UseFixedCameraInversion;
        target.UseNeedleOffset = defaults.UseNeedleOffset;
        target.NeedleOffsetX_mm = defaults.NeedleOffsetX_mm;
        target.NeedleOffsetY_mm = defaults.NeedleOffsetY_mm;
        target.NeedleOffsetR_deg = defaults.NeedleOffsetR_deg;
        ExportSettingsPersistence.Save(target);
        RefreshWaypointPreview();
    }

    private void LoadCalibration(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter =
                "Calibration JSON|*.json|Robot-CV transform|RobotCvTransformCalibration.json|Compact Marker transform|transform.json|All files|*.*",
            FileName = "RobotCvTransformCalibration.json"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            ApplyLoadedCalibration(CalibrationLoader.Load(dialog.FileName), dialog.FileName);
            _statusLabel.Text = "Calibration loaded";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Load calibration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ExportCsv(object? sender, EventArgs e)
    {
        if (!TryApplyExportSettings())
        {
            return;
        }

        var waypoints = BuildExportWaypoints();
        if (waypoints.Count == 0)
        {
            MessageBox.Show(this, "No segments to export.", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "CSV|*.csv",
            FileName = "glue_path.csv"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        FanucExporter.ExportCsv(waypoints, dialog.FileName);
        _statusLabel.Text = $"Exported CSV: {dialog.FileName}";
    }

    private void ExportLs(object? sender, EventArgs e)
    {
        if (_calibration?.PixelToRobot == null)
        {
            MessageBox.Show(this, "Load a calibration JSON file first.", "Export .LS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!TryApplyExportSettings())
        {
            return;
        }

        if (_canvas.Document.Defaults.CoordinateMode != ExportCoordinateMode.UframeAbsolute)
        {
            MessageBox.Show(
                this,
                "Fanuc .LS export requires UFRAME / UTOOL absolute mode. Switch export settings or use CSV for offset modes.",
                "Export .LS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var waypoints = BuildExportWaypoints();
        if (waypoints.Count == 0)
        {
            MessageBox.Show(this, "No segments to export.", "Export .LS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "Fanuc LS|*.LS;*.ls",
            FileName = "GLUE_PATH.LS"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            FanucExporter.ExportLs(waypoints, _calibration, Path.GetFileNameWithoutExtension(dialog.FileName), dialog.FileName);
            _statusLabel.Text = $"Exported LS: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Export .LS", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteSelectedSegment(object? sender, EventArgs e) => DeleteSelectedSegment();

    private void DeleteSelectedSegment()
    {
        _canvas.DeleteSelectedSegment();
        RefreshSegmentList();
        RefreshWaypointPreview();
    }

    private void ClearSegments(object? sender, EventArgs e)
    {
        if (MessageBox.Show(this, "Clear all segments?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        _canvas.ClearSegments();
        RefreshSegmentList();
        RefreshWaypointPreview();
    }

    private void FitImage(object? sender, EventArgs e) => FitImage();

    private void FitImage() => _canvas.FitToView();

    private void RefreshWaypointPreview(object? sender, EventArgs e) => RefreshWaypointPreview();

    private void RefreshWaypointPreview()
    {
        ProcessDefaults defaults = _canvas.Document.Defaults;
        string mode = WaypointExportService.DescribeExportMode(defaults.CoordinateMode);
        string fov = defaults.UseFovCenterNeedleOffset ? "FOV ON" : "FOV OFF";
        string camera = defaults.UseFixedCameraInversion ? "Fixed camera ON" : "Fixed camera OFF";
        string needle = defaults.UseNeedleOffset
            ? $"Needle ON ({defaults.NeedleOffsetX_mm:0.###}, {defaults.NeedleOffsetY_mm:0.###})"
            : "Needle OFF";
        _previewExportModeLabel.Text = $"Export preview: {mode} | {fov} | {camera} | {needle}";

        _waypointPreview.Items.Clear();
        foreach (RobotWaypoint wp in BuildPreviewWaypoints())
        {
            var item = new ListViewItem(wp.Index.ToString());
            item.SubItems.Add(wp.Role);
            item.SubItems.Add(double.IsNaN(wp.ExportX_mm) ? "-" : wp.ExportX_mm.ToString("0.###"));
            item.SubItems.Add(double.IsNaN(wp.ExportY_mm) ? "-" : wp.ExportY_mm.ToString("0.###"));
            item.SubItems.Add(double.IsNaN(wp.ExportR_deg) ? "-" : wp.ExportR_deg.ToString("0.###"));
            item.SubItems.Add(wp.LocalX_px.ToString("0.###"));
            item.SubItems.Add(wp.LocalY_px.ToString("0.###"));
            if (wp.IsPreviewOnly)
            {
                item.ForeColor = Color.DimGray;
            }

            _waypointPreview.Items.Add(item);
        }
    }

    private IReadOnlyList<RobotWaypoint> BuildPreviewWaypoints()
    {
        PathDocument snapshot = _canvas.CreateDocumentSnapshot();
        return GeometryHelper.BuildPreviewWaypoints(snapshot, _calibration);
    }

    private IReadOnlyList<RobotWaypoint> BuildExportWaypoints()
    {
        PathDocument snapshot = _canvas.CreateDocumentSnapshot();
        return GeometryHelper.BuildWaypoints(snapshot, _calibration);
    }

    private void RefreshSegmentList()
    {
        int selected = _canvas.SelectedSegmentIndex ?? (
            _segmentList.SelectedIndices.Count > 0 ? _segmentList.SelectedIndices[0] : -1);
        IReadOnlyList<SequenceGap> gaps = PathSequenceService.AnalyzeGaps(_canvas.Document.Segments);
        _segmentList.Items.Clear();
        for (int i = 0; i < _canvas.Document.Segments.Count; i++)
        {
            PathSegment segment = _canvas.Document.Segments[i];
            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(segment.Kind.ToString());
            item.SubItems.Add(segment.DisplayName);
            if (i < gaps.Count)
            {
                SequenceGap gap = gaps[i];
                item.SubItems.Add(FormatGapDistance(gap.Distance_px));
                if (gap.HasGap(PathSequenceService.DefaultGapThresholdPx))
                {
                    item.ForeColor = Color.DarkOrange;
                }
            }
            else
            {
                item.SubItems.Add("—");
            }

            item.Tag = i;
            _segmentList.Items.Add(item);
        }

        if (selected >= 0 && selected < _segmentList.Items.Count)
        {
            _segmentList.Items[selected].Selected = true;
        }

        UpdateSegmentGapSummary(gaps);
        UpdateSegmentReorderButtons();
    }

    private static string FormatGapDistance(double distancePx) =>
        distancePx < 0.05 ? "0 px" : $"{distancePx:F1} px";

    private void UpdateSegmentGapSummary(IReadOnlyList<SequenceGap> gaps)
    {
        int gapCount = gaps.Count(gap => gap.HasGap(PathSequenceService.DefaultGapThresholdPx));
        if (gaps.Count == 0)
        {
            _segmentGapSummaryLabel.Text = "Sequence: single segment";
            _segmentGapSummaryLabel.ForeColor = Color.DimGray;
            return;
        }

        if (gapCount == 0)
        {
            _segmentGapSummaryLabel.Text =
                $"Sequence: all connected (≤ {PathSequenceService.DefaultGapThresholdPx:F0} px)";
            _segmentGapSummaryLabel.ForeColor = Color.DimGray;
            return;
        }

        _segmentGapSummaryLabel.Text =
            $"{gapCount} sequence gap(s) (> {PathSequenceService.DefaultGapThresholdPx:F0} px)";
        _segmentGapSummaryLabel.ForeColor = Color.DarkOrange;
    }

    private void UpdateSegmentReorderButtons()
    {
        if (_segmentList.SelectedIndices.Count != 1 ||
            _segmentList.SelectedItems[0].Tag is not int index)
        {
            _btnMoveSegmentUp.Enabled = false;
            _btnMoveSegmentDown.Enabled = false;
            return;
        }

        _btnMoveSegmentUp.Enabled = PathSequenceService.CanMoveSegment(_canvas.Document.Segments.Count, index, -1);
        _btnMoveSegmentDown.Enabled = PathSequenceService.CanMoveSegment(_canvas.Document.Segments.Count, index, 1);
    }

    private void MoveSelectedSegmentUp(object? sender, EventArgs e) => MoveSelectedSegment(-1);

    private void MoveSelectedSegmentDown(object? sender, EventArgs e) => MoveSelectedSegment(1);

    private void MoveSelectedSegment(int delta)
    {
        if (_segmentList.SelectedIndices.Count != 1 ||
            _segmentList.SelectedItems[0].Tag is not int index)
        {
            return;
        }

        _canvas.TryMoveSegment(index, delta);
    }

    private void SyncUiFromDocument()
    {
        _updatingUi = true;
        try
        {
            _frameOriginX.Value = Clamp(_frameOriginX, (decimal)_canvas.Document.Frame.OriginX_px);
            _frameOriginY.Value = Clamp(_frameOriginY, (decimal)_canvas.Document.Frame.OriginY_px);
            _frameAngle.Value = Clamp(_frameAngle, (decimal)_canvas.Document.Frame.AngleDeg);
        }
        finally
        {
            _updatingUi = false;
        }
    }

    private void SyncSelectionFromCanvas()
    {
        _updatingUi = true;
        try
        {
            _segmentList.SelectedIndices.Clear();
            if (_canvas.SelectedSegmentIndex is int index &&
                index >= 0 &&
                index < _segmentList.Items.Count)
            {
                _segmentList.Items[index].Selected = true;
                _segmentList.Items[index].Focused = true;
            }
        }
        finally
        {
            _updatingUi = false;
        }
    }

    private void BeginFrameEdit()
    {
        _frameEditing = true;
        SetFrameEditorsEnabled(true);
        OnFrameDraftChanged();
        _statusLabel.Text = "Editing frame — Apply or Cancel";
    }

    private void ApplyFrameEdit()
    {
        if (!_frameEditing)
        {
            return;
        }

        bool moveShapes = _radioMoveShapes.Checked;
        _canvas.ApplyFrameChange(
            (double)_frameOriginX.Value,
            (double)_frameOriginY.Value,
            (double)_frameAngle.Value,
            moveShapes);

        _canvas.Document.FrameIsDefined = true;
        _frameEditing = false;
        SetFrameEditorsEnabled(false);
        _canvas.ClearDraftFrame();
        UpdateUndoMenuState();
        RefreshTileList();
        _statusLabel.Text = moveShapes
            ? "Frame applied — shapes rebinding kept sensor px"
            : "Frame applied — local px unchanged";
    }

    private void CancelFrameEdit()
    {
        if (!_frameEditing)
        {
            _canvas.ClearDraftFrame();
            return;
        }

        _frameEditing = false;
        SetFrameEditorsEnabled(false);
        _canvas.ClearDraftFrame();
        SyncUiFromDocument();
        _statusLabel.Text = "Frame edit cancelled";
    }

    private void OnFrameDraftChanged()
    {
        if (!_frameEditing || _updatingUi)
        {
            return;
        }

        _canvas.SetDraftFrame(
            (double)_frameOriginX.Value,
            (double)_frameOriginY.Value,
            (double)_frameAngle.Value);
    }

    private void OnFrameOriginPicked(double sensorX, double sensorY)
    {
        if (!_frameEditing)
        {
            BeginFrameEdit();
        }

        _updatingUi = true;
        try
        {
            _frameOriginX.Value = Clamp(_frameOriginX, (decimal)Math.Round(sensorX));
            _frameOriginY.Value = Clamp(_frameOriginY, (decimal)Math.Round(sensorY));
        }
        finally
        {
            _updatingUi = false;
        }

        OnFrameDraftChanged();
        _statusLabel.Text = $"Frame origin draft: ({Math.Round(sensorX)}, {Math.Round(sensorY)}) px";
    }

    private void SetActiveTool(DrawTool tool)
    {
        _canvas.ActiveTool = tool;
        _toolPalette.SetActiveTool(tool);
        _canvas.CancelPending();
        _statusLabel.Text = $"Tool: {tool}";
    }

    private void RefreshTileList()
    {
        PathDocument document = _canvas.Document;
        string? selectedId = GetSelectedTileId();
        _tileList.Items.Clear();
        string? activeId = document.ActiveTileId;

        foreach (FovTile tile in document.Tiles)
        {
            var item = new ListViewItem(tile.Name);
            item.SubItems.Add(tile.RobotX_mm.ToString("0.##"));
            item.SubItems.Add(tile.RobotY_mm.ToString("0.##"));
            item.SubItems.Add(tile.RobotR_deg.ToString("0.##"));
            item.SubItems.Add(tile.Id == activeId ? "yes" : string.Empty);
            item.Tag = tile.Id;
            _tileList.Items.Add(item);
        }

        if (!string.IsNullOrWhiteSpace(selectedId))
        {
            foreach (ListViewItem item in _tileList.Items)
            {
                if (item.Tag is string id && id == selectedId)
                {
                    item.Selected = true;
                    break;
                }
            }
        }
        else if (_tileList.Items.Count > 0)
        {
            string? preferredId = TileDocumentHelper.GetAnchor(document)?.Id
                ?? document.ActiveTileId
                ?? _tileList.Items[0].Tag as string;

            foreach (ListViewItem item in _tileList.Items)
            {
                if (item.Tag is string id && id == preferredId)
                {
                    item.Selected = true;
                    break;
                }
            }
        }

        UpdateFovModeUi();
        UpdateTileTransformInfo();
    }

    private void UpdateFovModeUi()
    {
        PathDocument document = _canvas.Document;
        bool hasTiles = document.Tiles.Count > 0;
        bool multi = TileDocumentHelper.IsMultiFovMode(document);

        _tileModeLabel.Text = multi ? "Mode: Multi-FOV mosaic" : "Mode: Single FOV";
        _tileCaptureHint.Text = multi
            ? "Anchor FOV is the first image. Frame and path use anchor coordinates. Additional FOV tiles are placed on the virtual mosaic canvas."
            : hasTiles
                ? document.FrameIsDefined
                    ? "Single FOV mode. You can add another FOV image to switch to multi-FOV mosaic."
                    : "Single FOV mode. Define Frame on the anchor image before adding more FOV images."
                : "Single FOV mode. Open the anchor image to begin.";

        if (_tileOpenImageButton != null)
        {
            _tileOpenImageButton.Visible = !hasTiles;
        }

        if (_tileAddFovButton != null)
        {
            _tileAddFovButton.Visible = hasTiles;
            _tileAddFovButton.Enabled = TileDocumentHelper.CanAddAnotherFovImage(document);
        }
    }

    private void OnResetCapturePoseToCalibrationReference(object? sender, EventArgs e)
    {
        if (_calibration == null)
        {
            MessageBox.Show(this, "Load calibration JSON first.", "Capture pose", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        FovTile? tile = GetSelectedTile() ?? TileDocumentHelper.GetAnchor(_canvas.Document);
        if (tile == null)
        {
            return;
        }

        CapturePoseHelper.ApplyCalibrationReference(tile, _calibration);
        _canvas.RebuildMosaic(fitView: false);
        RefreshTileList();
        SyncTileEditorsFromSelection();
        RefreshWaypointPreview();
        _statusLabel.Text = "Capture pose reset to calibration reference";
    }

    private string? GetSelectedTileId()
    {
        if (_tileList.SelectedItems.Count == 0)
        {
            return null;
        }

        return _tileList.SelectedItems[0].Tag as string;
    }

    private FovTile? GetSelectedTile() =>
        GetSelectedTileId() is string id
            ? _canvas.Document.Tiles.FirstOrDefault(tile => tile.Id == id)
            : null;

    private void SyncTileEditorsFromSelection()
    {
        _updatingUi = true;
        try
        {
            FovTile? tile = GetSelectedTile();
            SetTileEditorsEnabled(tile != null);
            if (tile == null)
            {
                _tileOffsetLabel.Text = "Canvas offset: —";
                _tileWorldLabel.Text = "Tile (0,0) world: —";
                return;
            }

            _tileRobotX.Value = Clamp(_tileRobotX, (decimal)tile.RobotX_mm);
            _tileRobotY.Value = Clamp(_tileRobotY, (decimal)tile.RobotY_mm);
            _tileRobotR.Value = Clamp(_tileRobotR, (decimal)tile.RobotR_deg);
            UpdateTileTransformInfo();
        }
        finally
        {
            _updatingUi = false;
        }
    }

    private void ApplyTilePoseFromEditors()
    {
        if (_updatingUi)
        {
            return;
        }

        FovTile? tile = GetSelectedTile();
        if (tile == null)
        {
            return;
        }

        tile.RobotX_mm = (double)_tileRobotX.Value;
        tile.RobotY_mm = (double)_tileRobotY.Value;
        tile.RobotR_deg = (double)_tileRobotR.Value;
        _canvas.RebuildMosaic(fitView: false);
        RefreshTileList();
        RefreshWaypointPreview();
    }

    private void UpdateTileTransformInfo()
    {
        FovTile? tile = GetSelectedTile();
        FovTile? anchor = TileDocumentHelper.GetAnchor(_canvas.Document);
        if (tile == null || anchor == null)
        {
            _tileOffsetLabel.Text = "Canvas offset: —";
            _tileWorldLabel.Text = "Tile (0,0) world: —";
            return;
        }

        if (_calibration?.PixelToRobot == null)
        {
            _tileOffsetLabel.Text = "Canvas offset: load calibration";
            _tileWorldLabel.Text = "Tile (0,0) world: load calibration";
            return;
        }

        try
        {
            (double worldX, double worldY) = TileTransformService.TilePixelToWorld(0, 0, tile, _calibration);
            _tileWorldLabel.Text = $"Tile (0,0) world: ({worldX:0.##}, {worldY:0.##}) mm";

            if (tile.Id == anchor.Id)
            {
                _tileOffsetLabel.Text = "Canvas offset: (0, 0) px — anchor";
                return;
            }

            (double offsetX, double offsetY) = TileTransformService.GetTileCanvasOffset(tile, anchor, _calibration);
            _tileOffsetLabel.Text = $"Canvas offset: ({offsetX:0.#}, {offsetY:0.#}) px";
        }
        catch (Exception ex)
        {
            _tileOffsetLabel.Text = $"Canvas offset: {ex.Message}";
            _tileWorldLabel.Text = "Tile (0,0) world: —";
        }
    }

    private void SetTileEditorsEnabled(bool enabled)
    {
        _tileRobotX.Enabled = enabled;
        _tileRobotY.Enabled = enabled;
        _tileRobotR.Enabled = enabled;
    }

    private void OnTileButtonClick(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.Tag is not string action)
        {
            return;
        }

        string? tileId = GetSelectedTileId();
        switch (action)
        {
            case "open":
                OpenAnchorImage();
                break;

            case "add":
                TryAddFovImage();
                break;

            case "remove":
                if (tileId == null)
                {
                    return;
                }

                if (MessageBox.Show(this, "Remove selected tile?", "FOV Tiles", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                TileDocumentHelper.RemoveTile(_canvas.Document, tileId);
                _canvas.RebuildMosaic();
                RefreshTileList();
                SyncUiFromDocument();
                _statusLabel.Text = "FOV image removed";
                break;

            case "activate":
                if (tileId == null)
                {
                    return;
                }

                TileDocumentHelper.SetActive(_canvas.Document, tileId);
                _canvas.FocusTileView(tileId);
                RefreshTileList();
                _statusLabel.Text = "Focused tile view";
                break;
        }
    }

    private void TryLoadCalibrationFromDocument(PathDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.CalibrationPath) || !File.Exists(document.CalibrationPath))
        {
            return;
        }

        try
        {
            ApplyLoadedCalibration(CalibrationLoader.Load(document.CalibrationPath), document.CalibrationPath);
        }
        catch
        {
            _statusLabel.Text = "Path references calibration file that could not be loaded";
        }
    }

    private void ApplyLoadedCalibration(RobotCalibrationFile calibration, string path)
    {
        _calibration = calibration;
        _calibrationPath = path;
        _canvas.Document.CalibrationPath = path;
        _calibrationLabel.Text = CalibrationLoader.FormatSummary(calibration, path);
        CalibrationLoader.ApplyCompactMarkerProcessDefaults(calibration, _canvas.Document.Defaults);

        FovTile? anchor = TileDocumentHelper.GetAnchor(_canvas.Document);
        if (anchor != null && CapturePoseHelper.IsUnsetCapturePose(anchor))
        {
            CapturePoseHelper.ApplyCalibrationReference(anchor, calibration);
        }

        _canvas.SetMosaicCalibration(_calibration);
        RefreshTileList();
        SyncTileEditorsFromSelection();
        RefreshWaypointPreview();
    }

    private static NumericUpDown CreateNumeric(decimal min, decimal max, decimal increment, int decimals = 0)
    {
        return new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            Increment = increment,
            DecimalPlaces = decimals,
            Width = 120
        };
    }

    private static NumericUpDown CreateNumeric(int min, int max, int increment) =>
        CreateNumeric((decimal)min, (decimal)max, increment, 0);

    private static decimal Clamp(NumericUpDown control, decimal value) =>
        Math.Max(control.Minimum, Math.Min(control.Maximum, value));

    private static ToolStripMenuItem CreateMenuItem(string text, EventHandler handler, Keys? shortcut = null)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += handler;
        if (shortcut is Keys keys && IsValidMenuShortcut(keys))
        {
            item.ShortcutKeys = keys;
        }

        return item;
    }

    /// <summary>
    /// WinForms menu shortcuts require a modifier (Ctrl/Alt/Shift) or an F-key.
    /// Bare keys like Esc/Delete must be handled via KeyDown only.
    /// </summary>
    private static bool IsValidMenuShortcut(Keys keys)
    {
        Keys modifiers = keys & Keys.Modifiers;
        if (modifiers != Keys.None)
        {
            return true;
        }

        Keys keyCode = keys & Keys.KeyCode;
        return keyCode is >= Keys.F1 and <= Keys.F24;
    }

    private static ToolStripMenuItem CreateMenu(string text, ToolStripItem[] items)
    {
        var menu = new ToolStripMenuItem(text);
        menu.DropDownItems.AddRange(items);
        return menu;
    }
}
