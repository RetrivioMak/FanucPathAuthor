using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public sealed class SimulationOverlay
{
    public double LocalX { get; init; }
    public double LocalY { get; init; }
    public bool GlueOn { get; init; }
    public IReadOnlyList<(double X, double Y, bool GlueOn)> Trail { get; init; } =
        Array.Empty<(double, double, bool)>();
}

public enum DrawTool
{
    Select,
    Move,
    SetFrameOrigin,
    Dot,
    Line,
    Arc,
    Rectangle,
    Circle
}

public sealed class PathCanvas : Control
{
    private Image? _image;
    private CanvasSpace _space = new CvCanvasSpace();
    private readonly List<MosaicTilePlacement> _mosaicTiles = new();
    private RectangleF _canvasBounds = RectangleF.Empty;
    private RobotCalibrationFile? _mosaicCalibration;
    private readonly List<(double X, double Y)> _pendingPoints = new();
    private readonly UndoRedoStack<PathDocument> _undoRedo = new(DocumentClone.Clone);
    private float _zoom = 1f;
    private double _toolWidthMm = ProcessDefaults.DefaultMinWidthMm;
    private double _toolZDeltaMm;
    private PointF _panOffset = PointF.Empty;
    private bool _isPanning;
    private Point _lastMouse;
    private (double X, double Y)? _cursorLocal;
    private (double SensorX, double SensorY)? _snapHover;
    private MarkerPoint? _hoverPoint;
    private int? _hoverShapeIndex;
    private MarkerPoint? _selectedPoint;
    private bool _suppressUndo;
    private readonly HScrollBar _horizontalScroll;
    private readonly VScrollBar _verticalScroll;
    private readonly Panel _scrollCorner;
    private readonly GridPainter _gridPainter = new();
    private readonly System.Windows.Forms.Timer _pointerTimer;
    private bool _adjustingScroll;
    private bool _trackingPointer;

    private readonly record struct MarkerPoint(
        double OriginX,
        double OriginY,
        double SensorX,
        double SensorY,
        int? DotSegmentIndex,
        string? ReferenceId,
        bool IsFrameOrigin = false);
    private bool _isDraggingVertex;
    private VertexPick? _activeVertexPick;

    public PathDocument Document { get; } = new();
    private DrawTool _activeTool = DrawTool.Select;

    public DrawTool ActiveTool
    {
        get => _activeTool;
        set
        {
            if (_activeTool == value)
            {
                return;
            }

            if (_isDraggingVertex)
            {
                CancelVertexDrag(undo: true);
            }

            _activeTool = value;
            ClearObjectHover();
            UpdatePointerTracking();
        }
    }
    public bool SnapToPointEnabled { get; set; }
    public int? SelectedSegmentIndex { get; private set; }
    public bool SimulationMode { get; set; }
    public SimulationOverlay? SimulationOverlay { get; set; }
    public bool CanUndo => _undoRedo.CanUndo;
    public bool CanRedo => _undoRedo.CanRedo;
    public bool HasPendingPoints => _pendingPoints.Count > 0;
    public Func<string?, string?>? ResolveStoredFile { get; set; }

    public void ClearSimulationOverlay()
    {
        SimulationOverlay = null;
        Invalidate();
    }

    public event EventHandler? DocumentChanged;
    public event EventHandler? SelectionChanged;
    public event EventHandler? UndoStateChanged;
    public event EventHandler<string>? StatusMessage;
    public event EventHandler<(double SensorX, double SensorY, double LocalX, double LocalY)>? CursorMoved;
    public event EventHandler<(double SensorX, double SensorY)>? FrameOriginPicked;

    /// <summary>Draft frame overlay while editing (optional).</summary>
    public FrameDefinition? DraftFrame { get; set; }

    public PathCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(40, 40, 40);
        TabStop = true;
        _horizontalScroll = new HScrollBar { TabStop = false };
        _verticalScroll = new VScrollBar { TabStop = false };
        _scrollCorner = new Panel { BackColor = SystemColors.Control, TabStop = false };
        _horizontalScroll.ValueChanged += (_, _) => OnScrollValueChanged();
        _verticalScroll.ValueChanged += (_, _) => OnScrollValueChanged();
        Controls.Add(_horizontalScroll);
        Controls.Add(_verticalScroll);
        Controls.Add(_scrollCorner);
        _pointerTimer = new System.Windows.Forms.Timer { Interval = 30 };
        _pointerTimer.Tick += (_, _) => TrackPointerUnderCursor();
    }

    public void LoadImage(string path)
    {
        _image?.Dispose();
        _image = ImageFileLoader.Open(path);
        Document.ImagePath = path;
        _canvasBounds = new RectangleF(0, 0, _image.Width, _image.Height);
        FitToView();
        Invalidate();
    }

    public void SetMosaicCalibration(RobotCalibrationFile? calibration)
    {
        _mosaicCalibration = calibration;
        RebuildMosaic();
    }

    public void RebuildMosaic(bool fitView = true)
    {
        ClearMosaicTiles();
        _image?.Dispose();
        _image = null;

        if (_space.TryMeasureBackground(Document, out RectangleF measured))
        {
            Document.ImagePath = null;
            _canvasBounds = measured;
            if (fitView)
            {
                FitToView();
            }

            Invalidate();
            return;
        }

        if (_mosaicCalibration?.PixelToRobot != null && Document.Tiles.Count > 0)
        {
            MosaicLayout? layout = MosaicLayoutService.TryBuild(Document, _mosaicCalibration, ResolveStoredFile);
            if (layout != null)
            {
                _mosaicTiles.AddRange(layout.Tiles);
                _canvasBounds = layout.Bounds;
                if (fitView)
                {
                    FitToView();
                }

                Invalidate();
                return;
            }
        }

        FovTile? activeTile = TileDocumentHelper.GetActive(Document);
        string? imagePath = ResolveFile(activeTile?.ImagePath);
        if (activeTile != null &&
            !string.IsNullOrWhiteSpace(imagePath) &&
            File.Exists(imagePath))
        {
            _image = ImageFileLoader.Open(imagePath);
            _canvasBounds = new RectangleF(0, 0, _image.Width, _image.Height);
            Document.ImagePath = activeTile.ImagePath;
            if (fitView)
            {
                FitToView();
            }
        }
        else
        {
            _canvasBounds = RectangleF.Empty;
            Document.ImagePath = activeTile?.ImagePath;
        }

        Invalidate();
    }

    private string? ResolveFile(string? storedPath) =>
        ResolveStoredFile != null ? ResolveStoredFile(storedPath) : storedPath;

    public void LoadActiveTileImage() => RebuildMosaic();

    public void FocusTileView(string tileId)
    {
        MosaicTilePlacement? placement = _mosaicTiles.FirstOrDefault(item => item.Tile.Id == tileId);
        if (placement == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        var tileBounds = new RectangleF(
            (float)placement.CanvasOriginX,
            (float)placement.CanvasOriginY,
            placement.Image.Width,
            placement.Image.Height);

        Size view = ViewSize;
        float scaleX = view.Width / tileBounds.Width;
        float scaleY = view.Height / tileBounds.Height;
        _zoom = Math.Min(scaleX, scaleY) * 0.9f;
        _panOffset = new PointF(
            (view.Width - tileBounds.Width * _zoom) / 2f - tileBounds.Left * _zoom,
            (view.Height - tileBounds.Height * _zoom) / 2f - tileBounds.Top * _zoom);
        SyncScrollBars();
        Invalidate();
    }

    public void UseSpace(CanvasSpace space)
    {
        ArgumentNullException.ThrowIfNull(space);
        bool changed = _space.GetType() != space.GetType();
        _space = space;
        if (changed)
        {
            RebuildMosaic();
        }
    }

    public string FormatCursor(double sensorX, double sensorY, double localX, double localY)
    {
        if (TryStoredScale(out FrameAxisScale scale))
        {
            (double mmX, double mmY) = FrameMillimeters.ToMillimeters(localX, localY, scale);
            return $"sensor ({sensorX:F0}, {sensorY:F0}) px  frame ({mmX:0.000}, {mmY:0.000}) mm";
        }

        return _space.FormatCursor(sensorX, sensorY, localX, localY);
    }

    private FrameAxisScale? ActiveMillimeters() =>
        TryStoredScale(out FrameAxisScale scale) ? scale : null;

    private bool TryStoredScale(out FrameAxisScale scale)
    {
        scale = default;
        if (!FrameMillimeters.StoresMillimeters(Document))
        {
            return false;
        }

        scale = FrameMillimeters.ResolveImageScale(
            Document.Frame,
            _mosaicCalibration,
            Document.Defaults.ReferenceMmPerPx);
        return scale.IsDefined;
    }

    private double ActiveMmPerPx()
    {
        if (Document.Workspace == WorkspaceKind.Teach)
        {
            return 1;
        }

        FrameAxisScale scale = FrameMillimeters.ResolveImageScale(
            Document.Frame,
            _mosaicCalibration,
            Document.Defaults.ReferenceMmPerPx);
        return FrameMillimeters.MeanMmPerPx(scale);
    }

    public void SetToolWidth(double widthMm)
    {
        if (widthMm <= 0 || Math.Abs(_toolWidthMm - widthMm) < 1e-9)
        {
            return;
        }

        _toolWidthMm = widthMm;
        if (_pendingPoints.Count > 0)
        {
            Invalidate();
        }
    }

    public void SetToolZDelta(double zDeltaMm)
    {
        if (!double.IsFinite(zDeltaMm) || Math.Abs(_toolZDeltaMm - zDeltaMm) < 1e-9)
        {
            return;
        }

        _toolZDeltaMm = zDeltaMm;
    }

    public void ApplyProcessSettings(ProcessDefaults incoming)
    {
        var copy = ProcessDefaultsCopy.Clone(incoming);
        StrokeWidths.EnsureStored(copy);
        if (StrokeWidths.SameSettings(Document.Defaults, copy))
        {
            return;
        }

        CommitUndoSnapshot();
        Document.Defaults = copy;
        RaiseDocumentChanged();
        Invalidate();
    }

    public void SetWidthSteps(IReadOnlyList<double> steps)
    {
        List<double> stored = StrokeWidths.StoredList(steps, Document.Defaults.MinWidth_mm);
        if (StrokeWidths.SameSettings(
                Document.Defaults,
                new ProcessDefaults
                {
                    Z_mm = Document.Defaults.Z_mm,
                    W_deg = Document.Defaults.W_deg,
                    P_deg = Document.Defaults.P_deg,
                    R_deg = Document.Defaults.R_deg,
                    GlueOn = Document.Defaults.GlueOn,
                    MinWidth_mm = Document.Defaults.MinWidth_mm,
                    ReferenceMmPerPx = Document.Defaults.ReferenceMmPerPx,
                    Widths_mm = stored,
                    ZDeltas_mm = new List<double>(Document.Defaults.ZDeltas_mm)
                }))
        {
            return;
        }

        CommitUndoSnapshot();
        Document.Defaults.Widths_mm = stored;
        RaiseDocumentChanged();
    }

    public void SetZDeltas(IReadOnlyList<double> offsets)
    {
        List<double> stored = ZOffsets.StoredList(offsets);
        if (StrokeWidths.SameSettings(
                Document.Defaults,
                new ProcessDefaults
                {
                    Z_mm = Document.Defaults.Z_mm,
                    W_deg = Document.Defaults.W_deg,
                    P_deg = Document.Defaults.P_deg,
                    R_deg = Document.Defaults.R_deg,
                    GlueOn = Document.Defaults.GlueOn,
                    MinWidth_mm = Document.Defaults.MinWidth_mm,
                    ReferenceMmPerPx = Document.Defaults.ReferenceMmPerPx,
                    Widths_mm = new List<double>(Document.Defaults.Widths_mm),
                    ZDeltas_mm = stored
                }))
        {
            return;
        }

        CommitUndoSnapshot();
        Document.Defaults.ZDeltas_mm = stored;
        RaiseDocumentChanged();
    }

    private (double X, double Y) StoredToPixels(double x, double y) =>
        TryStoredScale(out FrameAxisScale scale)
            ? FrameMillimeters.ToLocalPixels(x, y, scale)
            : (x, y);

    private bool CanStorePath()
    {
        if (!FrameMillimeters.StoresMillimeters(Document) || TryStoredScale(out _))
        {
            return true;
        }

        StatusMessage?.Invoke(this, "Load calibration before drawing. The CV path is stored in millimeters.");
        return false;
    }

    private (double X, double Y) StorePixels(double pixelX, double pixelY) =>
        TryStoredScale(out FrameAxisScale scale)
            ? FrameMillimeters.ToMillimeters(pixelX, pixelY, scale)
            : (pixelX, pixelY);

    private void RebindStoredPath(FrameDefinition oldFrame, FrameDefinition newFrame)
    {
        if (FrameMillimeters.StoresMillimeters(Document))
        {
            if (!FrameMillimeters.TryGetScale(oldFrame, _mosaicCalibration, out FrameAxisScale oldScale) ||
                !FrameMillimeters.TryGetScale(newFrame, _mosaicCalibration, out FrameAxisScale newScale))
            {
                return;
            }

            FrameTransformHelper.RebindSegmentsToFrame(Document.Segments, oldFrame, newFrame, oldScale, newScale);
            return;
        }

        FrameTransformHelper.RebindSegmentsToFrame(Document.Segments, oldFrame, newFrame);
    }

    private bool HasCanvas => _space.HasSurfaceWithoutImage || _mosaicTiles.Count > 0 || _image != null;

    private void ClearMosaicTiles()
    {
        foreach (MosaicTilePlacement placement in _mosaicTiles)
        {
            placement.Image.Dispose();
        }

        _mosaicTiles.Clear();
    }

    public void FitToView()
    {
        Size view = ViewSize;
        if (!HasCanvas || _canvasBounds.Width <= 0 || _canvasBounds.Height <= 0 ||
            view.Width <= 0 || view.Height <= 0)
        {
            return;
        }

        float scaleX = view.Width / _canvasBounds.Width;
        float scaleY = view.Height / _canvasBounds.Height;
        float fitted = Math.Min(scaleX, scaleY) * 0.95f;
        _zoom = _space.ClampFittedZoom ? ClampZoom(fitted) : fitted;
        double centerX = _canvasBounds.Left + _canvasBounds.Width / 2.0;
        double centerY = _canvasBounds.Top + _canvasBounds.Height / 2.0;
        _panOffset = PanForWorldAt(centerX, centerY, view.Width / 2f, view.Height / 2f);
        SyncScrollBars();
        Invalidate();
    }

    private float ClampZoom(float zoom) => Math.Clamp(zoom, _space.MinZoom, _space.MaxZoom);

    private PointF PanForWorldAt(double worldX, double worldY, float screenX, float screenY) =>
        _space.ScreenYIncreasesUp
            ? new PointF(screenX - (float)(worldX * _zoom), screenY + (float)(worldY * _zoom))
            : new PointF(screenX - (float)(worldX * _zoom), screenY - (float)(worldY * _zoom));

    public void SetFrameFromSensorPoint(double sensorX, double sensorY)
    {
        FrameOriginPicked?.Invoke(this, (sensorX, sensorY));
    }

    public void ApplyFrameChange(
        double originX,
        double originY,
        double angleDeg,
        bool moveShapesWithFrame)
    {
        if (!_space.AllowsFrameEdit)
        {
            StatusMessage?.Invoke(this, _space.FrameEditBlockedMessage);
            return;
        }

        if (Math.Abs(Document.Frame.OriginX_px - originX) < double.Epsilon &&
            Math.Abs(Document.Frame.OriginY_px - originY) < double.Epsilon &&
            Math.Abs(Document.Frame.AngleDeg - angleDeg) < double.Epsilon)
        {
            DraftFrame = null;
            Invalidate();
            return;
        }

        CommitUndoSnapshot();

        FrameDefinition oldFrame = Document.Frame.Clone();
        var newFrame = new FrameDefinition
        {
            OriginX_px = originX,
            OriginY_px = originY,
            AngleDeg = angleDeg,
            InvertY = Document.Frame.InvertY
        };

        if (moveShapesWithFrame)
        {
            RebindStoredPath(oldFrame, newFrame);
        }

        Document.Frame = newFrame;
        Document.FrameIsDefined = true;
        DraftFrame = null;
        RaiseDocumentChanged();
        Invalidate();
    }

    public void SetDraftFrame(double originX, double originY, double angleDeg)
    {
        DraftFrame = new FrameDefinition
        {
            OriginX_px = originX,
            OriginY_px = originY,
            AngleDeg = angleDeg,
            InvertY = Document.Frame.InvertY
        };
        Invalidate();
    }

    public void ClearDraftFrame()
    {
        DraftFrame = null;
        Invalidate();
    }

    public void SetFrameInvertY(bool invertY)
    {
        if (!_space.AllowsFrameEdit)
        {
            return;
        }

        if (Document.Frame.InvertY == invertY)
        {
            return;
        }

        CommitUndoSnapshot();
        FrameDefinition oldFrame = Document.Frame.Clone();
        FrameDefinition newFrame = oldFrame.Clone();
        newFrame.InvertY = invertY;
        RebindStoredPath(oldFrame, newFrame);
        for (int i = 0; i < _pendingPoints.Count; i++)
        {
            (double sensorX, double sensorY) = CoordinateTransform.LocalToSensor(
                _pendingPoints[i].X,
                _pendingPoints[i].Y,
                oldFrame);
            _pendingPoints[i] = CoordinateTransform.SensorToLocal(sensorX, sensorY, newFrame);
        }

        Document.Frame = newFrame;
        if (DraftFrame != null)
        {
            DraftFrame.InvertY = invertY;
        }

        RaiseDocumentChanged();
        StatusMessage?.Invoke(
            this,
            invertY
                ? "Frame Y inverted to Fanuc. Path stayed on the image. Set export rotation to None."
                : "Frame Y matches Open eVision. Export uses calibration axes.");
        Invalidate();
    }

    public void UpdateFrame(double originX, double originY, double angleDeg)
    {
        ApplyFrameChange(originX, originY, angleDeg, moveShapesWithFrame: false);
    }

    public void LoadDocument(PathDocument document)
    {
        _suppressUndo = true;
        try
        {
            Document.SchemaVersion = document.SchemaVersion;
            Document.Workspace = document.Workspace;
            Document.CalibrationPath = document.CalibrationPath;
            Document.ActiveTileId = document.ActiveTileId;
            Document.Frame = document.Frame;
            Document.FrameIsDefined = document.FrameIsDefined;
            Document.Defaults = document.Defaults;
            Document.Grid = document.Grid.Clone();
            Document.ReferencePoints.Clear();
            Document.ReferencePoints.AddRange(document.ReferencePoints.Select(static point => point.Clone()));
            Document.Segments.Clear();
            Document.Segments.AddRange(document.Segments.Select(DocumentClone.CloneSegment));
            Document.Tiles.Clear();
            Document.Tiles.AddRange(document.Tiles.Select(DocumentClone.CloneTile));
            Document.ImagePath = document.ImagePath;
            Document.Exporters.Extra = ExporterSettings.CloneExtra(document.Exporters.Extra);

            TileDocumentHelper.EnsureLegacyMigrated(Document);
            RebuildMosaic();

            SelectedSegmentIndex = null;
            _selectedPoint = null;
            _hoverPoint = null;
            _hoverShapeIndex = null;
            _pendingPoints.Clear();
            _cursorLocal = null;
            _isDraggingVertex = false;
            _activeVertexPick = null;
            Capture = false;
            _undoRedo.Clear();
        }
        finally
        {
            _suppressUndo = false;
        }

        RaiseDocumentChanged();
        RaiseSelectionChanged();
        RaiseUndoStateChanged();
        Invalidate();
    }

    public PathDocument CreateDocumentSnapshot()
    {
        TileDocumentHelper.SyncImagePath(Document);

        return new PathDocument
        {
            SchemaVersion = Document.SchemaVersion,
            Workspace = Document.Workspace,
            ImagePath = Document.ImagePath,
            CalibrationPath = Document.CalibrationPath,
            ActiveTileId = Document.ActiveTileId,
            Frame = Document.Frame.Clone(),
            FrameIsDefined = Document.FrameIsDefined,
            Defaults = ProcessDefaultsCopy.Clone(Document.Defaults),
            Exporters = Document.Exporters.Clone(),
            Grid = Document.Grid.Clone(),
            ReferencePoints = Document.ReferencePoints.Select(static point => point.Clone()).ToList(),
            Tiles = Document.Tiles.Select(DocumentClone.CloneTile).ToList(),
            Segments = Document.Segments
                .Select(static s => new PathSegment
                {
                    Kind = s.Kind,
                    GlueOn = s.GlueOn,
                    Width_mm = s.Width_mm,
                    ZDelta_mm = s.ZDelta_mm,
                    X1 = s.X1,
                    Y1 = s.Y1,
                    X2 = s.X2,
                    Y2 = s.Y2,
                    X3 = s.X3,
                    Y3 = s.Y3
                })
                .ToList()
        };
    }

    public void DeleteSelectedSegment()
    {
        if (SelectedSegmentIndex is not int index ||
            index < 0 ||
            index >= Document.Segments.Count)
        {
            return;
        }

        CommitUndoSnapshot();
        Document.Segments.RemoveAt(index);
        SelectedSegmentIndex = null;
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        Invalidate();
    }

    public bool TryMoveSegment(int index, int delta)
    {
        if (!PathSequenceService.CanMoveSegment(Document.Segments.Count, index, delta))
        {
            return false;
        }

        CommitUndoSnapshot();
        PathSequenceService.MoveSegment(Document.Segments, index, delta);
        SelectedSegmentIndex = index + delta;
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        Invalidate();
        return true;
    }

    public bool TrySetSegmentProperty(int index, SegmentPropertyField field, double number, bool flag)
    {
        if (index < 0 || index >= Document.Segments.Count)
        {
            return false;
        }

        PathSegment segment = Document.Segments[index];
        if (!SegmentProperties.WouldChange(segment, field, number, flag))
        {
            return false;
        }

        CommitUndoSnapshot();
        SegmentProperties.Apply(segment, field, number, flag);
        if (field == SegmentPropertyField.GlueOn && segment.Kind is SegmentKind.Line or SegmentKind.Arc)
        {
            if (!segment.GlueOn)
            {
                segment.Width_mm = 0;
            }
            else if (segment.Width_mm <= 0)
            {
                segment.Width_mm = StrokeWidths.Floor(Document.Defaults.MinWidth_mm);
            }
        }

        SyncSelectedMarker();
        RaiseDocumentChanged();
        Invalidate();
        return true;
    }

    public void AppendSegments(IReadOnlyList<PathSegment> segments)
    {
        if (segments.Count == 0)
        {
            return;
        }

        CommitUndoSnapshot();
        Document.Segments.AddRange(segments.Select(DocumentClone.CloneSegment));
        SelectCreatedSegment();
        _pendingPoints.Clear();
        _cursorLocal = null;
        ApplyMeasuredBounds();
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        Invalidate();
    }

    public void ClearSegments()
    {
        if (Document.Segments.Count == 0)
        {
            return;
        }

        CommitUndoSnapshot();
        Document.Segments.Clear();
        SelectedSegmentIndex = null;
        _pendingPoints.Clear();
        _cursorLocal = null;
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        Invalidate();
    }

    public void CancelPending()
    {
        if (_pendingPoints.Count == 0)
        {
            return;
        }

        _pendingPoints.Clear();
        _cursorLocal = null;
        StatusMessage?.Invoke(this, "Cancelled in-progress shape");
        Invalidate();
    }

    public void Undo()
    {
        PathDocument? previous = _undoRedo.Undo(CreateDocumentSnapshot());
        if (previous == null)
        {
            return;
        }

        RestoreSnapshot(previous);
        StatusMessage?.Invoke(this, "Undo");
    }

    public void Redo()
    {
        PathDocument? next = _undoRedo.Redo(CreateDocumentSnapshot());
        if (next == null)
        {
            return;
        }

        RestoreSnapshot(next);
        StatusMessage?.Invoke(this, "Redo");
    }

    public bool TryHandleKey(Keys key)
    {
        if (key == Keys.Escape)
        {
            if (_isDraggingVertex)
            {
                CancelVertexDrag(undo: true);
                return true;
            }

            CancelPending();
            return true;
        }

        return false;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (!HasCanvas || _pendingPoints.Count > 0)
        {
            base.OnMouseWheel(e);
            return;
        }

        float oldZoom = _zoom;
        float factor = e.Delta > 0 ? 1.1f : 0.9f;
        float newZoom = ClampZoom(_zoom * factor);
        if (Math.Abs(oldZoom - newZoom) < float.Epsilon)
        {
            base.OnMouseWheel(e);
            return;
        }

        PointF world = ScreenToImage(e.Location);
        _zoom = newZoom;
        _panOffset = PanForWorldAt(world.X, world.Y, e.X, e.Y);
        SyncScrollBars();
        Invalidate();

        base.OnMouseWheel(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        Focus();

        if (e.Button == MouseButtons.Right && _pendingPoints.Count > 0)
        {
            CancelPending();
            return;
        }

        if (e.Button == MouseButtons.Middle ||
            (e.Button == MouseButtons.Left && ModifierKeys == Keys.Control))
        {
            BeginPan(e.Location);
            return;
        }

        if (e.Button == MouseButtons.Right &&
            ActiveTool == DrawTool.Select &&
            HasCanvas &&
            TryShowSelectionMenu(e.Location))
        {
            return;
        }

        if (e.Button == MouseButtons.Right &&
            _space.TryHandleRightClick(this, Document, e.Location, ImageToScreen, AlignGridTo))
        {
            return;
        }

        if (e.Button == MouseButtons.Right && HasCanvas && !IsOverSelectable(e.Location))
        {
            BeginPan(e.Location);
            return;
        }

        if (e.Button != MouseButtons.Left || !HasCanvas)
        {
            base.OnMouseDown(e);
            return;
        }

        if (SimulationMode && ActiveTool != DrawTool.Select)
        {
            return;
        }

        PointF imagePoint = ScreenToImage(e.Location);
        double sensorX = imagePoint.X;
        double sensorY = imagePoint.Y;

        switch (ActiveTool)
        {
            case DrawTool.SetFrameOrigin:
                if (!_space.AllowsFrameEdit)
                {
                    StatusMessage?.Invoke(this, _space.FrameEditBlockedMessage);
                    break;
                }

                SetFrameFromSensorPoint(sensorX, sensorY);
                break;

            case DrawTool.Select:
                SelectObjectAt(sensorX, sensorY);
                break;

            case DrawTool.Move:
                BeginVertexDrag(sensorX, sensorY);
                break;

            case DrawTool.Dot:
            {
                (double drawX, double drawY, bool snapped) = ResolveDrawClick(sensorX, sensorY);
                if ((snapped || _space.ConfirmEveryDot) && !ConfirmDuplicateDot(drawX, drawY))
                {
                    break;
                }

                AddDot(drawX, drawY);
                break;
            }

            case DrawTool.Line:
            {
                (double drawX, double drawY, _) = ResolveDrawClick(sensorX, sensorY);
                HandlePolylineClick((drawX, drawY), requiredPoints: 2, SegmentKind.Line);
                break;
            }

            case DrawTool.Arc:
            {
                (double drawX, double drawY, _) = ResolveDrawClick(sensorX, sensorY);
                HandlePolylineClick((drawX, drawY), requiredPoints: 3, SegmentKind.Arc);
                break;
            }

            case DrawTool.Rectangle:
            {
                (double drawX, double drawY, _) = ResolveDrawClick(sensorX, sensorY);
                HandleRectangleClick((drawX, drawY));
                break;
            }

            case DrawTool.Circle:
            {
                (double drawX, double drawY, _) = ResolveDrawClick(sensorX, sensorY);
                HandleCircleClick((drawX, drawY));
                break;
            }
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_isPanning)
        {
            _panOffset.X += e.X - _lastMouse.X;
            _panOffset.Y += e.Y - _lastMouse.Y;
            _lastMouse = e.Location;
            SyncScrollBars();
            Invalidate();
        }
        else if (_isDraggingVertex && _activeVertexPick != null && HasCanvas && !SimulationMode)
        {
            PointF imagePoint = ScreenToImage(e.Location);
            double sensorX = imagePoint.X;
            double sensorY = imagePoint.Y;
            if (_space.HandlesPointer)
            {
                CanvasSpace.PointerHit hit = _space.AdjustPointer(
                    Document,
                    sensorX,
                    sensorY,
                    GetSnapExcludePoints(_activeVertexPick),
                    ImageToScreen,
                    SnapToPointEnabled);
                sensorX = hit.X;
                sensorY = hit.Y;
                if (SnapToPointEnabled)
                {
                    _snapHover = (sensorX, sensorY);
                }
                else
                {
                    ClearSnapHover();
                }
            }
            else if (SnapToPointEnabled)
            {
                (double resolvedX, double resolvedY, bool snappedToPoint) = ResolveSnapPosition(
                    imagePoint.X,
                    imagePoint.Y,
                    GetSnapExcludePoints(_activeVertexPick));
                sensorX = resolvedX;
                sensorY = resolvedY;
                _snapHover = snappedToPoint || Document.Grid.Step_mm > 0 ? (sensorX, sensorY) : null;
            }
            else
            {
                ClearSnapHover();
            }

            (double localX, double localY) = CoordinateTransform.SensorToLocal(
                sensorX,
                sensorY,
                Document.Frame);
            PointEditHelper.ApplyDrag(Document, _activeVertexPick, localX, localY, ActiveMillimeters());
            Invalidate();
        }
        else
        {
            TrackPointer(e.Location);
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        TrackPointerUnderCursor();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        // A status-bar update during tracking can relayout the form and raise a
        // leave even though the cursor is still over the canvas. Clearing the
        // snap marker then makes the first project look like snap is dead.
        if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
        {
            base.OnMouseLeave(e);
            return;
        }

        if (!_isDraggingVertex)
        {
            ClearSnapHover();
            ClearObjectHover();
            if (ActiveTool == DrawTool.Move || IsDrawTool(ActiveTool) || _pendingPoints.Count > 0)
            {
                Invalidate();
            }
        }

        base.OnMouseLeave(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (_isPanning)
        {
            _isPanning = false;
            Capture = false;
            Cursor = Cursors.Default;
        }
        else if (_isDraggingVertex && e.Button == MouseButtons.Left)
        {
            EndVertexDrag(e.Location);
        }

        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        if (_mosaicTiles.Count > 0)
        {
            foreach (MosaicTilePlacement placement in _mosaicTiles)
            {
                var dest = new RectangleF(
                    (float)(placement.CanvasOriginX * _zoom + _panOffset.X),
                    (float)(placement.CanvasOriginY * _zoom + _panOffset.Y),
                    placement.Image.Width * _zoom,
                    placement.Image.Height * _zoom);
                e.Graphics.DrawImage(placement.Image, dest);

                using var borderPen = new Pen(Color.FromArgb(100, 180, 180, 180), 1f);
                e.Graphics.DrawRectangle(borderPen, dest.X, dest.Y, dest.Width, dest.Height);

                if (placement.Tile.IsAnchor)
                {
                    using var labelBrush = new SolidBrush(Color.FromArgb(180, 120, 220, 120));
                    e.Graphics.DrawString("Anchor", Font, labelBrush, dest.X + 4, dest.Y + 4);
                }
            }
        }
        else if (_image != null)
        {
            var dest = new RectangleF(
                _panOffset.X,
                _panOffset.Y,
                _image.Width * _zoom,
                _image.Height * _zoom);
            e.Graphics.DrawImage(_image, dest);
        }

        _space.DrawBackground(e.Graphics, Document, _zoom, ClientSize, ImageToScreen, ScreenToImage);
        DrawProjectGrid(e.Graphics);
        DrawFrame(e.Graphics, Document.Frame, Color.Lime, _space.FrameLabel);
        _space.DrawForeground(e.Graphics, Document, Font, ImageToScreen);
        if (DraftFrame != null &&
            (Math.Abs(DraftFrame.OriginX_px - Document.Frame.OriginX_px) > double.Epsilon ||
             Math.Abs(DraftFrame.OriginY_px - Document.Frame.OriginY_px) > double.Epsilon ||
             Math.Abs(DraftFrame.AngleDeg - Document.Frame.AngleDeg) > double.Epsilon))
        {
            DrawFrame(e.Graphics, DraftFrame, Color.Orange, "Draft");
        }
        DrawSegments(e.Graphics);
        DrawSelectMarkers(e.Graphics);
        // DrawSequenceGaps(e.Graphics);
        DrawSequenceLabels(e.Graphics);
        DrawSimulationOverlay(e.Graphics);
        DrawPending(e.Graphics);
        DrawRubberBand(e.Graphics);
        DrawSnapHover(e.Graphics);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pointerTimer.Stop();
            _pointerTimer.Dispose();
            ClearMosaicTiles();
            _gridPainter.Dispose();
            _image?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void UpdatePointerTracking()
    {
        if (!IsHandleCreated)
        {
            return;
        }

        bool track = HasCanvas &&
            !SimulationMode &&
            (IsDrawTool(ActiveTool) || ActiveTool == DrawTool.Move || _pendingPoints.Count > 0);
        if (_pointerTimer.Enabled == track)
        {
            if (track)
            {
                TrackPointerUnderCursor();
            }

            return;
        }

        _pointerTimer.Enabled = track;
        if (track)
        {
            TrackPointerUnderCursor();
        }
    }

    private void TrackPointerUnderCursor()
    {
        if (!IsHandleCreated || !Visible || _isPanning || _isDraggingVertex)
        {
            return;
        }

        Point client = PointToClient(Cursor.Position);
        if (!ClientRectangle.Contains(client))
        {
            return;
        }

        TrackPointer(client);
    }

    private void TrackPointer(Point location)
    {
        if (_trackingPointer || _isPanning || _isDraggingVertex || !HasCanvas || SimulationMode)
        {
            return;
        }

        _trackingPointer = true;
        try
        {
            PointF imagePoint = ScreenToImage(location);
            double sensorX = imagePoint.X;
            double sensorY = imagePoint.Y;

            (double SensorX, double SensorY)? previousHover = _snapHover;
            if (ActiveTool == DrawTool.Move && !_isDraggingVertex)
            {
                UpdateMoveHover(sensorX, sensorY);
            }
            else if (ActiveTool == DrawTool.Select)
            {
                UpdateObjectHover(sensorX, sensorY);
            }
            else if (_space.HandlesPointer && IsDrawTool(ActiveTool))
            {
                CanvasSpace.PointerHit hit = _space.AdjustPointer(
                    Document,
                    sensorX,
                    sensorY,
                    null,
                    ImageToScreen,
                    SnapToPointEnabled);
                sensorX = hit.X;
                sensorY = hit.Y;
                _snapHover = SnapToPointEnabled ? (hit.X, hit.Y) : null;
            }
            else if (SnapToPointEnabled && IsDrawTool(ActiveTool))
            {
                (double resolvedX, double resolvedY, bool snappedToPoint) = ResolveDrawClick(sensorX, sensorY);
                sensorX = resolvedX;
                sensorY = resolvedY;
                _snapHover = snappedToPoint || Document.Grid.Step_mm > 0 ? (resolvedX, resolvedY) : null;
            }
            else if (ActiveTool != DrawTool.Move)
            {
                _snapHover = null;
            }

            (double localX, double localY) = CoordinateTransform.SensorToLocal(
                sensorX,
                sensorY,
                Document.Frame);
            bool cursorChanged = _cursorLocal is not (double oldX, double oldY) ||
                Math.Abs(oldX - localX) > 1e-6 ||
                Math.Abs(oldY - localY) > 1e-6;
            bool hoverChanged = !SamePoint(previousHover, _snapHover);
            if (!cursorChanged && !hoverChanged)
            {
                return;
            }

            _cursorLocal = (localX, localY);
            CursorMoved?.Invoke(this, (sensorX, sensorY, localX, localY));

            if (hoverChanged ||
                (cursorChanged &&
                 (_pendingPoints.Count > 0 ||
                  ActiveTool == DrawTool.Move ||
                  (_space.FollowDrawPointer && IsDrawTool(ActiveTool)) ||
                  (SnapToPointEnabled && IsDrawTool(ActiveTool)))))
            {
                Invalidate();
            }
        }
        finally
        {
            _trackingPointer = false;
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UpdatePointerTracking();
    }

    private void HandlePolylineClick((double SensorX, double SensorY) sensorPoint, int requiredPoints, SegmentKind kind)
    {
        if (!CanStorePath())
        {
            return;
        }

        (double localX, double localY) = CoordinateTransform.SensorToLocal(
            sensorPoint.SensorX,
            sensorPoint.SensorY,
            Document.Frame);

        _pendingPoints.Add((localX, localY));
        StatusMessage?.Invoke(this, $"Point {_pendingPoints.Count}/{requiredPoints} captured");

        if (_pendingPoints.Count < requiredPoints)
        {
            Invalidate();
            return;
        }

        CommitUndoSnapshot();

        var segment = new PathSegment
        {
            Kind = kind,
            GlueOn = Document.Defaults.GlueOn,
            Width_mm = kind == SegmentKind.Dot || !Document.Defaults.GlueOn ? 0 : _toolWidthMm,
            ZDelta_mm = _toolZDeltaMm
        };
        (segment.X1, segment.Y1) = StorePixels(_pendingPoints[0].X, _pendingPoints[0].Y);

        if (requiredPoints >= 2)
        {
            (segment.X2, segment.Y2) = StorePixels(_pendingPoints[1].X, _pendingPoints[1].Y);
        }

        if (requiredPoints >= 3)
        {
            (segment.X3, segment.Y3) = StorePixels(_pendingPoints[2].X, _pendingPoints[2].Y);
        }

        Document.Segments.Add(segment);
        _pendingPoints.Clear();
        _cursorLocal = null;
        SelectCreatedSegment();
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        Invalidate();
    }

    private void HandleRectangleClick((double SensorX, double SensorY) sensorPoint)
    {
        if (!CanStorePath())
        {
            return;
        }

        (double localX, double localY) = CoordinateTransform.SensorToLocal(
            sensorPoint.SensorX,
            sensorPoint.SensorY,
            Document.Frame);

        _pendingPoints.Add((localX, localY));
        StatusMessage?.Invoke(this, $"Point {_pendingPoints.Count}/2 captured");

        if (_pendingPoints.Count < 2)
        {
            Invalidate();
            return;
        }

        CommitUndoSnapshot();

        IReadOnlyList<PathSegment> lines = ShapeExpander.RectangleToLines(
            _pendingPoints[0].X,
            _pendingPoints[0].Y,
            _pendingPoints[1].X,
            _pendingPoints[1].Y,
            Document.Defaults.GlueOn);

        foreach (PathSegment line in lines)
        {
            FrameMillimeters.ConvertPoints(line, StorePixels);
            line.Width_mm = Document.Defaults.GlueOn ? _toolWidthMm : 0;
            line.ZDelta_mm = _toolZDeltaMm;
        }

        Document.Segments.AddRange(lines);
        _pendingPoints.Clear();
        _cursorLocal = null;
        SelectCreatedSegment();
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        StatusMessage?.Invoke(this, "Rectangle added (4 lines)");
        Invalidate();
    }

    private void HandleCircleClick((double SensorX, double SensorY) sensorPoint)
    {
        if (!CanStorePath())
        {
            return;
        }

        (double localX, double localY) = CoordinateTransform.SensorToLocal(
            sensorPoint.SensorX,
            sensorPoint.SensorY,
            Document.Frame);

        _pendingPoints.Add((localX, localY));
        StatusMessage?.Invoke(this, $"Point {_pendingPoints.Count}/2 captured");

        if (_pendingPoints.Count < 2)
        {
            Invalidate();
            return;
        }

        IReadOnlyList<PathSegment> arcs = ShapeExpander.CircleToArcs(
            _pendingPoints[0].X,
            _pendingPoints[0].Y,
            _pendingPoints[1].X,
            _pendingPoints[1].Y,
            Document.Defaults.GlueOn);

        if (arcs.Count == 0)
        {
            _pendingPoints.Clear();
            StatusMessage?.Invoke(this, "Circle radius is too small");
            Invalidate();
            return;
        }

        CommitUndoSnapshot();

        foreach (PathSegment arc in arcs)
        {
            FrameMillimeters.ConvertPoints(arc, StorePixels);
            arc.Width_mm = Document.Defaults.GlueOn ? _toolWidthMm : 0;
            arc.ZDelta_mm = _toolZDeltaMm;
        }

        Document.Segments.AddRange(arcs);
        _pendingPoints.Clear();
        _cursorLocal = null;
        SelectCreatedSegment();
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        StatusMessage?.Invoke(this, "Circle added (2 arcs)");
        Invalidate();
    }

    private void AddDot(double sensorX, double sensorY)
    {
        if (!CanStorePath())
        {
            return;
        }

        (double localX, double localY) = CoordinateTransform.SensorToLocal(
            sensorX,
            sensorY,
            Document.Frame);

        CommitUndoSnapshot();

        (double storedX, double storedY) = StorePixels(localX, localY);
        Document.Segments.Add(new PathSegment
        {
            Kind = SegmentKind.Dot,
            GlueOn = Document.Defaults.GlueOn,
            ZDelta_mm = _toolZDeltaMm,
            X1 = storedX,
            Y1 = storedY
        });

        SelectCreatedSegment();
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        Invalidate();
    }

    private void SelectCreatedSegment()
    {
        SelectedSegmentIndex = Document.Segments.Count - 1;
        SyncSelectedMarker();
    }

    public void SelectSegment(int? index)
    {
        if (index is int i && (i < 0 || i >= Document.Segments.Count))
        {
            index = null;
        }

        SelectedSegmentIndex = index;
        SyncSelectedMarker();
        RaiseSelectionChanged();
        Invalidate();
    }

    private void SyncSelectedMarker()
    {
        if (SelectedSegmentIndex is not int index ||
            index < 0 ||
            index >= Document.Segments.Count ||
            Document.Segments[index].Kind != SegmentKind.Dot)
        {
            _selectedPoint = null;
            return;
        }

        PathSegment segment = Document.Segments[index];
        _selectedPoint = MarkerFromStored(segment.X1, segment.Y1, index, null);
    }

    private static bool IsDrawTool(DrawTool tool) =>
        tool is DrawTool.Dot or DrawTool.Line or DrawTool.Arc or DrawTool.Rectangle or DrawTool.Circle;

    private void BeginVertexDrag(double sensorX, double sensorY)
    {
        if (!PointEditHelper.TryPickVertex(
                Document,
                sensorX,
                sensorY,
                ImageToScreen,
                SnapPointHelper.DefaultPickRadiusScreenPx,
                out VertexPick? pick,
                ActiveMillimeters()) ||
            pick == null)
        {
            StatusMessage?.Invoke(this, "No point near cursor");
            return;
        }

        CommitUndoSnapshot();
        _activeVertexPick = pick;
        _isDraggingVertex = true;
        Capture = true;
        SelectedSegmentIndex = pick.Bindings.Count > 0 ? pick.Bindings[0].SegmentIndex : pick.CircleArcIndexA;
        RaiseSelectionChanged();
        ClearSnapHover();
        Invalidate();
    }

    private void EndVertexDrag(Point screenLocation)
    {
        if (!_isDraggingVertex || _activeVertexPick == null)
        {
            return;
        }

        if (_space.HandlesPointer && HasCanvas)
        {
            PointF imagePoint = ScreenToImage(screenLocation);
            CanvasSpace.PointerHit hit = _space.AdjustPointer(
                Document,
                imagePoint.X,
                imagePoint.Y,
                GetSnapExcludePoints(_activeVertexPick),
                ImageToScreen,
                SnapToPointEnabled);
            (double teachLocalX, double teachLocalY) = CoordinateTransform.SensorToLocal(
                hit.X,
                hit.Y,
                Document.Frame);
            PointEditHelper.ApplyDrag(Document, _activeVertexPick, teachLocalX, teachLocalY, ActiveMillimeters());
        }
        else if (SnapToPointEnabled && HasCanvas)
        {
            PointF imagePoint = ScreenToImage(screenLocation);
            (double sensorX, double sensorY, bool snappedToPoint) = ResolveSnapPosition(
                imagePoint.X,
                imagePoint.Y,
                GetSnapExcludePoints(_activeVertexPick));
            if (snappedToPoint || Document.Grid.Step_mm > 0)
            {
                (double localX, double localY) = CoordinateTransform.SensorToLocal(
                    sensorX,
                    sensorY,
                    Document.Frame);
                PointEditHelper.ApplyDrag(Document, _activeVertexPick, localX, localY, ActiveMillimeters());
            }
        }

        _isDraggingVertex = false;
        _activeVertexPick = null;
        Capture = false;
        ClearSnapHover();
        RaiseDocumentChanged();
        StatusMessage?.Invoke(this, "Point moved");
        Invalidate();
    }

    private void CancelVertexDrag(bool undo)
    {
        if (!_isDraggingVertex)
        {
            return;
        }

        _isDraggingVertex = false;
        _activeVertexPick = null;
        Capture = false;

        if (undo)
        {
            Undo();
        }

        StatusMessage?.Invoke(this, "Move cancelled");
        Invalidate();
    }

    private void UpdateMoveHover(double sensorX, double sensorY)
    {
        IReadOnlyList<(double SensorX, double SensorY)> points = SnapPointHelper.CollectSensorPoints(Document, millimeters: ActiveMillimeters());
        if (SnapPointHelper.TryFindNearestScreen(
                points,
                sensorX,
                sensorY,
                ImageToScreen,
                SnapPointHelper.DefaultPickRadiusScreenPx,
                out double snapX,
                out double snapY))
        {
            _snapHover = (snapX, snapY);
            return;
        }

        ClearSnapHover();
    }

    private (double SensorX, double SensorY, bool Snapped) ResolveSnapPosition(
        double sensorX,
        double sensorY,
        IEnumerable<(double LocalX, double LocalY)>? excludeLocal = null)
    {
        if (!SnapToPointEnabled)
        {
            return (sensorX, sensorY, false);
        }

        IReadOnlyList<(double SensorX, double SensorY)> points = excludeLocal == null
            ? SnapPointHelper.CollectSensorPoints(Document, millimeters: ActiveMillimeters())
            : SnapPointHelper.CollectSensorPointsExcluding(Document, excludeLocal, millimeters: ActiveMillimeters());

        if (SnapPointHelper.TryFindNearestScreen(
                points,
                sensorX,
                sensorY,
                ImageToScreen,
                SnapPointHelper.DefaultPickRadiusScreenPx,
                out double snapX,
                out double snapY))
        {
            return (snapX, snapY, true);
        }

        if (TrySnapToGrid(sensorX, sensorY, out double gridX, out double gridY))
        {
            return (gridX, gridY, false);
        }

        return (sensorX, sensorY, false);
    }

    private bool TrySnapToGrid(double sensorX, double sensorY, out double x, out double y)
    {
        x = sensorX;
        y = sensorY;
        if (Document.Grid.Step_mm <= 0)
        {
            return false;
        }

        (double gridX, double gridY) = SensorToGrid(sensorX, sensorY);
        (double snappedX, double snappedY) = TeachGridSnap.Snap(gridX, gridY, Document.Grid);
        (x, y) = GridToSensor(snappedX, snappedY);
        return true;
    }

    private IEnumerable<(double LocalX, double LocalY)> GetSnapExcludePoints(VertexPick pick)
    {
        if (pick.Mode is VertexDragMode.CircleCenter or VertexDragMode.CircleRadius)
        {
            foreach (int arcIndex in new[] { pick.CircleArcIndexA, pick.CircleArcIndexB })
            {
                if (arcIndex < 0 || arcIndex >= Document.Segments.Count)
                {
                    continue;
                }

                foreach ((double localX, double localY) in GeometryHelper.ExpandSegment(Document.Segments[arcIndex]))
                {
                    yield return (localX, localY);
                }
            }

            yield break;
        }

        foreach ((int segmentIndex, int pointSlot) in pick.Bindings)
        {
            if (segmentIndex < 0 || segmentIndex >= Document.Segments.Count)
            {
                continue;
            }

            PathSegment segment = Document.Segments[segmentIndex];
            yield return pointSlot switch
            {
                0 => (segment.X1, segment.Y1),
                1 => (segment.X2, segment.Y2),
                2 => (segment.X3, segment.Y3),
                _ => (segment.X1, segment.Y1)
            };
        }
    }

    private (double SensorX, double SensorY, bool Snapped) ResolveDrawClick(double sensorX, double sensorY)
    {
        if (_space.HandlesPointer)
        {
            CanvasSpace.PointerHit hit = _space.AdjustPointer(
                Document,
                sensorX,
                sensorY,
                null,
                ImageToScreen,
                SnapToPointEnabled);
            return (hit.X, hit.Y, hit.SnappedToPoint);
        }

        if (!SnapToPointEnabled || !IsDrawTool(ActiveTool))
        {
            return (sensorX, sensorY, false);
        }

        (double snapX, double snapY, bool snappedToPoint) = ResolveSnapPosition(sensorX, sensorY);
        if (snappedToPoint || Document.Grid.Step_mm > 0)
        {
            return (snapX, snapY, snappedToPoint);
        }

        return (Math.Round(sensorX), Math.Round(sensorY), false);
    }

    private void ClearSnapHover()
    {
        if (!_snapHover.HasValue)
        {
            return;
        }

        _snapHover = null;
    }

    private bool ConfirmDuplicateDot(double sensorX, double sensorY)
    {
        double tolerance = _space.PointTolerance;
        IReadOnlyList<(double SensorX, double SensorY)> points = SnapPointHelper.CollectSensorPoints(Document, tolerance, ActiveMillimeters());
        if (!SnapPointHelper.ContainsSensorPoint(points, sensorX, sensorY, tolerance))
        {
            return true;
        }

        DialogResult result = MessageBox.Show(
            FindForm(),
            "A point already exists at the same coordinates.\n\nCreate another one anyway?",
            "Snap to point",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        return result == DialogResult.Yes;
    }

    private void DrawSnapHover(Graphics g)
    {
        if (_snapHover is not (double sensorX, double sensorY))
        {
            return;
        }

        if (_isDraggingVertex && !SnapToPointEnabled)
        {
            return;
        }
        else if (ActiveTool == DrawTool.Move)
        {
            // Always highlight pick targets in Move mode.
        }
        else if (_space.FollowDrawPointer && IsDrawTool(ActiveTool))
        {
            // Teach drawing follows the grid and reference points.
        }
        else if (!(SnapToPointEnabled && IsDrawTool(ActiveTool)))
        {
            return;
        }

        DrawDashedMarker(g, sensorX, sensorY);
    }

    private void DrawDashedMarker(Graphics g, double sensorX, double sensorY)
    {
        PointF screen = ImageToScreen(sensorX, sensorY);
        const float radius = 12f;
        var bounds = new RectangleF(screen.X - radius, screen.Y - radius, radius * 2, radius * 2);
        using var pen = CreateDashedPen(Color.FromArgb(220, 120, 0), 2f);
        DrawStroke(g, pen, drawPen => g.DrawEllipse(drawPen, bounds));
    }

    private void DrawSelectMarkers(Graphics g)
    {
        if (ActiveTool != DrawTool.Select)
        {
            return;
        }

        if (_selectedPoint is MarkerPoint selected)
        {
            DrawDashedMarker(g, selected.SensorX, selected.SensorY);
        }

        if (_hoverPoint is MarkerPoint hover &&
            (_selectedPoint is not MarkerPoint selectedPoint || !SameMarker(hover, selectedPoint)))
        {
            DrawDashedMarker(g, hover.SensorX, hover.SensorY);
        }
    }

    private void SelectObjectAt(double sensorX, double sensorY)
    {
        if (TryPickPoint(sensorX, sensorY, out MarkerPoint point))
        {
            ApplyPointSelection(point);
            return;
        }

        if (TryPickShape(sensorX, sensorY, out int shapeIndex))
        {
            ApplyShapeSelection(shapeIndex);
            return;
        }

        ClearObjectSelection();
    }

    private void ApplyPointSelection(MarkerPoint point)
    {
        _selectedPoint = point;
        SelectedSegmentIndex = point.DotSegmentIndex;
        RaiseSelectionChanged();
        Invalidate();
    }

    private void ApplyShapeSelection(int segmentIndex)
    {
        _selectedPoint = null;
        SelectedSegmentIndex = segmentIndex;
        RaiseSelectionChanged();
        Invalidate();
    }

    private void ClearObjectSelection()
    {
        _selectedPoint = null;
        SelectedSegmentIndex = null;
        RaiseSelectionChanged();
        Invalidate();
    }

    private void UpdateObjectHover(double sensorX, double sensorY)
    {
        MarkerPoint? point = TryPickPoint(sensorX, sensorY, out MarkerPoint hit) ? hit : null;
        int? shape = point == null && TryPickShape(sensorX, sensorY, out int shapeIndex) ? shapeIndex : null;
        bool changed = !SameMarker(_hoverPoint, point) || _hoverShapeIndex != shape;
        _hoverPoint = point;
        _hoverShapeIndex = shape;
        Cursor = point != null || shape != null ? Cursors.Hand : Cursors.Default;
        if (changed)
        {
            Invalidate();
        }
    }

    private void ClearObjectHover()
    {
        bool hadHover = _hoverPoint != null || _hoverShapeIndex != null;
        _hoverPoint = null;
        _hoverShapeIndex = null;
        if (Cursor == Cursors.Hand)
        {
            Cursor = Cursors.Default;
        }

        if (hadHover)
        {
            Invalidate();
        }
    }

    private bool TryShowSelectionMenu(Point screen)
    {
        PointF image = ScreenToImage(screen);
        if (TryPickPoint(image.X, image.Y, out MarkerPoint point))
        {
            ApplyPointSelection(point);
            ShowObjectMenu(screen, point.DotSegmentIndex, _space.HasGrid, point.OriginX, point.OriginY);
            return true;
        }

        if (TryPickShape(image.X, image.Y, out int shapeIndex))
        {
            ApplyShapeSelection(shapeIndex);
            ShowObjectMenu(screen, shapeIndex, includeGridOrigin: false, 0, 0);
            return true;
        }

        return false;
    }

    private void ShowObjectMenu(
        Point screen,
        int? segmentIndex,
        bool includeGridOrigin,
        double originX,
        double originY)
    {
        var menu = new ContextMenuStrip();
        if (includeGridOrigin)
        {
            menu.Items.Add("Set as grid origin", null, (_, _) => AlignGridTo(originX, originY));
        }

        if (segmentIndex is int index &&
            index >= 0 &&
            index < Document.Segments.Count &&
            Document.Segments[index].Kind is SegmentKind.Dot or SegmentKind.Line or SegmentKind.Arc)
        {
            menu.Items.Add("Delete", null, (_, _) =>
            {
                SelectedSegmentIndex = index;
                DeleteSelectedSegment();
            });
        }

        if (menu.Items.Count == 0)
        {
            menu.Dispose();
            return;
        }

        menu.Closed += (_, _) =>
        {
            if (!IsHandleCreated || IsDisposed)
            {
                return;
            }

            BeginInvoke(() =>
            {
                if (!menu.IsDisposed)
                {
                    menu.Dispose();
                }
            });
        };
        menu.Show(this, screen);
    }

    private bool TryPickPoint(double sensorX, double sensorY, out MarkerPoint point)
    {
        PointF click = ImageToScreen(sensorX, sensorY);
        double best = SnapPointHelper.DefaultPickRadiusScreenPx;
        point = default;
        bool found = false;

        for (int i = Document.Segments.Count - 1; i >= 0; i--)
        {
            PathSegment segment = Document.Segments[i];
            foreach ((double x, double y) in GeometryHelper.ExpandSegment(segment))
            {
                MarkerPoint candidate = MarkerFromStored(x, y, i, null);
                double distance = ScreenDistance(click, ImageToScreen(candidate.SensorX, candidate.SensorY));
                if (distance <= SnapPointHelper.DefaultPickRadiusScreenPx && (!found || distance < best))
                {
                    best = distance;
                    point = candidate;
                    found = true;
                }
            }
        }

        if (_space.HasReferencePoints)
        {
            foreach (ReferencePoint reference in Document.ReferencePoints)
            {
                double distance = ScreenDistance(click, ImageToScreen(reference.X_mm, reference.Y_mm));
                if (distance <= SnapPointHelper.DefaultPickRadiusScreenPx && (!found || distance < best))
                {
                    best = distance;
                    point = new MarkerPoint(reference.X_mm, reference.Y_mm, reference.X_mm, reference.Y_mm, null, reference.Id);
                    found = true;
                }
            }
        }

        var frameOrigin = CoordinateTransform.LocalToSensor(0, 0, Document.Frame);
        double frameDistance = ScreenDistance(click, ImageToScreen(frameOrigin.X, frameOrigin.Y));
        if (frameDistance <= SnapPointHelper.DefaultPickRadiusScreenPx && (!found || frameDistance < best))
        {
            point = new MarkerPoint(0, 0, frameOrigin.X, frameOrigin.Y, null, null, true);
            found = true;
        }

        return found;
    }

    private bool TryPickShape(double sensorX, double sensorY, out int segmentIndex)
    {
        const float pickRadiusPx = 12f;
        PointF click = ImageToScreen(sensorX, sensorY);
        segmentIndex = -1;
        double best = double.MaxValue;
        for (int i = Document.Segments.Count - 1; i >= 0; i--)
        {
            PathSegment segment = Document.Segments[i];
            if (segment.Kind == SegmentKind.Dot)
            {
                continue;
            }

            double distance = SegmentScreenDistance(segment, click);
            float stroke = StrokeWidths.ScreenPixels(
                StrokeWidths.Effective(segment, Document.Defaults.MinWidth_mm),
                ActiveMmPerPx(),
                _zoom);
            float pick = Math.Max(pickRadiusPx, stroke / 2f);
            if (distance <= pick && distance < best)
            {
                best = distance;
                segmentIndex = i;
            }
        }

        return segmentIndex >= 0;
    }

    private int? SelectedShapeIndex()
    {
        if (SelectedSegmentIndex is int index &&
            index >= 0 &&
            index < Document.Segments.Count &&
            Document.Segments[index].Kind != SegmentKind.Dot)
        {
            return index;
        }

        return null;
    }

    private bool IsHighlightedShape(int index, int? primary)
    {
        if (primary is not int shape)
        {
            return false;
        }

        if (index == shape)
        {
            return true;
        }

        return PointEditHelper.TryGetCircleForArc(
                Document.Segments,
                shape,
                out int arcA,
                out int arcB,
                out _,
                out _,
                out _) &&
            (index == arcA || index == arcB);
    }

    private MarkerPoint MarkerFromStored(double x, double y, int? dotSegmentIndex, string? referenceId)
    {
        (double pixelX, double pixelY) = StoredToPixels(x, y);
        var sensor = CoordinateTransform.LocalToSensor(pixelX, pixelY, Document.Frame);
        return new MarkerPoint(x, y, sensor.X, sensor.Y, dotSegmentIndex, referenceId);
    }

    private static bool SameMarker(MarkerPoint? left, MarkerPoint? right)
    {
        if (left is not MarkerPoint a || right is not MarkerPoint b)
        {
            return left is null && right is null;
        }

        return a.IsFrameOrigin == b.IsFrameOrigin &&
               a.ReferenceId == b.ReferenceId &&
               a.DotSegmentIndex == b.DotSegmentIndex &&
               Math.Abs(a.OriginX - b.OriginX) < 1e-6 &&
               Math.Abs(a.OriginY - b.OriginY) < 1e-6;
    }

    private double SegmentScreenDistance(PathSegment segment, PointF click)
    {
        switch (segment.Kind)
        {
            case SegmentKind.Dot:
                return ScreenDistance(click, StoredToScreen(segment.X1, segment.Y1));

            case SegmentKind.Line:
                return DistanceToSegment(
                    click,
                    StoredToScreen(segment.X1, segment.Y1),
                    StoredToScreen(segment.X2, segment.Y2));

            case SegmentKind.Arc:
                IReadOnlyList<(double X, double Y)> samples = PathSimulationService.SampleArc(segment);
                if (samples.Count == 0)
                {
                    return double.MaxValue;
                }

                PointF previous = StoredToScreen(samples[0].X, samples[0].Y);
                if (samples.Count == 1)
                {
                    return ScreenDistance(click, previous);
                }

                double best = double.MaxValue;
                for (int i = 1; i < samples.Count; i++)
                {
                    PointF next = StoredToScreen(samples[i].X, samples[i].Y);
                    best = Math.Min(best, DistanceToSegment(click, previous, next));
                    previous = next;
                }

                return best;

            default:
                return double.MaxValue;
        }
    }

    private PointF StoredToScreen(double x, double y)
    {
        (double pixelX, double pixelY) = StoredToPixels(x, y);
        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(pixelX, pixelY, Document.Frame);
        return ImageToScreen(sensorX, sensorY);
    }

    private static double ScreenDistance(PointF a, PointF b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static double DistanceToSegment(PointF point, PointF start, PointF end)
    {
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;
        double lengthSquared = dx * dx + dy * dy;
        if (lengthSquared < 1e-6)
        {
            return ScreenDistance(point, start);
        }

        double t = Math.Clamp(((point.X - start.X) * dx + (point.Y - start.Y) * dy) / lengthSquared, 0, 1);
        var projection = new PointF((float)(start.X + t * dx), (float)(start.Y + t * dy));
        return ScreenDistance(point, projection);
    }

    private void DrawFrame(Graphics g, FrameDefinition frame, Color color, string label)
    {
        double axis = _space.FrameAxisLength(_zoom);
        var (originX, originY) = CoordinateTransform.LocalToSensor(0, 0, frame);
        PointF origin = ImageToScreen(originX, originY);
        var (xSensorX, xSensorY) = CoordinateTransform.LocalToSensor(axis, 0, frame);
        var (ySensorX, ySensorY) = CoordinateTransform.LocalToSensor(0, axis, frame);
        PointF xEnd = ImageToScreen(xSensorX, xSensorY);
        PointF yEnd = ImageToScreen(ySensorX, ySensorY);

        using var pen = new Pen(color, 2);
        using var brush = new SolidBrush(color);
        g.DrawLine(pen, origin, xEnd);
        g.DrawLine(pen, origin, yEnd);
        g.FillEllipse(brush, origin.X - 4, origin.Y - 4, 8, 8);
        g.DrawString(label, Font, brush, origin.X + 6, origin.Y + 6);
        g.DrawString("X", Font, brush, xEnd.X + 4, xEnd.Y - 8);
        g.DrawString("Y", Font, brush, yEnd.X + 4, yEnd.Y - 8);
    }

    /*
    private void DrawSequenceGaps(Graphics g)
    {
        IReadOnlyList<SequenceGap> gaps = PathSequenceService.AnalyzeGaps(Document.Segments);
        if (gaps.Count == 0)
        {
            return;
        }

        using var pen = new Pen(Color.OrangeRed, 2)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
        };
        using var brush = new SolidBrush(Color.OrangeRed);

        foreach (SequenceGap gap in gaps)
        {
            if (!gap.HasGap(PathSequenceService.DefaultGapThresholdPx))
            {
                continue;
            }

            DrawLine(g, pen, gap.FromX_px, gap.FromY_px, gap.ToX_px, gap.ToY_px);
            DrawPoint(g, brush, gap.FromX_px, gap.FromY_px, 5);
            DrawPoint(g, brush, gap.ToX_px, gap.ToY_px, 5);
        }
    }
    */

    private void DrawSequenceLabels(Graphics g)
    {
        IReadOnlyList<PathSegment> segments = Document.Segments;
        if (segments.Count == 0)
        {
            return;
        }

        (double startX, double startY) = StoredToPixels(PathSequenceService.GetStartPoint(segments[0]).X, PathSequenceService.GetStartPoint(segments[0]).Y);
        DrawSequenceLabel(g, startX, startY, "1s", Color.LimeGreen);

        int lastNumber = segments.Count;
        (double endStoredX, double endStoredY) = PathSequenceService.GetEndPoint(segments[lastNumber - 1]);
        (double endX, double endY) = StoredToPixels(endStoredX, endStoredY);
        DrawSequenceLabel(g, endX, endY, $"{lastNumber}e", Color.DeepSkyBlue);

        double gapThreshold = FrameMillimeters.StoresMillimeters(Document)
            ? PathSequenceService.TeachGapThresholdMm
            : PathSequenceService.DefaultGapThresholdPx;
        IReadOnlyList<SequenceGap> gaps = PathSequenceService.AnalyzeGaps(segments);
        foreach (SequenceGap gap in gaps)
        {
            if (!gap.HasGap(gapThreshold))
            {
                continue;
            }

            int fromNumber = gap.FromSegmentIndex + 1;
            int toNumber = gap.ToSegmentIndex + 1;
            (double fromX, double fromY) = StoredToPixels(gap.FromX_px, gap.FromY_px);
            (double toX, double toY) = StoredToPixels(gap.ToX_px, gap.ToY_px);
            DrawSequenceLabel(g, fromX, fromY, $"{fromNumber}e", Color.OrangeRed);
            DrawSequenceLabel(g, toX, toY, $"{toNumber}s", Color.OrangeRed);
        }
    }

    private void DrawSequenceLabel(Graphics g, double localX, double localY, string label, Color color)
    {
        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(localX, localY, Document.Frame);
        PointF screen = ImageToScreen(sensorX, sensorY);
        using var font = new Font(Font.FontFamily, 8.5f, FontStyle.Bold);
        float x = screen.X + 6;
        float y = screen.Y - 16;
        SizeF size = g.MeasureString(label, font);
        var bounds = new RectangleF(x - 3, y - 2, size.Width + 6, size.Height + 4);
        using (var shadow = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
        {
            g.FillRectangle(shadow, bounds);
        }

        using var textBrush = new SolidBrush(color);
        g.DrawString(label, font, textBrush, x, y);
    }

    private void DrawSimulationOverlay(Graphics g)
    {
        if (SimulationOverlay == null)
        {
            return;
        }

        IReadOnlyList<(double X, double Y, bool GlueOn)> trail = SimulationOverlay.Trail;
        if (trail.Count > 1)
        {
            using var dispensePen = new Pen(Color.FromArgb(180, 30, 144, 255), 3);
            using var travelPen = new Pen(Color.FromArgb(180, 255, 140, 0), 3)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
            };

            for (int i = 1; i < trail.Count; i++)
            {
                (double x1, double y1, bool glue1) = trail[i - 1];
                (_, _, bool glue2) = trail[i];
                Pen pen = glue1 && glue2 ? dispensePen : travelPen;
                DrawLine(g, pen, x1, y1, trail[i].X, trail[i].Y);
            }
        }

        Color headColor = SimulationOverlay.GlueOn ? Color.DeepSkyBlue : Color.Orange;
        using (var halo = new SolidBrush(Color.FromArgb(120, Color.White)))
        {
            DrawPoint(g, halo, SimulationOverlay.LocalX, SimulationOverlay.LocalY, 11);
        }

        using var headBrush = new SolidBrush(headColor);
        DrawPoint(g, headBrush, SimulationOverlay.LocalX, SimulationOverlay.LocalY, 8);

        string glueLabel = SimulationOverlay.GlueOn ? "ON" : "OFF";
        DrawSequenceLabel(g, SimulationOverlay.LocalX, SimulationOverlay.LocalY, glueLabel, headColor);
    }

    private void DrawSegments(Graphics g)
    {
        for (int i = 0; i < Document.Segments.Count; i++)
        {
            PathSegment segment = Document.Segments[i];
            bool travel = !segment.GlueOn;
            bool selected = segment.Kind != SegmentKind.Dot && IsHighlightedShape(i, SelectedShapeIndex());
            bool hover = segment.Kind != SegmentKind.Dot && !selected && IsHighlightedShape(i, _hoverShapeIndex);
            Color color = segment.GlueOn ? Color.DeepSkyBlue : TravelColor;
            if (!travel && selected)
            {
                color = Color.Yellow;
            }
            else if (!travel && hover)
            {
                color = Color.FromArgb(120, 255, 160);
            }

            Color pointColor = travel
                ? selected ? Color.Yellow : hover ? Color.FromArgb(170, 255, 190) : TravelColor
                : color;
            using var brush = new SolidBrush(pointColor);

            switch (segment.Kind)
            {
                case SegmentKind.Dot:
                    (double dotX, double dotY) = StoredToPixels(segment.X1, segment.Y1);
                    if (travel)
                    {
                        using Pen dotPen = CreateTravelPen();
                        DrawOpenPoint(g, dotPen, dotX, dotY, 6);
                    }
                    else
                    {
                        DrawPoint(g, brush, dotX, dotY, 6);
                    }

                    DrawZOffset(g, dotX, dotY, segment.ZDelta_mm);
                    break;

                case SegmentKind.Line:
                    (double lineX1, double lineY1) = StoredToPixels(segment.X1, segment.Y1);
                    (double lineX2, double lineY2) = StoredToPixels(segment.X2, segment.Y2);
                    using (Pen pen = travel ? CreateTravelPen() : CreateStrokePen(color, segment.Width_mm))
                    {
                        DrawLine(g, pen, lineX1, lineY1, lineX2, lineY2);
                    }

                    DrawPoint(g, brush, lineX1, lineY1, 4);
                    DrawPoint(g, brush, lineX2, lineY2, 4);
                    DrawZOffset(g, (lineX1 + lineX2) / 2, (lineY1 + lineY2) / 2, segment.ZDelta_mm);
                    break;

                case SegmentKind.Arc:
                    (double arcX1, double arcY1) = StoredToPixels(segment.X1, segment.Y1);
                    (double arcX2, double arcY2) = StoredToPixels(segment.X2, segment.Y2);
                    (double arcX3, double arcY3) = StoredToPixels(segment.X3, segment.Y3);
                    using (Pen pen = travel ? CreateTravelPen() : CreateStrokePen(color, segment.Width_mm))
                    {
                        DrawArc(g, pen, arcX1, arcY1, arcX2, arcY2, arcX3, arcY3);
                    }

                    DrawPoint(g, brush, arcX1, arcY1, 4);
                    DrawPoint(g, brush, arcX2, arcY2, 4);
                    DrawPoint(g, brush, arcX3, arcY3, 4);
                    DrawZOffset(g, arcX2, arcY2, segment.ZDelta_mm);
                    break;
            }
        }
    }

    private const int ZMarkRadius = 5;

    private static readonly Color TravelColor = Color.FromArgb(0, 170, 60);

    private static readonly Color ZMarkColor = Color.MediumPurple;

    private Pen CreateTravelPen()
    {
        float px = StrokeWidths.ScreenPixels(
            StrokeWidths.Floor(Document.Defaults.MinWidth_mm),
            ActiveMmPerPx(),
            _zoom);
        return CreateDashedPen(TravelColor, px);
    }

    private void DrawOpenPoint(Graphics g, Pen pen, double localX, double localY, int radius)
    {
        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(localX, localY, Document.Frame);
        PointF screen = ImageToScreen(sensorX, sensorY);
        var bounds = new RectangleF(screen.X - radius, screen.Y - radius, radius * 2, radius * 2);
        DrawStroke(g, pen, drawPen => g.DrawEllipse(drawPen, bounds));
    }

    private void DrawZOffset(Graphics g, double localX, double localY, double deltaMm)
    {
        if (ZOffsets.NearlyEqual(deltaMm, 0))
        {
            return;
        }

        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(localX, localY, Document.Frame);
        PointF screen = ImageToScreen(sensorX, sensorY);
        PointF[] diamond =
        [
            new(screen.X, screen.Y - ZMarkRadius),
            new(screen.X + ZMarkRadius, screen.Y),
            new(screen.X, screen.Y + ZMarkRadius),
            new(screen.X - ZMarkRadius, screen.Y)
        ];
        using (var pen = new Pen(ZMarkColor, 1.5f))
        using (var brush = new SolidBrush(Color.FromArgb(210, ZMarkColor)))
        {
            g.FillPolygon(brush, diamond);
            g.DrawPolygon(pen, diamond);
        }

        string label = ZOffsets.Format(deltaMm);
        using var font = new Font(Font.FontFamily, 8.5f, FontStyle.Bold);
        float textX = screen.X + ZMarkRadius + 3;
        float textY = screen.Y - 16;
        SizeF size = g.MeasureString(label, font);
        var bounds = new RectangleF(textX - 3, textY - 2, size.Width + 6, size.Height + 4);
        using (var shadow = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
        {
            g.FillRectangle(shadow, bounds);
        }

        using var textBrush = new SolidBrush(ZMarkColor);
        g.DrawString(label, font, textBrush, textX, textY);
    }

    private void DrawPending(Graphics g)
    {
        if (_pendingPoints.Count == 0)
        {
            return;
        }

        using var brush = new SolidBrush(Color.Magenta);
        using var pen = new Pen(Color.Magenta, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
        foreach ((double localX, double localY) in _pendingPoints)
        {
            DrawPoint(g, brush, localX, localY, 5);
        }
    }

    private void DrawRubberBand(Graphics g)
    {
        if (_pendingPoints.Count == 0 || _cursorLocal is not (double cx, double cy))
        {
            return;
        }

        using Pen pen = Document.Defaults.GlueOn
            ? CreateDashedPen(Color.Magenta, StrokeWidths.ScreenPixels(_toolWidthMm, ActiveMmPerPx(), _zoom))
            : CreateTravelPen();

        switch (ActiveTool)
        {
            case DrawTool.Line when _pendingPoints.Count == 1:
                DrawLine(g, pen, _pendingPoints[0].X, _pendingPoints[0].Y, cx, cy);
                break;

            case DrawTool.Arc when _pendingPoints.Count == 1:
                DrawLine(g, pen, _pendingPoints[0].X, _pendingPoints[0].Y, cx, cy);
                break;

            case DrawTool.Arc when _pendingPoints.Count == 2:
            {
                var preview = new PathSegment
                {
                    X1 = _pendingPoints[0].X,
                    Y1 = _pendingPoints[0].Y,
                    X2 = _pendingPoints[1].X,
                    Y2 = _pendingPoints[1].Y,
                    X3 = cx,
                    Y3 = cy
                };
                DrawArc(g, pen, preview);
                break;
            }

            case DrawTool.Rectangle when _pendingPoints.Count == 1:
            {
                foreach (PathSegment line in ShapeExpander.RectangleToLines(
                             _pendingPoints[0].X,
                             _pendingPoints[0].Y,
                             cx,
                             cy,
                             glueOn: true))
                {
                    DrawLine(g, pen, line.X1, line.Y1, line.X2, line.Y2);
                }

                break;
            }

            case DrawTool.Circle when _pendingPoints.Count == 1:
            {
                double centerX = _pendingPoints[0].X;
                double centerY = _pendingPoints[0].Y;
                foreach (PathSegment arc in ShapeExpander.CircleToArcs(
                             centerX,
                             centerY,
                             cx,
                             cy,
                             glueOn: true))
                {
                    DrawArc(g, pen, arc);
                }

                DrawLine(g, pen, centerX, centerY, cx, cy);
                break;
            }
        }
    }

    private void CommitUndoSnapshot()
    {
        if (_suppressUndo)
        {
            return;
        }

        _undoRedo.PushSnapshot(CreateDocumentSnapshot());
        RaiseUndoStateChanged();
    }

    private void RestoreSnapshot(PathDocument snapshot)
    {
        _suppressUndo = true;
        try
        {
            WorkspaceKind lockedKind = Document.Workspace;
            Document.SchemaVersion = snapshot.SchemaVersion;
            Document.Workspace = lockedKind == WorkspaceKind.Unset ? snapshot.Workspace : lockedKind;
            Document.ImagePath = snapshot.ImagePath;
            Document.CalibrationPath = snapshot.CalibrationPath;
            Document.ActiveTileId = snapshot.ActiveTileId;
            Document.Frame = snapshot.Frame.Clone();
            Document.FrameIsDefined = snapshot.FrameIsDefined;
            Document.Defaults = ProcessDefaultsCopy.Clone(snapshot.Defaults);
            Document.Exporters = snapshot.Exporters.Clone();
            Document.Grid = snapshot.Grid.Clone();
            Document.ReferencePoints.Clear();
            Document.ReferencePoints.AddRange(snapshot.ReferencePoints.Select(static point => point.Clone()));
            Document.Tiles.Clear();
            Document.Tiles.AddRange(snapshot.Tiles.Select(DocumentClone.CloneTile));
            Document.Segments.Clear();
            Document.Segments.AddRange(snapshot.Segments.Select(DocumentClone.CloneSegment));
            TileDocumentHelper.SyncImagePath(Document);
            RebuildMosaic();
            SelectedSegmentIndex = null;
            _pendingPoints.Clear();
            _cursorLocal = null;
            _isDraggingVertex = false;
            _activeVertexPick = null;
            Capture = false;
        }
        finally
        {
            _suppressUndo = false;
        }

        RaiseDocumentChanged();
        RaiseSelectionChanged();
        RaiseUndoStateChanged();
        Invalidate();
    }

    private Pen CreateStrokePen(Color color, double widthMm)
    {
        double width = widthMm > 0 ? widthMm : Document.Defaults.MinWidth_mm;
        float px = StrokeWidths.ScreenPixels(width, ActiveMmPerPx(), _zoom);
        return new Pen(color, px)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round
        };
    }

    private static Pen CreateDashedPen(Color color, float widthPx) =>
        new(color, Math.Max(1f, widthPx))
        {
            // Round caps on a dashed pen wider than one pixel make GDI+ throw,
            // or draw nothing, at the zoom a new project fits to. The canvas
            // then keeps the grid and the finished solid line, but never the
            // snap circle or the preview.
            StartCap = System.Drawing.Drawing2D.LineCap.Flat,
            EndCap = System.Drawing.Drawing2D.LineCap.Flat,
            DashCap = System.Drawing.Drawing2D.DashCap.Flat,
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
        };

    private void DrawLine(Graphics g, Pen pen, double lx1, double ly1, double lx2, double ly2)
    {
        var (sx1, sy1) = CoordinateTransform.LocalToSensor(lx1, ly1, Document.Frame);
        var (sx2, sy2) = CoordinateTransform.LocalToSensor(lx2, ly2, Document.Frame);
        PointF start = ImageToScreen(sx1, sy1);
        PointF end = ImageToScreen(sx2, sy2);
        if (pen.DashStyle != System.Drawing.Drawing2D.DashStyle.Solid &&
            Math.Abs(start.X - end.X) < 0.75f &&
            Math.Abs(start.Y - end.Y) < 0.75f)
        {
            return;
        }

        DrawStroke(g, pen, drawPen => g.DrawLine(drawPen, start, end));
    }

    private static void DrawStroke(Graphics g, Pen pen, Action<Pen> draw)
    {
        System.Drawing.Drawing2D.SmoothingMode saved = g.SmoothingMode;
        if (pen.DashStyle != System.Drawing.Drawing2D.DashStyle.Solid)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        }

        try
        {
            try
            {
                draw(pen);
            }
            catch (OutOfMemoryException) when (IsWideDash(pen))
            {
                DrawThin(g, pen, draw);
            }
            catch (System.Runtime.InteropServices.ExternalException) when (IsWideDash(pen))
            {
                DrawThin(g, pen, draw);
            }
        }
        finally
        {
            g.SmoothingMode = saved;
        }
    }

    private static bool IsWideDash(Pen pen) =>
        pen.DashStyle != System.Drawing.Drawing2D.DashStyle.Solid && pen.Width > 1f;

    private static void DrawThin(Graphics g, Pen pen, Action<Pen> draw)
    {
        float width = pen.Width;
        pen.Width = 1f;
        try
        {
            draw(pen);
        }
        catch (OutOfMemoryException)
        {
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
        }
        finally
        {
            pen.Width = width;
        }
    }

    private static bool SamePoint(
        (double X, double Y)? left,
        (double X, double Y)? right) =>
        left is null && right is null ||
        left is (double lx, double ly) &&
        right is (double rx, double ry) &&
        Math.Abs(lx - rx) < 1e-6 &&
        Math.Abs(ly - ry) < 1e-6;

    private void DrawPoint(Graphics g, Brush brush, double localX, double localY, int radius)
    {
        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(localX, localY, Document.Frame);
        PointF screen = ImageToScreen(sensorX, sensorY);
        g.FillEllipse(brush, screen.X - radius, screen.Y - radius, radius * 2, radius * 2);
    }

    private void DrawArc(Graphics g, Pen pen, PathSegment segment) =>
        DrawArc(g, pen, segment.X1, segment.Y1, segment.X2, segment.Y2, segment.X3, segment.Y3);

    private void DrawArc(Graphics g, Pen pen, double x1, double y1, double x2, double y2, double x3, double y3)
    {
        var points = new[]
        {
            CoordinateTransform.LocalToSensor(x1, y1, Document.Frame),
            CoordinateTransform.LocalToSensor(x2, y2, Document.Frame),
            CoordinateTransform.LocalToSensor(x3, y3, Document.Frame)
        };

        if (!TryArcBounds(points[0], points[1], points[2], out RectangleF bounds, out float startAngle, out float sweepAngle))
        {
            g.DrawLine(pen, ImageToScreen(points[0].X, points[0].Y), ImageToScreen(points[2].X, points[2].Y));
            return;
        }

        var topLeft = ImageToScreen(bounds.Left, bounds.Top);
        var bottomRight = ImageToScreen(bounds.Right, bounds.Bottom);
        var screenBounds = RectangleF.FromLTRB(
            Math.Min(topLeft.X, bottomRight.X),
            Math.Min(topLeft.Y, bottomRight.Y),
            Math.Max(topLeft.X, bottomRight.X),
            Math.Max(topLeft.Y, bottomRight.Y));

        if (_space.InvertArcSweep)
        {
            startAngle = -startAngle;
            sweepAngle = -sweepAngle;
        }

        if (screenBounds.Width > 1 && screenBounds.Height > 1)
        {
            DrawStroke(g, pen, drawPen => g.DrawArc(drawPen, screenBounds, startAngle, sweepAngle));
        }
    }

    private static bool TryArcBounds(
        (double X, double Y) start,
        (double X, double Y) mid,
        (double X, double Y) end,
        out RectangleF bounds,
        out float startAngle,
        out float sweepAngle)
    {
        bounds = RectangleF.Empty;
        startAngle = 0;
        sweepAngle = 0;

        double d = 2 * (start.X * (mid.Y - end.Y) +
                        mid.X * (end.Y - start.Y) +
                        end.X * (start.Y - mid.Y));
        if (Math.Abs(d) < 1e-6)
        {
            return false;
        }

        double startSq = start.X * start.X + start.Y * start.Y;
        double midSq = mid.X * mid.X + mid.Y * mid.Y;
        double endSq = end.X * end.X + end.Y * end.Y;

        double centerX = (startSq * (mid.Y - end.Y) +
                          midSq * (end.Y - start.Y) +
                          endSq * (start.Y - mid.Y)) / d;
        double centerY = (startSq * (end.X - mid.X) +
                          midSq * (start.X - end.X) +
                          endSq * (mid.X - start.X)) / d;

        double radius = Math.Sqrt((start.X - centerX) * (start.X - centerX) +
                                  (start.Y - centerY) * (start.Y - centerY));

        bounds = new RectangleF(
            (float)(centerX - radius),
            (float)(centerY - radius),
            (float)(radius * 2),
            (float)(radius * 2));

        startAngle = (float)(Math.Atan2(start.Y - centerY, start.X - centerX) * 180 / Math.PI);
        float midAngle = (float)(Math.Atan2(mid.Y - centerY, mid.X - centerX) * 180 / Math.PI);
        float endAngle = (float)(Math.Atan2(end.Y - centerY, end.X - centerX) * 180 / Math.PI);

        float sweepToMid = NormalizeSweep(startAngle, midAngle);
        float sweepToEnd = NormalizeSweep(startAngle, endAngle);
        sweepAngle = Math.Abs(sweepToMid) <= Math.Abs(NormalizeSweep(startAngle, endAngle - Math.Sign(sweepToMid) * 360))
            ? sweepToEnd
            : sweepToEnd - Math.Sign(sweepToEnd) * 360;

        return true;
    }

    private static float NormalizeSweep(float startAngle, float targetAngle)
    {
        float delta = targetAngle - startAngle;
        while (delta <= -180) delta += 360;
        while (delta > 180) delta -= 360;
        return delta;
    }

    private void DrawProjectGrid(Graphics graphics) =>
        _gridPainter.Draw(
            graphics,
            Document.Grid,
            Document.Frame,
            ClientSize,
            _zoom,
            _panOffset,
            GridToScreen,
            ScreenToGrid);

    private PointF GridToScreen(double gridX, double gridY)
    {
        (double sensorX, double sensorY) = GridToSensor(gridX, gridY);
        return ImageToScreen(sensorX, sensorY);
    }

    private (double X, double Y) ScreenToGrid(Point screen)
    {
        PointF sensor = ScreenToImage(screen);
        return SensorToGrid(sensor.X, sensor.Y);
    }

    /// <summary>
    /// Grid values use stored path units. Teach and a calibrated CV project store millimeters.
    /// An uncalibrated CV frame still stores pixels, so the same numbers are frame pixels.
    /// </summary>
    private (double X, double Y) SensorToGrid(double sensorX, double sensorY)
    {
        if (_space is TeachCanvasSpace)
        {
            return (sensorX, sensorY);
        }

        (double localX, double localY) = CoordinateTransform.SensorToLocal(sensorX, sensorY, Document.Frame);
        return StorePixels(localX, localY);
    }

    private (double X, double Y) GridToSensor(double gridX, double gridY)
    {
        if (_space is TeachCanvasSpace)
        {
            return (gridX, gridY);
        }

        (double pixelX, double pixelY) = StoredToPixels(gridX, gridY);
        return CoordinateTransform.LocalToSensor(pixelX, pixelY, Document.Frame);
    }

    private string GridUnitLabel() =>
        _space is TeachCanvasSpace || TryStoredScale(out _) ? "mm" : "px";

    private PointF ScreenToImage(Point screenPoint) =>
        _space.ScreenYIncreasesUp
            ? new PointF(
                (screenPoint.X - _panOffset.X) / _zoom,
                (_panOffset.Y - screenPoint.Y) / _zoom)
            : new PointF(
                (screenPoint.X - _panOffset.X) / _zoom,
                (screenPoint.Y - _panOffset.Y) / _zoom);

    private PointF ImageToScreen(double imageX, double imageY) =>
        _space.ScreenYIncreasesUp
            ? new PointF(
                (float)(imageX * _zoom + _panOffset.X),
                (float)(-imageY * _zoom + _panOffset.Y))
            : new PointF(
                (float)(imageX * _zoom + _panOffset.X),
                (float)(imageY * _zoom + _panOffset.Y));

    public void AddReferencePoints(IReadOnlyList<ReferencePoint> points)
    {
        if (points.Count == 0)
        {
            return;
        }

        if (!_space.HasReferencePoints)
        {
            return;
        }

        bool firstPoints = Document.ReferencePoints.Count == 0;
        CommitUndoSnapshot();
        Document.ReferencePoints.AddRange(points.Select(static point => point.Clone()));
        ApplyMeasuredBounds();
        if (firstPoints)
        {
            FitToView();
        }

        RaiseDocumentChanged();
        StatusMessage?.Invoke(this, $"Added {points.Count} teach point(s)");
        Invalidate();
    }

    public void RemoveReferencePoint(string id)
    {
        int index = Document.ReferencePoints.FindIndex(point => point.Id == id);
        if (index < 0)
        {
            return;
        }

        CommitUndoSnapshot();
        Document.ReferencePoints.RemoveAt(index);
        ApplyMeasuredBounds();
        RaiseDocumentChanged();
        StatusMessage?.Invoke(this, "Teach point removed");
        Invalidate();
    }

    public void AlignGridTo(double x, double y)
    {
        if (!_space.HasGrid)
        {
            return;
        }

        CommitUndoSnapshot();
        Document.Grid.OriginX_mm = x;
        Document.Grid.OriginY_mm = y;
        Document.Grid.IsAligned = true;
        Document.Grid.Enabled = true;
        RaiseDocumentChanged();
        StatusMessage?.Invoke(this, $"Grid origin set to ({x:0.###}, {y:0.###}) {GridUnitLabel()}");
        Invalidate();
    }

    public void SetGridEnabled(bool enabled)
    {
        if (!_space.HasGrid || Document.Grid.Enabled == enabled)
        {
            return;
        }

        Document.Grid.Enabled = enabled;
        Invalidate();
    }

    public void SetGridStep(double stepMm, IReadOnlyList<double> steps)
    {
        if (!_space.HasGrid || stepMm <= 0)
        {
            return;
        }

        Document.Grid.Steps_mm = steps.Count > 0
            ? steps.ToList()
            : new List<double>(TeachGridSettings.DefaultStepsMm);
        bool stepChanged = Math.Abs(Document.Grid.Step_mm - stepMm) >= 1e-9;
        Document.Grid.Step_mm = stepMm;
        if (stepChanged)
        {
            Invalidate();
        }
    }

    public void ClearHistory()
    {
        _undoRedo.Clear();
        RaiseUndoStateChanged();
    }

    public void AdoptFrameMillimeters(RobotCalibrationFile? calibration, bool recordUndo)
    {
        bool converting = recordUndo &&
                          Document.SchemaVersion < FrameMillimeters.SchemaVersion &&
                          Document.Segments.Count > 0 &&
                          FrameMillimeters.TryGetScale(Document.Frame, calibration, out _);
        if (converting)
        {
            CommitUndoSnapshot();
        }

        if (FrameMillimeters.Migrate(Document, calibration))
        {
            RaiseDocumentChanged();
            Invalidate();
        }
    }

    public void ResetProject()
    {
        ProcessDefaults defaults = ProcessDefaultsCopy.Clone(Document.Defaults);
        ExporterSettings exporters = Document.Exporters.Clone();
        _suppressUndo = true;
        try
        {
            ClearMosaicTiles();
            _image?.Dispose();
            _image = null;
            Document.SchemaVersion = 4;
            Document.Workspace = WorkspaceKind.Unset;
            _space = new CvCanvasSpace();
            Document.ImagePath = null;
            Document.CalibrationPath = null;
            Document.ActiveTileId = null;
            Document.Frame = new FrameDefinition();
            Document.FrameIsDefined = false;
            Document.Defaults = defaults;
            Document.Exporters = exporters;
            Document.Grid = new TeachGridSettings();
            Document.ReferencePoints.Clear();
            Document.Tiles.Clear();
            Document.Segments.Clear();
            _canvasBounds = RectangleF.Empty;
            _zoom = 1f;
            _panOffset = PointF.Empty;
            SyncScrollBars();
            SelectedSegmentIndex = null;
            _selectedPoint = null;
            _hoverPoint = null;
            _hoverShapeIndex = null;
            _pendingPoints.Clear();
            _cursorLocal = null;
            _snapHover = null;
            _isDraggingVertex = false;
            _activeVertexPick = null;
            Capture = false;
            DraftFrame = null;
            _undoRedo.Clear();
        }
        finally
        {
            _suppressUndo = false;
        }

        RaiseDocumentChanged();
        RaiseSelectionChanged();
        RaiseUndoStateChanged();
        Invalidate();
    }

    private void ApplyMeasuredBounds()
    {
        if (_space.TryMeasureBackground(Document, out RectangleF bounds))
        {
            _canvasBounds = bounds;
            SyncScrollBars();
        }
    }

    private Size ViewSize
    {
        get
        {
            int barWidth = SystemInformation.VerticalScrollBarWidth;
            int barHeight = SystemInformation.HorizontalScrollBarHeight;
            return new Size(
                Math.Max(1, ClientSize.Width - barWidth),
                Math.Max(1, ClientSize.Height - barHeight));
        }
    }

    private void BeginPan(Point location)
    {
        _isPanning = true;
        _lastMouse = location;
        Capture = true;
        Cursor = Cursors.SizeAll;
    }

    private bool IsOverSelectable(Point screen)
    {
        if (!HasCanvas)
        {
            return false;
        }

        PointF image = ScreenToImage(screen);
        return TryPickPoint(image.X, image.Y, out _) ||
               TryPickShape(image.X, image.Y, out _);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutScrollBars();
        SyncScrollBars();
    }

    private void LayoutScrollBars()
    {
        int barWidth = SystemInformation.VerticalScrollBarWidth;
        int barHeight = SystemInformation.HorizontalScrollBarHeight;
        int width = Math.Max(0, ClientSize.Width - barWidth);
        int height = Math.Max(0, ClientSize.Height - barHeight);
        _verticalScroll.SetBounds(ClientSize.Width - barWidth, 0, barWidth, height);
        _horizontalScroll.SetBounds(0, ClientSize.Height - barHeight, width, barHeight);
        _scrollCorner.SetBounds(ClientSize.Width - barWidth, ClientSize.Height - barHeight, barWidth, barHeight);
    }

    private RectangleF ScrollWorld()
    {
        RectangleF content = _canvasBounds;
        if (content.Width < 1 || content.Height < 1)
        {
            content = new RectangleF(-50, -50, 100, 100);
        }

        content = RectangleF.Union(content, new RectangleF(-5, -5, 10, 10));
        float padX = Math.Max(ViewSize.Width / Math.Max(_zoom, 0.001f), 10);
        float padY = Math.Max(ViewSize.Height / Math.Max(_zoom, 0.001f), 10);
        return RectangleF.Inflate(content, padX, padY);
    }

    private void SyncScrollBars()
    {
        if (_adjustingScroll)
        {
            return;
        }

        RectangleF world = ScrollWorld();
        int maxX = ScrollLimit(world.Width * _zoom, ViewSize.Width);
        int maxY = ScrollLimit(world.Height * _zoom, ViewSize.Height);
        int scrollX = Math.Clamp((int)Math.Round(-world.Left * _zoom - _panOffset.X), 0, maxX);
        int scrollY = Math.Clamp(
            _space.ScreenYIncreasesUp
                ? (int)Math.Round(world.Bottom * _zoom - _panOffset.Y)
                : (int)Math.Round(-world.Top * _zoom - _panOffset.Y),
            0,
            maxY);
        _panOffset = _space.ScreenYIncreasesUp
            ? new PointF(-world.Left * _zoom - scrollX, world.Bottom * _zoom - scrollY)
            : new PointF(-world.Left * _zoom - scrollX, -world.Top * _zoom - scrollY);
        _adjustingScroll = true;
        try
        {
            SetScrollBar(_horizontalScroll, scrollX, world.Width * _zoom, ViewSize.Width);
            SetScrollBar(_verticalScroll, scrollY, world.Height * _zoom, ViewSize.Height);
        }
        finally
        {
            _adjustingScroll = false;
        }
    }

    private void OnScrollValueChanged()
    {
        if (_adjustingScroll)
        {
            return;
        }

        RectangleF world = ScrollWorld();
        _panOffset = _space.ScreenYIncreasesUp
            ? new PointF(
                -world.Left * _zoom - _horizontalScroll.Value,
                world.Bottom * _zoom - _verticalScroll.Value)
            : new PointF(
                -world.Left * _zoom - _horizontalScroll.Value,
                -world.Top * _zoom - _verticalScroll.Value);
        Invalidate();
    }

    private static int ScrollLimit(float worldPixels, int viewport)
    {
        int extent = ScrollExtent(worldPixels);
        int large = Math.Clamp(Math.Max(viewport, 1), 1, extent);
        return Math.Max(0, extent - large);
    }

    private static int ScrollExtent(float worldPixels) =>
        (int)Math.Clamp(Math.Ceiling(Math.Max(worldPixels, 1)), 1, 1_000_000_000);

    private static void SetScrollBar(ScrollBar bar, int value, float worldPixels, int viewport)
    {
        int extent = ScrollExtent(worldPixels);
        int large = Math.Clamp(Math.Max(viewport, 1), 1, extent);
        int maximum = Math.Max(extent - 1, 0);
        if (large > maximum + 1)
        {
            large = maximum + 1;
        }

        int maxValue = Math.Max(0, maximum - large + 1);
        int clamped = Math.Clamp(value, 0, maxValue);
        if (bar.Value > maximum)
        {
            bar.Value = 0;
        }

        bar.Maximum = maximum;
        bar.LargeChange = large;
        bar.SmallChange = Math.Max(1, large / 10);
        if (bar.Value != clamped)
        {
            bar.Value = clamped;
        }
    }

    private void RaiseDocumentChanged() => DocumentChanged?.Invoke(this, EventArgs.Empty);
    private void RaiseSelectionChanged() => SelectionChanged?.Invoke(this, EventArgs.Empty);
    private void RaiseUndoStateChanged() => UndoStateChanged?.Invoke(this, EventArgs.Empty);
}
