using FanucPathAuthor.Models;
using FanucPathAuthor.Services;

namespace FanucPathAuthor.Controls;

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
    private readonly List<MosaicTilePlacement> _mosaicTiles = new();
    private RectangleF _canvasBounds = RectangleF.Empty;
    private RobotCalibrationFile? _mosaicCalibration;
    private readonly List<(double X, double Y)> _pendingPoints = new();
    private readonly UndoRedoStack<PathDocument> _undoRedo = new(DocumentClone.Clone);
    private float _zoom = 1f;
    private PointF _panOffset = PointF.Empty;
    private bool _isPanning;
    private Point _lastMouse;
    private (double X, double Y)? _cursorLocal;
    private (double SensorX, double SensorY)? _snapHover;
    private bool _suppressUndo;
    private bool _isDraggingVertex;
    private VertexPick? _activeVertexPick;

    public PathDocument Document { get; } = new();
    private DrawTool _activeTool = DrawTool.Dot;

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
        }
    }
    public bool SnapToPointEnabled { get; set; }
    public int? SelectedSegmentIndex { get; private set; }
    public bool SimulationMode { get; set; }
    public SimulationOverlay? SimulationOverlay { get; set; }
    public bool CanUndo => _undoRedo.CanUndo;
    public bool CanRedo => _undoRedo.CanRedo;
    public bool HasPendingPoints => _pendingPoints.Count > 0;

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
    }

    public void LoadImage(string path)
    {
        _image?.Dispose();
        _image = Image.FromFile(path);
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

        if (_mosaicCalibration?.PixelToRobot != null && Document.Tiles.Count > 0)
        {
            MosaicLayout? layout = MosaicLayoutService.TryBuild(Document, _mosaicCalibration);
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
        if (activeTile != null &&
            !string.IsNullOrWhiteSpace(activeTile.ImagePath) &&
            File.Exists(activeTile.ImagePath))
        {
            _image = Image.FromFile(activeTile.ImagePath);
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

        float scaleX = ClientSize.Width / tileBounds.Width;
        float scaleY = ClientSize.Height / tileBounds.Height;
        _zoom = Math.Min(scaleX, scaleY) * 0.9f;
        _panOffset = new PointF(
            (ClientSize.Width - tileBounds.Width * _zoom) / 2f - tileBounds.Left * _zoom,
            (ClientSize.Height - tileBounds.Height * _zoom) / 2f - tileBounds.Top * _zoom);
        Invalidate();
    }

    private bool HasCanvas => _mosaicTiles.Count > 0 || _image != null;

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
        if (!HasCanvas || _canvasBounds.Width <= 0 || _canvasBounds.Height <= 0 ||
            ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        float scaleX = ClientSize.Width / _canvasBounds.Width;
        float scaleY = ClientSize.Height / _canvasBounds.Height;
        _zoom = Math.Min(scaleX, scaleY) * 0.95f;
        _panOffset = new PointF(
            (ClientSize.Width - _canvasBounds.Width * _zoom) / 2f - _canvasBounds.Left * _zoom,
            (ClientSize.Height - _canvasBounds.Height * _zoom) / 2f - _canvasBounds.Top * _zoom);
        Invalidate();
    }

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
        if (Math.Abs(Document.Frame.OriginX_px - originX) < double.Epsilon &&
            Math.Abs(Document.Frame.OriginY_px - originY) < double.Epsilon &&
            Math.Abs(Document.Frame.AngleDeg - angleDeg) < double.Epsilon)
        {
            DraftFrame = null;
            Invalidate();
            return;
        }

        CommitUndoSnapshot();

        var oldFrame = new FrameDefinition
        {
            OriginX_px = Document.Frame.OriginX_px,
            OriginY_px = Document.Frame.OriginY_px,
            AngleDeg = Document.Frame.AngleDeg
        };

        var newFrame = new FrameDefinition
        {
            OriginX_px = originX,
            OriginY_px = originY,
            AngleDeg = angleDeg
        };

        if (moveShapesWithFrame)
        {
            FrameTransformHelper.RebindSegmentsToFrame(Document.Segments, oldFrame, newFrame);
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
            AngleDeg = angleDeg
        };
        Invalidate();
    }

    public void ClearDraftFrame()
    {
        DraftFrame = null;
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
            Document.CalibrationPath = document.CalibrationPath;
            Document.ActiveTileId = document.ActiveTileId;
            Document.Frame = document.Frame;
            Document.FrameIsDefined = document.FrameIsDefined;
            Document.Defaults = document.Defaults;
            Document.Segments.Clear();
            Document.Segments.AddRange(document.Segments.Select(DocumentClone.CloneSegment));
            Document.Tiles.Clear();
            Document.Tiles.AddRange(document.Tiles.Select(DocumentClone.CloneTile));
            Document.ImagePath = document.ImagePath;

            TileDocumentHelper.EnsureLegacyMigrated(Document);
            RebuildMosaic();

            SelectedSegmentIndex = null;
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
            ImagePath = Document.ImagePath,
            CalibrationPath = Document.CalibrationPath,
            ActiveTileId = Document.ActiveTileId,
            Frame = new FrameDefinition
            {
                OriginX_px = Document.Frame.OriginX_px,
                OriginY_px = Document.Frame.OriginY_px,
                AngleDeg = Document.Frame.AngleDeg
            },
            FrameIsDefined = Document.FrameIsDefined,
            Defaults = ProcessDefaultsCopy.Clone(Document.Defaults),
            Tiles = Document.Tiles.Select(DocumentClone.CloneTile).ToList(),
            Segments = Document.Segments
                .Select(static s => new PathSegment
                {
                    Kind = s.Kind,
                    GlueOn = s.GlueOn,
                    Speed_mm_s = s.Speed_mm_s,
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
        if (_image == null && _mosaicTiles.Count == 0)
        {
            base.OnMouseWheel(e);
            return;
        }

        float oldZoom = _zoom;
        float factor = e.Delta > 0 ? 1.1f : 0.9f;
        float newZoom = Math.Clamp(_zoom * factor, 0.05f, 20f);
        if (Math.Abs(oldZoom - newZoom) < float.Epsilon)
        {
            base.OnMouseWheel(e);
            return;
        }

        // Keep the image point under the cursor fixed while zoom changes.
        float imageX = (e.X - _panOffset.X) / oldZoom;
        float imageY = (e.Y - _panOffset.Y) / oldZoom;
        _zoom = newZoom;
        _panOffset.X = e.X - imageX * _zoom;
        _panOffset.Y = e.Y - imageY * _zoom;
        Invalidate();

        base.OnMouseWheel(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        Focus();

        if (e.Button == MouseButtons.Middle ||
            (e.Button == MouseButtons.Left && ModifierKeys == Keys.Control))
        {
            _isPanning = true;
            _lastMouse = e.Location;
            Cursor = Cursors.Hand;
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
                SetFrameFromSensorPoint(sensorX, sensorY);
                break;

            case DrawTool.Select:
                SelectSegmentAt(sensorX, sensorY);
                break;

            case DrawTool.Move:
                BeginVertexDrag(sensorX, sensorY);
                break;

            case DrawTool.Dot:
            {
                (double drawX, double drawY, bool snapped) = ResolveDrawClick(sensorX, sensorY);
                if (snapped && !ConfirmDuplicateDot(drawX, drawY))
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
            Invalidate();
        }
        else if (_isDraggingVertex && _activeVertexPick != null && HasCanvas && !SimulationMode)
        {
            PointF imagePoint = ScreenToImage(e.Location);
            (double localX, double localY) = CoordinateTransform.SensorToLocal(
                imagePoint.X,
                imagePoint.Y,
                Document.Frame);
            PointEditHelper.ApplyDrag(Document, _activeVertexPick, localX, localY);

            if (SnapToPointEnabled)
            {
                (double resolvedX, double resolvedY, bool snapped) = ResolveSnapPosition(
                    imagePoint.X,
                    imagePoint.Y,
                    GetSnapExcludePoints(_activeVertexPick));
                _snapHover = snapped ? (resolvedX, resolvedY) : null;
            }
            else
            {
                ClearSnapHover();
            }

            Invalidate();
        }
        else if (HasCanvas)
        {
            PointF imagePoint = ScreenToImage(e.Location);
            double sensorX = imagePoint.X;
            double sensorY = imagePoint.Y;

            if (ActiveTool == DrawTool.Move)
            {
                UpdateMoveHover(sensorX, sensorY);
            }
            else if (SnapToPointEnabled && IsDrawTool(ActiveTool))
            {
                (double resolvedX, double resolvedY, bool snapped) = ResolveDrawClick(sensorX, sensorY);
                sensorX = resolvedX;
                sensorY = resolvedY;
                _snapHover = snapped ? (resolvedX, resolvedY) : null;
            }
            else
            {
                ClearSnapHover();
            }

            (double localX, double localY) = CoordinateTransform.SensorToLocal(
                sensorX,
                sensorY,
                Document.Frame);
            _cursorLocal = (localX, localY);
            CursorMoved?.Invoke(this, (Math.Round(sensorX), Math.Round(sensorY), localX, localY));

            if (_pendingPoints.Count > 0 ||
                ActiveTool == DrawTool.Move ||
                (SnapToPointEnabled && IsDrawTool(ActiveTool)))
            {
                Invalidate();
            }
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        if (!_isDraggingVertex)
        {
            ClearSnapHover();
            if (ActiveTool == DrawTool.Move)
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

        DrawFrame(e.Graphics, Document.Frame, Color.Lime, "Frame");
        if (DraftFrame != null &&
            (Math.Abs(DraftFrame.OriginX_px - Document.Frame.OriginX_px) > double.Epsilon ||
             Math.Abs(DraftFrame.OriginY_px - Document.Frame.OriginY_px) > double.Epsilon ||
             Math.Abs(DraftFrame.AngleDeg - Document.Frame.AngleDeg) > double.Epsilon))
        {
            DrawFrame(e.Graphics, DraftFrame, Color.Orange, "Draft");
        }
        DrawSegments(e.Graphics);
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
            ClearMosaicTiles();
            _image?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void HandlePolylineClick((double SensorX, double SensorY) sensorPoint, int requiredPoints, SegmentKind kind)
    {
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
            Speed_mm_s = Document.Defaults.Speed_mm_s,
            X1 = _pendingPoints[0].X,
            Y1 = _pendingPoints[0].Y
        };

        if (requiredPoints >= 2)
        {
            segment.X2 = _pendingPoints[1].X;
            segment.Y2 = _pendingPoints[1].Y;
        }

        if (requiredPoints >= 3)
        {
            segment.X3 = _pendingPoints[2].X;
            segment.Y3 = _pendingPoints[2].Y;
        }

        Document.Segments.Add(segment);
        _pendingPoints.Clear();
        _cursorLocal = null;
        SelectedSegmentIndex = Document.Segments.Count - 1;
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        Invalidate();
    }

    private void HandleRectangleClick((double SensorX, double SensorY) sensorPoint)
    {
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
            Document.Defaults.GlueOn,
            Document.Defaults.Speed_mm_s);

        Document.Segments.AddRange(lines);
        _pendingPoints.Clear();
        _cursorLocal = null;
        SelectedSegmentIndex = Document.Segments.Count - 1;
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        StatusMessage?.Invoke(this, "Rectangle added (4 lines)");
        Invalidate();
    }

    private void HandleCircleClick((double SensorX, double SensorY) sensorPoint)
    {
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
            Document.Defaults.GlueOn,
            Document.Defaults.Speed_mm_s);

        if (arcs.Count == 0)
        {
            _pendingPoints.Clear();
            StatusMessage?.Invoke(this, "Circle radius is too small");
            Invalidate();
            return;
        }

        CommitUndoSnapshot();

        Document.Segments.AddRange(arcs);
        _pendingPoints.Clear();
        _cursorLocal = null;
        SelectedSegmentIndex = Document.Segments.Count - 1;
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        StatusMessage?.Invoke(this, "Circle added (2 arcs)");
        Invalidate();
    }

    private void AddDot(double sensorX, double sensorY)
    {
        (double localX, double localY) = CoordinateTransform.SensorToLocal(
            sensorX,
            sensorY,
            Document.Frame);

        CommitUndoSnapshot();

        Document.Segments.Add(new PathSegment
        {
            Kind = SegmentKind.Dot,
            GlueOn = Document.Defaults.GlueOn,
            Speed_mm_s = Document.Defaults.Speed_mm_s,
            X1 = localX,
            Y1 = localY
        });

        SelectedSegmentIndex = Document.Segments.Count - 1;
        RaiseDocumentChanged();
        RaiseSelectionChanged();
        Invalidate();
    }

    public void SelectSegment(int? index)
    {
        if (index is int i && (i < 0 || i >= Document.Segments.Count))
        {
            index = null;
        }

        SelectedSegmentIndex = index;
        RaiseSelectionChanged();
        Invalidate();
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
                out VertexPick? pick) ||
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

        if (SnapToPointEnabled && HasCanvas)
        {
            PointF imagePoint = ScreenToImage(screenLocation);
            (double sensorX, double sensorY, bool snapped) = ResolveSnapPosition(
                imagePoint.X,
                imagePoint.Y,
                GetSnapExcludePoints(_activeVertexPick));
            if (snapped)
            {
                (double localX, double localY) = CoordinateTransform.SensorToLocal(
                    sensorX,
                    sensorY,
                    Document.Frame);
                PointEditHelper.ApplyDrag(Document, _activeVertexPick, localX, localY);
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
        IReadOnlyList<(double SensorX, double SensorY)> points = SnapPointHelper.CollectSensorPoints(Document);
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
            ? SnapPointHelper.CollectSensorPoints(Document)
            : SnapPointHelper.CollectSensorPointsExcluding(Document, excludeLocal);

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

        return (sensorX, sensorY, false);
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
        if (!SnapToPointEnabled || !IsDrawTool(ActiveTool))
        {
            return (sensorX, sensorY, false);
        }

        (double snapX, double snapY, bool snapped) = ResolveSnapPosition(sensorX, sensorY);
        return snapped ? (snapX, snapY, true) : (Math.Round(sensorX), Math.Round(sensorY), false);
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
        IReadOnlyList<(double SensorX, double SensorY)> points = SnapPointHelper.CollectSensorPoints(Document);
        if (!SnapPointHelper.ContainsSensorPoint(points, sensorX, sensorY))
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

        if (_isDraggingVertex)
        {
            if (!SnapToPointEnabled)
            {
                return;
            }
        }
        else if (ActiveTool == DrawTool.Move)
        {
            // Always highlight pick targets in Move mode.
        }
        else if (!(SnapToPointEnabled && IsDrawTool(ActiveTool)))
        {
            return;
        }

        PointF screen = ImageToScreen(sensorX, sensorY);
        const float radius = 12f;
        using var pen = new Pen(Color.FromArgb(220, 120, 0), 2f)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
        };
        g.DrawEllipse(pen, screen.X - radius, screen.Y - radius, radius * 2, radius * 2);
    }

    private void SelectSegmentAt(double sensorX, double sensorY)
    {
        const double pickRadius = 12;
        SelectedSegmentIndex = null;

        for (int i = Document.Segments.Count - 1; i >= 0; i--)
        {
            PathSegment segment = Document.Segments[i];
            if (IsNearSegment(segment, sensorX, sensorY, pickRadius))
            {
                SelectedSegmentIndex = i;
                break;
            }
        }

        RaiseSelectionChanged();
        Invalidate();
    }

    private bool IsNearSegment(PathSegment segment, double sensorX, double sensorY, double radius)
    {
        foreach ((double localX, double localY) in GeometryHelper.ExpandSegment(segment))
        {
            var (sx, sy) = CoordinateTransform.LocalToSensor(localX, localY, Document.Frame);
            double dx = sx - sensorX;
            double dy = sy - sensorY;
            if (Math.Sqrt(dx * dx + dy * dy) <= radius)
            {
                return true;
            }
        }

        return false;
    }

    private void DrawFrame(Graphics g, FrameDefinition frame, Color color, string label)
    {
        var (originX, originY) = CoordinateTransform.LocalToSensor(0, 0, frame);
        PointF origin = ImageToScreen(originX, originY);
        var (axisX, _) = CoordinateTransform.LocalToSensor(80, 0, frame);
        var (_, axisY) = CoordinateTransform.LocalToSensor(0, 80, frame);
        PointF xEnd = ImageToScreen(axisX, originY);
        PointF yEnd = ImageToScreen(originX, axisY);

        using var pen = new Pen(color, 2);
        using var brush = new SolidBrush(color);
        g.DrawLine(pen, origin, xEnd);
        g.DrawLine(pen, origin, yEnd);
        g.FillEllipse(brush, origin.X - 4, origin.Y - 4, 8, 8);
        g.DrawString(label, Font, brush, origin.X + 6, origin.Y + 6);
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

        (double startX, double startY) = PathSequenceService.GetStartPoint(segments[0]);
        DrawSequenceLabel(g, startX, startY, "1s", Color.LimeGreen);

        int lastNumber = segments.Count;
        (double endX, double endY) = PathSequenceService.GetEndPoint(segments[lastNumber - 1]);
        DrawSequenceLabel(g, endX, endY, $"{lastNumber}e", Color.DeepSkyBlue);

        IReadOnlyList<SequenceGap> gaps = PathSequenceService.AnalyzeGaps(segments);
        foreach (SequenceGap gap in gaps)
        {
            if (!gap.HasGap(PathSequenceService.DefaultGapThresholdPx))
            {
                continue;
            }

            int fromNumber = gap.FromSegmentIndex + 1;
            int toNumber = gap.ToSegmentIndex + 1;
            DrawSequenceLabel(g, gap.FromX_px, gap.FromY_px, $"{fromNumber}e", Color.OrangeRed);
            DrawSequenceLabel(g, gap.ToX_px, gap.ToY_px, $"{toNumber}s", Color.OrangeRed);
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
            bool selected = SelectedSegmentIndex == i;
            Color color = segment.GlueOn ? Color.DeepSkyBlue : Color.Orange;
            if (selected)
            {
                color = Color.Yellow;
            }

            using var pen = new Pen(color, selected ? 3 : 2);
            using var brush = new SolidBrush(color);

            switch (segment.Kind)
            {
                case SegmentKind.Dot:
                    DrawPoint(g, brush, segment.X1, segment.Y1, 6);
                    break;

                case SegmentKind.Line:
                    DrawLine(g, pen, segment.X1, segment.Y1, segment.X2, segment.Y2);
                    DrawPoint(g, brush, segment.X1, segment.Y1, 4);
                    DrawPoint(g, brush, segment.X2, segment.Y2, 4);
                    break;

                case SegmentKind.Arc:
                    DrawArc(g, pen, segment);
                    DrawPoint(g, brush, segment.X1, segment.Y1, 4);
                    DrawPoint(g, brush, segment.X2, segment.Y2, 4);
                    DrawPoint(g, brush, segment.X3, segment.Y3, 4);
                    break;
            }
        }
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

        using var pen = new Pen(Color.Magenta, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };

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
                             glueOn: true,
                             speed_mm_s: 0))
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
                             glueOn: true,
                             speed_mm_s: 0))
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
            Document.SchemaVersion = snapshot.SchemaVersion;
            Document.ImagePath = snapshot.ImagePath;
            Document.CalibrationPath = snapshot.CalibrationPath;
            Document.ActiveTileId = snapshot.ActiveTileId;
            Document.Frame = new FrameDefinition
            {
                OriginX_px = snapshot.Frame.OriginX_px,
                OriginY_px = snapshot.Frame.OriginY_px,
                AngleDeg = snapshot.Frame.AngleDeg
            };
            Document.FrameIsDefined = snapshot.FrameIsDefined;
            Document.Defaults = ProcessDefaultsCopy.Clone(snapshot.Defaults);
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

    private void DrawLine(Graphics g, Pen pen, double lx1, double ly1, double lx2, double ly2)
    {
        var (sx1, sy1) = CoordinateTransform.LocalToSensor(lx1, ly1, Document.Frame);
        var (sx2, sy2) = CoordinateTransform.LocalToSensor(lx2, ly2, Document.Frame);
        g.DrawLine(pen, ImageToScreen(sx1, sy1), ImageToScreen(sx2, sy2));
    }

    private void DrawPoint(Graphics g, Brush brush, double localX, double localY, int radius)
    {
        var (sensorX, sensorY) = CoordinateTransform.LocalToSensor(localX, localY, Document.Frame);
        PointF screen = ImageToScreen(sensorX, sensorY);
        g.FillEllipse(brush, screen.X - radius, screen.Y - radius, radius * 2, radius * 2);
    }

    private void DrawArc(Graphics g, Pen pen, PathSegment segment)
    {
        var points = new[]
        {
            CoordinateTransform.LocalToSensor(segment.X1, segment.Y1, Document.Frame),
            CoordinateTransform.LocalToSensor(segment.X2, segment.Y2, Document.Frame),
            CoordinateTransform.LocalToSensor(segment.X3, segment.Y3, Document.Frame)
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

        if (screenBounds.Width > 1 && screenBounds.Height > 1)
        {
            g.DrawArc(pen, screenBounds, startAngle, sweepAngle);
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

    private PointF ScreenToImage(Point screenPoint) =>
        new(
            (screenPoint.X - _panOffset.X) / _zoom,
            (screenPoint.Y - _panOffset.Y) / _zoom);

    private PointF ImageToScreen(double imageX, double imageY) =>
        new(
            (float)(imageX * _zoom + _panOffset.X),
            (float)(imageY * _zoom + _panOffset.Y));

    private void RaiseDocumentChanged() => DocumentChanged?.Invoke(this, EventArgs.Empty);
    private void RaiseSelectionChanged() => SelectionChanged?.Invoke(this, EventArgs.Empty);
    private void RaiseUndoStateChanged() => UndoStateChanged?.Invoke(this, EventArgs.Empty);
}
