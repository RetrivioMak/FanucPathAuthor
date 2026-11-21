using System.Text.Json;
using DispensePathAuthor.Contracts;
using DispensePathAuthor.Controls;
using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor;

public partial class MainForm : Form
{
    private readonly PathCanvas _canvas = new() { Dock = DockStyle.Fill };
    private readonly ToolPalettePanel _toolPalette = new();
    private readonly PathSimulationController _simulation = new();
    private CvProjectPanel? _cvPanel;
    private TeachProjectPanel? _teachPanel;
    private TabPage? _projectPage;

    private readonly IReadOnlyList<string>? _pluginNotes;
    private bool _offerStartup;
    private bool _startupOffered;
    private bool _pluginsShown;
    private bool _suppressCloseConfirm;
    private ProjectSession? _session;
    private RobotCalibrationFile? _draftCalibration;
    private string? _draftCalibrationPath;
    private string? _draftFilePath;
    private string? _packageCacheDirectory;
    private List<double> _gridSteps = new();
    private bool _updatingGrid;
    private double _toolWidthMm = ProcessDefaults.DefaultMinWidthMm;
    private bool _updatingWidth;
    private readonly ToolStripMenuItem _projectSettingsMenuItem = new("Project settings...");
    private readonly ToolStripMenuItem _widthMenuItem = new("Width");

    private RobotCalibrationFile? _calibration
    {
        get => _session?.Calibration ?? _draftCalibration;
        set
        {
            if (_session == null)
            {
                _draftCalibration = value;
                return;
            }

            _session.Calibration = value;
        }
    }

    private string? _calibrationPath
    {
        get => _session?.CalibrationPath ?? _draftCalibrationPath;
        set
        {
            if (_session == null)
            {
                _draftCalibrationPath = value;
                return;
            }

            _session.CalibrationPath = value;
        }
    }

    private string? _currentPathFile
    {
        get => _session?.FilePath ?? _draftFilePath;
        set
        {
            if (_session == null)
            {
                _draftFilePath = value;
                return;
            }

            _session.FilePath = value;
        }
    }

    public MainForm()
    {
        InitializeComponent();
        Text = AppVersion.WindowTitle;
        _canvasHost.Controls.Add(_canvas);
        _paletteHost.Controls.Add(_toolPalette);
        var resizeGrip = new PanelResizeGrip
        {
            TargetPanel = _rightPanel,
            MinTargetWidth = 320,
            MaxTargetWidth = 1200
        };
        _rightPanel.Controls.Add(resizeGrip);
        FormClosed += (_, _) => _simulation.Dispose();
        WireEvents();
        ExportSettingsPersistence.TryLoadInto(_canvas.Document.Exporters.FanucLs);
        RefreshSegmentList();
        RefreshWaypointPreview();
        UpdateUndoMenuState();
        SetActiveTool(DrawTool.Dot);
        _canvas.ResolveStoredFile = ResolveStoredFile;
        _statusLabel.Text = "Choose a project to begin.";
    }

    public MainForm(IReadOnlyList<string> pluginNotes)
        : this()
    {
        _pluginNotes = pluginNotes;
        _offerStartup = true;
    }

    public MainForm(ProjectStart start, IReadOnlyList<string>? pluginNotes = null)
        : this()
    {
        _pluginNotes = pluginNotes;
        ApplyProjectStart(start);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_offerStartup && !_startupOffered)
        {
            _startupOffered = true;
            BeginInvoke(OfferStartup);
            return;
        }

        ShowPluginNotes();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (ShouldConfirmClose(e) &&
            MessageBox.Show(
                this,
                "Close the program?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            e.Cancel = true;
            return;
        }

        ExportSettingsPersistence.Save(_canvas.Document.Exporters.FanucLs);
        base.OnFormClosing(e);
    }

    private bool ShouldConfirmClose(FormClosingEventArgs e) =>
        !_suppressCloseConfirm &&
        e.CloseReason == CloseReason.UserClosing &&
        !DesignMode;

    private void WireEvents()
    {
        _newProjectMenuItem.Click += NewProject;
        _loadPathMenuItem.Click += LoadPath;
        _savePathMenuItem.Click += SavePath;
        _savePathAsMenuItem.Click += SavePathAs;
        _loadCalibrationMenuItem.Click += LoadCalibration;
        _projectSettingsMenuItem.Click += OpenProjectSettings;
        int calibrationIndex = _fileMenu.DropDownItems.IndexOf(_loadCalibrationMenuItem);
        _fileMenu.DropDownItems.Insert(calibrationIndex + 1, _projectSettingsMenuItem);
        _exportSettingsMenuItem.Click += OpenExportSettings;
        _exportCsvMenuItem.Click += ExportCsv;
        BuildExporterMenu();
        BuildImporterMenu();
        _undoMenuItem.Click += UndoAction;
        _redoMenuItem.Click += RedoAction;
        _cancelShapeMenuItem.Click += CancelPending;
        _deleteSegmentMenuItem.Click += DeleteSelectedSegment;
        _clearSegmentsMenuItem.Click += ClearSegments;
        int clearIndex = _editMenu.DropDownItems.IndexOf(_clearSegmentsMenuItem);
        _editMenu.DropDownItems.Insert(clearIndex + 1, _widthMenuItem);
        _fitViewMenuItem.Click += FitImage;
        _refreshPreviewMenuItem.Click += RefreshWaypointPreview;
        _simulatePlayMenuItem.Click += SimulationPlay;
        _simulatePauseMenuItem.Click += SimulationPause;
        _simulateStopMenuItem.Click += SimulationStop;

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
            RefreshTeachPointList();
            UpdateSimulationUi();
        };
        _canvas.SelectionChanged += (_, _) => SyncSelectionFromCanvas();
        _canvas.StatusMessage += (_, message) => _statusLabel.Text = message;
        _canvas.UndoStateChanged += (_, _) => UpdateUndoMenuState();
        _canvas.CursorMoved += (_, cursor) =>
            _cursorLabel.Text = _canvas.FormatCursor(cursor.SensorX, cursor.SensorY, cursor.LocalX, cursor.LocalY);
        _canvas.FrameOriginPicked += (_, pick) => OnFrameOriginPicked(pick.SensorX, pick.SensorY);

        _toolPalette.ToolSelected += (_, tool) => SetActiveTool(tool);
        _toolPalette.SnapToPointChanged += (_, enabled) =>
        {
            _canvas.SnapToPointEnabled = enabled;
            _canvas.Invalidate();
        };
        _toolPalette.ShowGridChanged += (_, show) =>
        {
            if (_updatingGrid)
            {
                return;
            }

            _canvas.SetGridEnabled(show);
            _showGridMenuItem.Checked = _canvas.Document.Grid.Enabled;
            if (_showGridMenuItem.Checked != show)
            {
                _toolPalette.SetShowGrid(_showGridMenuItem.Checked);
            }
        };
        _toolPalette.GridStepChanged += (_, step) =>
        {
            if (_updatingGrid)
            {
                return;
            }

            ApplyGridStep(step);
            BeginInvoke(SyncGridUi);
        };
        _toolPalette.WidthChanged += (_, width) =>
        {
            if (_updatingWidth)
            {
                return;
            }

            _toolWidthMm = width;
            _canvas.SetToolWidth(width);
            BeginInvoke(SyncWidthUi);
        };
        _showGridMenuItem.Click += (_, _) =>
        {
            if (_updatingGrid)
            {
                return;
            }

            _canvas.SetGridEnabled(_showGridMenuItem.Checked);
            SyncGridUi();
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

        _segmentListPanel.SelectionChanged += (_, _) =>
        {
            if (_segmentListPanel.SelectedIndex is int index)
            {
                _canvas.SelectSegment(index);
            }
        };
        _segmentListPanel.MoveUpRequested += (_, _) => MoveSelectedSegment(-1);
        _segmentListPanel.MoveDownRequested += (_, _) => MoveSelectedSegment(1);
        _segmentListPanel.PropertiesEdited += (_, edit) =>
            _canvas.TrySetSegmentProperty(edit.Index, edit.Field, edit.Number, edit.Flag);

        _simulation.FrameAdvanced += (_, _) =>
        {
            UpdateSimulationOverlay();
            UpdateSimulationUi();
        };
        _simulation.StateChanged += (_, _) => UpdateSimulationUi();

        KeyDown += OnFormKeyDown;
        StrokeWidths.EnsureStored(_canvas.Document.Defaults);
        ResetToolWidth();
        SyncGridUi();
        SyncWidthUi();
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
            if (_segmentListPanel.IsEditingProperties)
            {
                return;
            }

            DeleteSelectedSegment();
        }

        if (e.KeyCode == Keys.Space &&
            ActiveControl is not NumericUpDown &&
            !_segmentListPanel.IsEditingProperties)
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
        if (FrameMillimeters.StoresMillimeters(snapshot) &&
            FrameMillimeters.TryGetScale(snapshot.Frame, _calibration, out FrameAxisScale scale))
        {
            foreach (PathSegment segment in snapshot.Segments)
            {
                FrameMillimeters.ConvertPoints(segment, (x, y) => FrameMillimeters.ToLocalPixels(x, y, scale));
            }
        }

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
        if (!EnsureWorkspace(WorkspaceKind.Cv))
        {
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

        LoadImageIntoDocument(dialog.FileName, isAdditionalFov: false);
    }

    private void TryAddFovImage()
    {
        if (!EnsureWorkspace(WorkspaceKind.Cv))
        {
            return;
        }

        PathDocument document = _canvas.Document;
        if (!TileDocumentHelper.CanAddAnotherFovImage(document))
        {
            MessageBox.Show(
                this,
                "The Frame is not defined yet, so another FOV image cannot be added.\n\n" +
                "In Frame (sensor px) above the tabs, click Edit Frame, set the origin on the anchor image, then click Apply.",
                "Frame is not defined",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
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
        AttachSession(WorkspaceKind.Cv);
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
            Filter = ProjectPackage.OpenFilter
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            LoadPathFromFile(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Open project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadPathFromFile(string filePath)
    {
        LoadedProject loaded = ProjectPackage.LoadFile(filePath);
        _packageCacheDirectory = loaded.CacheDirectory;
        PathDocument document = loaded.Document;
        CancelFrameEdit();
        TileDocumentHelper.EnsureLegacyMigrated(document);
        _canvas.UseSpace(document.Workspace == WorkspaceKind.Teach
            ? new TeachCanvasSpace()
            : new CvCanvasSpace());
        _canvas.LoadDocument(document);
        _draftCalibration = null;
        _draftCalibrationPath = null;
        _draftFilePath = null;
        _session = document.Workspace == WorkspaceKind.Unset
            ? null
            : ProjectSession.Bind(_canvas.Document);
        ShowProjectPanel(document.Workspace);
        _currentPathFile = filePath;
        TryLoadCalibrationFromDocument(document);
        MigrateOpenPath(recordUndo: false);
        ResetToolWidth();
        SyncUiFromDocument();
        RefreshSegmentList();
        RefreshWaypointPreview();
        RefreshTileList();
        RefreshTeachPointList();
        _statusLabel.Text = $"Loaded project: {filePath}";
    }

    private void SavePath(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentPathFile) ||
            !ProjectPackage.HasPackageExtension(_currentPathFile))
        {
            if (!string.IsNullOrWhiteSpace(_currentPathFile))
            {
                WritePackage(Path.ChangeExtension(_currentPathFile, ProjectPackage.Extension));
                return;
            }

            SavePathAs(sender, e);
            return;
        }

        WritePackage(_currentPathFile);
    }

    private void SavePathAs(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = ProjectPackage.SaveFilter,
            DefaultExt = "dpath",
            AddExtension = true,
            FileName = string.IsNullOrWhiteSpace(_currentPathFile)
                ? "project.dpath"
                : Path.ChangeExtension(Path.GetFileName(_currentPathFile), ProjectPackage.Extension)
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        WritePackage(dialog.FileName);
    }

    private void WritePackage(string path)
    {
        try
        {
            PathDocument snapshot = _canvas.CreateDocumentSnapshot();
            if (!string.IsNullOrWhiteSpace(_calibrationPath))
            {
                snapshot.CalibrationPath = _calibrationPath;
            }

            SavedPackage saved = ProjectPackage.Save(snapshot, path, _packageCacheDirectory);
            _packageCacheDirectory = saved.CacheDirectory;
            ApplySavedPackage(saved.Document);
            _currentPathFile = path;
            _statusLabel.Text = $"Saved project: {path}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Save project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ApplySavedPackage(PathDocument saved)
    {
        foreach (FovTile savedTile in saved.Tiles)
        {
            FovTile? live = _canvas.Document.Tiles.FirstOrDefault(tile => tile.Id == savedTile.Id);
            if (live != null)
            {
                live.ImagePath = savedTile.ImagePath;
            }
        }

        TileDocumentHelper.SyncImagePath(_canvas.Document);
        _canvas.Document.SchemaVersion = saved.SchemaVersion;
        _canvas.Document.CalibrationPath = saved.CalibrationPath;
        _calibrationPath = saved.CalibrationPath;
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
            _canvas.Document.Exporters.FanucLs,
            _calibration,
            _canvas.Document.Workspace == WorkspaceKind.Teach,
            ExporterRegistry.Exporters,
            _canvas.Document.Exporters);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        ApplyProcessDefaults(dialog.Result);
        ApplyFanucSettings(dialog.FanucResult);
        foreach ((string id, JsonElement section) in dialog.PluginSections)
        {
            _canvas.Document.Exporters.SetSection(id, section);
        }

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
        RefreshWaypointPreview();
    }

    private void ApplyFanucSettings(FanucLsSettings fanuc)
    {
        _canvas.Document.Exporters.FanucLs = fanuc.Clone();
        ExportSettingsPersistence.Save(_canvas.Document.Exporters.FanucLs);
        RefreshWaypointPreview();
    }

    private void LoadCalibration(object? sender, EventArgs e)
    {
        if (_canvas.Document.Workspace == WorkspaceKind.Teach)
        {
            MessageBox.Show(
                this,
                "A Teach project already uses robot millimeters. Calibration is not applied.",
                "Load calibration",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

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
            ApplyLoadedCalibration(CalibrationLoader.Load(dialog.FileName), dialog.FileName, recordUndo: true);
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

        DiagnosticCsv.Write(waypoints, dialog.FileName);
        _statusLabel.Text = $"Exported CSV: {dialog.FileName}";
    }

    private void ExportWith(IProgramExporter exporter)
    {
        if (!TryApplyExportSettings())
        {
            return;
        }

        string? error = exporter.Validate(CreateExportRequest(exporter, string.Empty));
        if (error != null)
        {
            MessageBox.Show(this, error, exporter.MenuLabel, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_canvas.Document.Segments.Count == 0)
        {
            MessageBox.Show(this, "No segments to export.", exporter.MenuLabel, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = exporter.FileFilter,
            FileName = exporter.DefaultFileName
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            exporter.Export(CreateExportRequest(exporter, dialog.FileName));
            _statusLabel.Text = $"Exported: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, exporter.MenuLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private ProgramExportRequest CreateExportRequest(IProgramExporter exporter, string destination)
    {
        PathDocument snapshot = _canvas.CreateDocumentSnapshot();
        FrameTrajectoryResult trajectory = FrameTrajectory.Build(snapshot, _calibration);
        return new ProgramExportRequest
        {
            Moves = trajectory.Moves,
            Settings = snapshot.Exporters.Section(exporter.Id),
            DestinationPath = destination,
            TrajectoryError = trajectory.Error,
            Host = new ExportHost
            {
                Document = snapshot,
                Calibration = _calibration
            }
        };
    }

    private void BuildImporterMenu()
    {
        var importMenu = new ToolStripMenuItem("Import") { Name = "importMenu" };
        foreach (IGeometryImporter importer in ImporterRegistry.Importers)
        {
            IGeometryImporter captured = importer;
            var item = new ToolStripMenuItem(captured.MenuLabel) { Name = captured.Id + "ImportMenuItem" };
            item.Click += (_, _) => ImportWith(captured);
            importMenu.DropDownItems.Add(item);
        }

        _menuStrip.Items.Insert(1, importMenu);
    }

    private void ImportWith(IGeometryImporter importer)
    {
        string? reason = importer.UnavailableReason(_canvas.Document.Workspace);
        if (reason != null)
        {
            MessageBox.Show(this, reason, importer.MenuLabel, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!EnsureMillimeterPath())
        {
            return;
        }

        using var fileDialog = new OpenFileDialog
        {
            Filter = importer.FileFilter,
            Title = importer.MenuLabel
        };
        if (fileDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        GeometryImportSurvey survey;
        try
        {
            survey = importer.Survey(fileDialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, importer.MenuLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var page = new GeometryImportDialog(survey);
        if (page.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        GeometryImportResult result;
        try
        {
            result = importer.Import(
                fileDialog.FileName,
                new GeometryImportOptions
                {
                    Layers = page.SelectedLayers,
                    Unit = page.Unit,
                    OriginX_mm = page.OriginX_mm,
                    OriginY_mm = page.OriginY_mm
                },
                _canvas.Document.Defaults.GlueOn,
                _canvas.Document.Defaults.Speed_mm_s);
            foreach (PathSegment segment in result.Segments)
            {
                if (segment.Kind != SegmentKind.Dot)
                {
                    segment.Width_mm = _toolWidthMm;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, importer.MenuLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _canvas.AppendSegments(result.Segments);
        if (result.Segments.Count > 0)
        {
            _canvas.FitToView();
        }

        _statusLabel.Text = $"Imported {result.Segments.Count} segments.";
        if (result.Warnings.Count > 0)
        {
            MessageBox.Show(
                this,
                string.Join(Environment.NewLine, result.Warnings),
                importer.MenuLabel,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private void MigrateOpenPath(bool recordUndo)
    {
        _canvas.AdoptFrameMillimeters(_calibration, recordUndo);
    }

    private bool EnsureMillimeterPath()
    {
        MigrateOpenPath(recordUndo: true);
        if (_canvas.Document.Workspace != WorkspaceKind.Cv ||
            _canvas.Document.SchemaVersion >= FrameMillimeters.SchemaVersion)
        {
            return true;
        }

        MessageBox.Show(
            this,
            "Load calibration before importing a DXF. The current CV path is still stored in pixels.",
            "Import DXF",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        return false;
    }

    private void BuildExporterMenu()
    {
        int index = _fileMenu.DropDownItems.IndexOf(_exportLsMenuItem);
        _fileMenu.DropDownItems.Remove(_exportLsMenuItem);
        if (index < 0)
        {
            index = _fileMenu.DropDownItems.IndexOf(_exportCsvMenuItem) + 1;
        }

        foreach (IProgramExporter exporter in ExporterRegistry.Exporters)
        {
            IProgramExporter captured = exporter;
            var item = new ToolStripMenuItem(captured.MenuLabel);
            item.Click += (_, _) => ExportWith(captured);
            _fileMenu.DropDownItems.Insert(index, item);
            index++;
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

    private void SetActiveTool(DrawTool tool)
    {
        _canvas.ActiveTool = tool;
        _toolPalette.SetActiveTool(tool);
        _canvas.CancelPending();
        _statusLabel.Text = $"Tool: {tool}";
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

    private void RefreshWaypointPreview()
    {
        FanucLsSettings fanuc = _canvas.Document.Exporters.FanucLs;
        string summary;
        if (_canvas.Document.Workspace == WorkspaceKind.Teach)
        {
            string teachNeedle = fanuc.UseNeedleOffset
                ? $"Needle ON ({fanuc.NeedleOffsetX_mm:0.###}, {fanuc.NeedleOffsetY_mm:0.###})"
                : "Needle OFF";
            summary = $"Export preview: Teach absolute mm | {teachNeedle}";
        }
        else
        {
            string mode = WaypointExportService.DescribeExportMode(fanuc.CoordinateMode);
            string fov = fanuc.UseFovCenterNeedleOffset ? "FOV ON" : "FOV OFF";
            string camera = WaypointExportService.DescribeLocalRotation(fanuc.LocalRotation);
            string needle = fanuc.UseNeedleOffset
                ? $"Needle ON ({fanuc.NeedleOffsetX_mm:0.###}, {fanuc.NeedleOffsetY_mm:0.###})"
                : "Needle OFF";
            summary = $"Export preview: {mode} | {fov} | {camera} | {needle}";
        }

        _waypointPreviewPanel.Show(summary, BuildPreviewWaypoints());
    }

    private void RefreshSegmentList() =>
        _segmentListPanel.Show(_canvas.Document, _canvas.SelectedSegmentIndex);

    private void SyncSelectionFromCanvas() =>
        _segmentListPanel.Show(_canvas.Document, _canvas.SelectedSegmentIndex);

    private void MoveSelectedSegment(int delta)
    {
        if (_segmentListPanel.SelectedIndex is not int index)
        {
            return;
        }

        _canvas.TryMoveSegment(index, delta);
    }

    private void SyncUiFromDocument()
    {
        _cvPanel?.ShowFrame(_canvas.Document.Frame);
        SyncGridUi();
        SyncWidthUi();
    }

    private void ApplyGridStep(double step)
    {
        if (step <= 0)
        {
            return;
        }

        IReadOnlyList<double> steps = _gridSteps.Count > 0 ? _gridSteps : _canvas.Document.Grid.Steps_mm;
        _canvas.SetGridStep(step, steps);
    }

    private void SyncGridUi()
    {
        TeachGridSettings grid = _canvas.Document.Grid;
        _gridSteps = GridSteps.NormalizeList(grid.Steps_mm, grid.Step_mm, out double selected);
        _updatingGrid = true;
        try
        {
            _toolPalette.BindGrid(grid.Enabled, _gridSteps, selected);
            _showGridMenuItem.Checked = grid.Enabled;
            RebuildGridSizeMenu(selected);
        }
        finally
        {
            _updatingGrid = false;
        }
    }

    private void RebuildGridSizeMenu(double selected)
    {
        var items = new List<ToolStripItem>();
        foreach (double step in _gridSteps)
        {
            double chosen = step;
            var item = new ToolStripMenuItem(GridSteps.Format(step))
            {
                Checked = GridSteps.NearlyEqual(step, selected)
            };
            item.Click += (_, _) =>
            {
                if (_updatingGrid)
                {
                    return;
                }

                ApplyGridStep(chosen);
                BeginInvoke(SyncGridUi);
            };
            items.Add(item);
        }

        items.Add(new ToolStripSeparator());
        var customize = new ToolStripMenuItem("Customize...");
        customize.Click += (_, _) => BeginInvoke(CustomizeGridSteps);
        items.Add(customize);

        ToolStripItem[] previous = _gridSizeMenuItem.DropDownItems.Cast<ToolStripItem>().ToArray();
        _gridSizeMenuItem.DropDownItems.Clear();
        _gridSizeMenuItem.DropDownItems.AddRange(items.ToArray());
        foreach (ToolStripItem item in previous)
        {
            item.Dispose();
        }
    }

    private void CustomizeGridSteps()
    {
        TeachGridSettings grid = _canvas.Document.Grid;
        using var dialog = new GridStepsDialog(grid.Steps_mm, grid.Step_mm);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _canvas.SetGridStep(dialog.SelectedStep, dialog.Steps);
        SyncGridUi();
    }

    private void ResetToolWidth()
    {
        _toolWidthMm = StrokeWidths.Floor(_canvas.Document.Defaults.MinWidth_mm);
        _canvas.SetToolWidth(_toolWidthMm);
    }

    private void SyncWidthUi()
    {
        ProcessDefaults defaults = _canvas.Document.Defaults;
        double minimum = StrokeWidths.Floor(defaults.MinWidth_mm);
        if (_toolWidthMm < minimum - 0.0005)
        {
            _toolWidthMm = minimum;
            _canvas.SetToolWidth(_toolWidthMm);
        }

        List<double> shown = StrokeWidths.DisplayList(defaults.Widths_mm, _toolWidthMm, minimum, out double selected);
        _toolWidthMm = selected;
        _canvas.SetToolWidth(selected);
        _updatingWidth = true;
        try
        {
            _toolPalette.BindWidths(shown, selected);
            RebuildWidthMenu(shown, selected);
        }
        finally
        {
            _updatingWidth = false;
        }
    }

    private void RebuildWidthMenu(IReadOnlyList<double> widths, double selected)
    {
        var items = new List<ToolStripItem>();
        foreach (double width in widths)
        {
            double chosen = width;
            var item = new ToolStripMenuItem(StrokeWidths.Format(width))
            {
                Checked = StrokeWidths.NearlyEqual(width, selected)
            };
            item.Click += (_, _) =>
            {
                if (_updatingWidth)
                {
                    return;
                }

                _toolWidthMm = chosen;
                _canvas.SetToolWidth(chosen);
                BeginInvoke(SyncWidthUi);
            };
            items.Add(item);
        }

        items.Add(new ToolStripSeparator());
        var customize = new ToolStripMenuItem("Customize...");
        customize.Click += (_, _) => BeginInvoke(CustomizeWidths);
        items.Add(customize);

        ToolStripItem[] previous = _widthMenuItem.DropDownItems.Cast<ToolStripItem>().ToArray();
        _widthMenuItem.DropDownItems.Clear();
        _widthMenuItem.DropDownItems.AddRange(items.ToArray());
        foreach (ToolStripItem item in previous)
        {
            item.Dispose();
        }
    }

    private void CustomizeWidths()
    {
        ProcessDefaults defaults = _canvas.Document.Defaults;
        using var dialog = new StrokeWidthsDialog(defaults.Widths_mm, defaults.MinWidth_mm);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _canvas.SetWidthSteps(dialog.Steps);
        SyncWidthUi();
    }

    private void OpenProjectSettings(object? sender, EventArgs e)
    {
        using var dialog = new ProjectSettingsDialog(
            _canvas.Document.Defaults,
            _canvas.Document.Workspace != WorkspaceKind.Teach);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _canvas.ApplyProcessSettings(dialog.Settings);
        _statusLabel.Text = "Project settings updated";
    }

    private void BeginFrameEdit()
    {
        if (_cvPanel == null)
        {
            return;
        }

        _cvPanel.BeginEdit();
        OnFrameDraftChanged();
        _statusLabel.Text = "Editing frame — Apply or Cancel";
    }

    private void ApplyFrameEdit()
    {
        if (_cvPanel is not { IsEditing: true })
        {
            return;
        }

        bool moveShapes = _cvPanel.MoveShapesWithFrame;
        _canvas.ApplyFrameChange(_cvPanel.OriginX, _cvPanel.OriginY, _cvPanel.AngleDeg, moveShapes);
        _canvas.Document.FrameIsDefined = true;
        _cvPanel.EndEdit();
        _canvas.ClearDraftFrame();
        UpdateUndoMenuState();
        RefreshTileList();
        _statusLabel.Text = moveShapes
            ? "Frame applied — shapes rebinding kept sensor px"
            : "Frame applied — local px unchanged";
    }

    private void CancelFrameEdit()
    {
        if (_cvPanel is not { IsEditing: true })
        {
            _canvas.ClearDraftFrame();
            return;
        }

        _cvPanel.EndEdit();
        _canvas.ClearDraftFrame();
        SyncUiFromDocument();
        _statusLabel.Text = "Frame edit cancelled";
    }

    private void OnFrameDraftChanged()
    {
        if (_cvPanel is not { IsEditing: true })
        {
            return;
        }

        _canvas.SetDraftFrame(_cvPanel.OriginX, _cvPanel.OriginY, _cvPanel.AngleDeg);
    }

    private void OnFrameOriginPicked(double sensorX, double sensorY)
    {
        if (_cvPanel == null)
        {
            return;
        }

        if (!_cvPanel.IsEditing)
        {
            _cvPanel.BeginEdit();
        }

        _cvPanel.SetOrigin(sensorX, sensorY);
        OnFrameDraftChanged();
        _statusLabel.Text = $"Frame origin draft: ({Math.Round(sensorX)}, {Math.Round(sensorY)}) px";
    }

    private void RefreshTileList() => _cvPanel?.ShowTiles(_canvas.Document, _calibration);

    private void RefreshTeachPointList() => _teachPanel?.Show(_canvas.Document);

    private void OnTilePoseChanged(object? sender, EventArgs e)
    {
        if (_cvPanel?.SelectedTileId is not string id)
        {
            return;
        }

        FovTile? tile = _canvas.Document.Tiles.FirstOrDefault(item => item.Id == id);
        if (tile == null)
        {
            return;
        }

        tile.RobotX_mm = _cvPanel.CaptureX;
        tile.RobotY_mm = _cvPanel.CaptureY;
        tile.RobotR_deg = _cvPanel.CaptureR;
        _canvas.RebuildMosaic(fitView: false);
        RefreshTileList();
        RefreshWaypointPreview();
    }

    private void OnResetCapturePose(object? sender, EventArgs e)
    {
        if (_calibration == null)
        {
            MessageBox.Show(this, "Load calibration JSON first.", "Capture pose", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        FovTile? tile = SelectedTile() ?? TileDocumentHelper.GetAnchor(_canvas.Document);
        if (tile == null)
        {
            return;
        }

        CapturePoseHelper.ApplyCalibrationReference(tile, _calibration);
        _canvas.RebuildMosaic(fitView: false);
        RefreshTileList();
        RefreshWaypointPreview();
        _statusLabel.Text = "Capture pose reset to calibration reference";
    }

    private void OnRemoveTile(object? sender, string tileId)
    {
        if (MessageBox.Show(this, "Remove selected tile?", "FOV Tiles", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        TileDocumentHelper.RemoveTile(_canvas.Document, tileId);
        _canvas.RebuildMosaic();
        RefreshTileList();
        SyncUiFromDocument();
        _statusLabel.Text = "FOV image removed";
    }

    private void OnFocusTile(object? sender, string tileId)
    {
        TileDocumentHelper.SetActive(_canvas.Document, tileId);
        _canvas.FocusTileView(tileId);
        RefreshTileList();
        _statusLabel.Text = "Focused tile view";
    }

    private FovTile? SelectedTile() =>
        _cvPanel?.SelectedTileId is string id
            ? _canvas.Document.Tiles.FirstOrDefault(tile => tile.Id == id)
            : null;

    private void TryLoadCalibrationFromDocument(PathDocument document)
    {
        string? calibrationFile = ResolveStoredFile(document.CalibrationPath);
        if (document.Workspace == WorkspaceKind.Teach ||
            string.IsNullOrWhiteSpace(document.CalibrationPath) ||
            calibrationFile == null)
        {
            SetCalibrationText("Calibration: not loaded");
            return;
        }

        try
        {
            ApplyLoadedCalibration(CalibrationLoader.Load(calibrationFile), document.CalibrationPath);
        }
        catch
        {
            _statusLabel.Text = "Path references calibration file that could not be loaded";
        }
    }

    private void ApplyLoadedCalibration(RobotCalibrationFile calibration, string path, bool recordUndo = false)
    {
        _calibration = calibration;
        _calibrationPath = path;
        _canvas.Document.CalibrationPath = path;
        SetCalibrationText(CalibrationLoader.FormatSummary(calibration, path));
        CalibrationLoader.ApplyCompactMarkerProcessDefaults(calibration, _canvas.Document.Defaults);

        FovTile? anchor = TileDocumentHelper.GetAnchor(_canvas.Document);
        if (anchor != null && CapturePoseHelper.IsUnsetCapturePose(anchor))
        {
            CapturePoseHelper.ApplyCalibrationReference(anchor, calibration);
        }

        _canvas.SetMosaicCalibration(_calibration);
        MigrateOpenPath(recordUndo);
        RefreshTileList();
        RefreshWaypointPreview();
    }

    private void SetCalibrationText(string text) => _cvPanel?.SetCalibrationText(text);

    private void ShowProjectPanel(WorkspaceKind kind)
    {
        _canvas.UseSpace(kind == WorkspaceKind.Teach
            ? new TeachCanvasSpace()
            : new CvCanvasSpace());
        SetFrameHostVisible(false);
        if (kind == WorkspaceKind.Cv)
        {
            DisposeProjectPanel(ref _teachPanel);
            _cvPanel ??= CreateCvPanel();
            _cvPanel.MountFrame(_frameHost);
            SetFrameHostVisible(true);
            _toolPalette.SetSnapEnabled(false);
            AttachProjectPage("FOV Tiles", _cvPanel);
            return;
        }

        if (kind == WorkspaceKind.Teach)
        {
            DisposeProjectPanel(ref _cvPanel);
            _teachPanel ??= CreateTeachPanel();
            _toolPalette.SetSnapEnabled(true);
            AttachProjectPage("Teach Points", _teachPanel);
            return;
        }

        DisposeProjectPanel(ref _cvPanel);
        DisposeProjectPanel(ref _teachPanel);
        AttachProjectPage(null, null);
    }

    private void SetFrameHostVisible(bool visible)
    {
        if (!visible)
        {
            _cvPanel?.RestoreFrame();
            _frameHost.Controls.Clear();
        }

        _frameHost.Visible = visible;
        _rightLayout.RowStyles[0] = visible
            ? new RowStyle(SizeType.AutoSize)
            : new RowStyle(SizeType.Absolute, 0F);
    }

    private void AttachProjectPage(string? title, Control? panel)
    {
        if (_projectPage != null
            && title != null
            && panel != null
            && _projectPage.Text == title
            && _projectPage.Controls.Count == 1
            && _projectPage.Controls[0] == panel)
        {
            return;
        }

        if (_projectPage != null)
        {
            _projectPage.Controls.Clear();
            _commonTabs.TabPages.Remove(_projectPage);
            _projectPage.Dispose();
            _projectPage = null;
        }

        if (title == null || panel == null)
        {
            return;
        }

        panel.Dock = DockStyle.Fill;
        _projectPage = new TabPage(title)
        {
            Padding = new Padding(4),
            UseVisualStyleBackColor = true
        };
        _projectPage.Controls.Add(panel);
        _commonTabs.TabPages.Add(_projectPage);
        _commonTabs.SelectedTab = _projectPage;
    }

    private CvProjectPanel CreateCvPanel()
    {
        var panel = new CvProjectPanel();
        panel.EditFrameRequested += (_, _) => BeginFrameEdit();
        panel.ApplyFrameRequested += (_, _) => ApplyFrameEdit();
        panel.CancelFrameRequested += (_, _) => CancelFrameEdit();
        panel.FrameDraftChanged += (_, _) => OnFrameDraftChanged();
        panel.InvertYChanged += (_, _) => _canvas.SetFrameInvertY(panel.InvertFrameY);
        panel.OpenImageRequested += (_, _) => OpenAnchorImage();
        panel.AddFovRequested += (_, _) => TryAddFovImage();
        panel.RemoveTileRequested += OnRemoveTile;
        panel.FocusTileRequested += OnFocusTile;
        panel.ResetCapturePoseRequested += OnResetCapturePose;
        panel.TilePoseChanged += OnTilePoseChanged;
        return panel;
    }

    private TeachProjectPanel CreateTeachPanel()
    {
        var panel = new TeachProjectPanel();
        panel.ImportRequested += (_, _) => ImportTeachPoints();
        panel.RemoveRequested += (_, _) =>
        {
            if (panel.SelectedPointId is string id)
            {
                _canvas.RemoveReferencePoint(id);
            }
        };
        panel.AlignRequested += (_, _) =>
        {
            if (panel.SelectedPointId is not string id)
            {
                return;
            }

            ReferencePoint? point = _canvas.Document.ReferencePoints.FirstOrDefault(item => item.Id == id);
            if (point != null)
            {
                _canvas.AlignGridTo(point.X_mm, point.Y_mm);
            }
        };
        return panel;
    }

    private static void DisposeProjectPanel<T>(ref T? panel) where T : Control
    {
        panel?.Dispose();
        panel = null;
    }

    private bool EnsureWorkspace(WorkspaceKind kind)
    {
        WorkspaceKind current = _session?.Kind ?? _canvas.Document.Workspace;
        if (current == WorkspaceKind.Unset || current == kind)
        {
            return true;
        }

        MessageBox.Show(
            this,
            WorkspaceGuard.RefusalMessage(current),
            "Project type",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        return false;
    }

    private void AttachSession(WorkspaceKind kind)
    {
        if (_session != null)
        {
            _session.Require(kind);
            return;
        }

        _session = ProjectSession.Start(_canvas.Document, kind);
        _session.FilePath = _draftFilePath;
        _session.Calibration = _draftCalibration;
        _session.CalibrationPath = _draftCalibrationPath;
        _draftFilePath = null;
        _draftCalibration = null;
        _draftCalibrationPath = null;
        _canvas.ClearHistory();
        ShowProjectPanel(kind);
        if (kind == WorkspaceKind.Cv)
        {
            SetCalibrationText(_calibration == null
                ? "Calibration: not loaded"
                : CalibrationLoader.FormatSummary(_calibration, _calibrationPath ?? string.Empty));
        }
    }

    private void ImportTeachPoints()
    {
        if (!EnsureWorkspace(WorkspaceKind.Teach))
        {
            return;
        }

        var existingNames = _canvas.Document.ReferencePoints.Select(static point => point.Name);
        using var dialog = new TeachPointImportDialog(existingNames);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Points.Count == 0)
        {
            return;
        }

        AttachSession(WorkspaceKind.Teach);
        _canvas.AddReferencePoints(dialog.Points);
    }

    private void OfferStartup()
    {
        if (IsDisposed)
        {
            return;
        }

        using var dialog = new StartupDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            if (!IsDisposed)
            {
                _suppressCloseConfirm = true;
                Close();
            }

            return;
        }

        ApplyProjectStart(new ProjectStart(dialog.Action, dialog.FilePath));
        ShowPluginNotes();
    }

    private void ShowPluginNotes()
    {
        if (_pluginsShown || _pluginNotes is not { Count: > 0 })
        {
            return;
        }

        _pluginsShown = true;
        MessageBox.Show(
            this,
            string.Join(Environment.NewLine, _pluginNotes),
            "Plugins",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private void NewProject(object? sender, EventArgs e)
    {
        if (!ConfirmReplaceProject())
        {
            return;
        }

        using var dialog = new StartupDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        ApplyProjectStart(new ProjectStart(dialog.Action, dialog.FilePath));
    }

    private void ApplyProjectStart(ProjectStart start)
    {
        if (start.Action == StartupAction.Open)
        {
            LoadPathFromFile(start.FilePath ?? throw new InvalidOperationException("Choose a project file."));
            return;
        }

        BeginEmptyProject(start.Action == StartupAction.NewTeach
            ? WorkspaceKind.Teach
            : WorkspaceKind.Cv);
    }

    private void BeginEmptyProject(WorkspaceKind kind)
    {
        CancelFrameEdit();
        _session = null;
        _draftCalibration = null;
        _draftCalibrationPath = null;
        _draftFilePath = null;
        _packageCacheDirectory = null;
        _canvas.ResetProject();
        AttachSession(kind);
        ResetToolWidth();
        SyncUiFromDocument();
        RefreshSegmentList();
        RefreshWaypointPreview();
        RefreshTileList();
        RefreshTeachPointList();
        _statusLabel.Text = kind == WorkspaceKind.Teach
            ? "Teach project. Import reference points to begin."
            : "CV project. Open an FOV image to begin.";
    }

    private string? ResolveStoredFile(string? storedPath) =>
        ProjectPackage.Resolve(_packageCacheDirectory, storedPath);

    private bool ConfirmReplaceProject()
    {
        PathDocument document = _canvas.Document;
        bool hasContent = document.Workspace != WorkspaceKind.Unset ||
                          document.Segments.Count > 0 ||
                          document.Tiles.Count > 0 ||
                          document.ReferencePoints.Count > 0;
        if (!hasContent)
        {
            return true;
        }

        return MessageBox.Show(
                this,
                "Start a new project? The current path stays on disk until you save this one.",
                "New project",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
