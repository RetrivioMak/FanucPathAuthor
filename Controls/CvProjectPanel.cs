using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public partial class CvProjectPanel : UserControl
{
    private bool _updating;
    private PathDocument? _document;
    private RobotCalibrationFile? _calibration;

    public CvProjectPanel()
    {
        InitializeComponent();
        _originX.ValueChanged += (_, _) => OnFrameDraftEdited();
        _originY.ValueChanged += (_, _) => OnFrameDraftEdited();
        _angle.ValueChanged += (_, _) => OnFrameDraftEdited();
        _invertY.CheckedChanged += (_, _) =>
        {
            if (!_updating)
            {
                InvertYChanged?.Invoke(this, EventArgs.Empty);
            }
        };
        _editFrameButton.Click += (_, _) => EditFrameRequested?.Invoke(this, EventArgs.Empty);
        _applyFrameButton.Click += (_, _) => ApplyFrameRequested?.Invoke(this, EventArgs.Empty);
        _cancelFrameButton.Click += (_, _) => CancelFrameRequested?.Invoke(this, EventArgs.Empty);
        _openImageButton.Click += (_, _) => OpenImageRequested?.Invoke(this, EventArgs.Empty);
        _addFovButton.Click += (_, _) => AddFovRequested?.Invoke(this, EventArgs.Empty);
        _removeTileButton.Click += (_, _) =>
        {
            if (SelectedTileId is string id)
            {
                RemoveTileRequested?.Invoke(this, id);
            }
        };
        _focusTileButton.Click += (_, _) =>
        {
            if (SelectedTileId is string id)
            {
                FocusTileRequested?.Invoke(this, id);
            }
        };
        _resetCaptureButton.Click += (_, _) => ResetCapturePoseRequested?.Invoke(this, EventArgs.Empty);
        _tileList.SelectedIndexChanged += (_, _) =>
        {
            if (!_updating && _document != null)
            {
                SyncTileEditors(_document, _calibration);
            }
        };
        _captureX.ValueChanged += (_, _) => OnTilePoseEdited();
        _captureY.ValueChanged += (_, _) => OnTilePoseEdited();
        _captureR.ValueChanged += (_, _) => OnTilePoseEdited();
        SetFrameEditing(false);
        SetTileEditorsEnabled(false);
    }

    public event EventHandler? EditFrameRequested;

    public event EventHandler? ApplyFrameRequested;

    public event EventHandler? CancelFrameRequested;

    public event EventHandler? FrameDraftChanged;

    public event EventHandler? InvertYChanged;

    public event EventHandler? OpenImageRequested;

    public event EventHandler? AddFovRequested;

    public event EventHandler<string>? RemoveTileRequested;

    public event EventHandler<string>? FocusTileRequested;

    public event EventHandler? ResetCapturePoseRequested;

    public event EventHandler? TilePoseChanged;

    public bool IsEditing { get; private set; }

    public bool MoveShapesWithFrame => _moveShapesRadio.Checked;

    public double OriginX => (double)_originX.Value;

    public double OriginY => (double)_originY.Value;

    public double AngleDeg => (double)_angle.Value;

    public bool InvertFrameY => _invertY.Checked;

    public double CaptureX => (double)_captureX.Value;

    public double CaptureY => (double)_captureY.Value;

    public double CaptureR => (double)_captureR.Value;

    public string? SelectedTileId =>
        _tileList.SelectedItems.Count == 0 ? null : _tileList.SelectedItems[0].Tag as string;

    public void ShowFrame(FrameDefinition frame)
    {
        _updating = true;
        try
        {
            _originX.Value = Clamp(_originX, (decimal)frame.OriginX_px);
            _originY.Value = Clamp(_originY, (decimal)frame.OriginY_px);
            _angle.Value = Clamp(_angle, (decimal)frame.AngleDeg);
            _invertY.Checked = frame.InvertY;
        }
        finally
        {
            _updating = false;
        }

        if (!IsEditing)
        {
            SetFrameEditing(false);
        }
    }

    public void SetOrigin(double x, double y)
    {
        _updating = true;
        try
        {
            _originX.Value = Clamp(_originX, (decimal)Math.Round(x));
            _originY.Value = Clamp(_originY, (decimal)Math.Round(y));
        }
        finally
        {
            _updating = false;
        }
    }

    public void BeginEdit() => SetFrameEditing(true);

    public void EndEdit() => SetFrameEditing(false);

    public void SetCalibrationText(string text) => _calibrationLabel.Text = text;

    public void ShowTiles(PathDocument document, RobotCalibrationFile? calibration)
    {
        _document = document;
        _calibration = calibration;
        string? selectedId = SelectedTileId;
        string? activeId = document.ActiveTileId;

        _updating = true;
        try
        {
            _tileList.Items.Clear();
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

            SelectTile(selectedId, document);
            UpdateFovMode(document);
            SyncTileEditors(document, calibration);
        }
        finally
        {
            _updating = false;
        }
    }

    private void SelectTile(string? selectedId, PathDocument document)
    {
        if (!string.IsNullOrWhiteSpace(selectedId))
        {
            foreach (ListViewItem item in _tileList.Items)
            {
                if (item.Tag is string id && id == selectedId)
                {
                    item.Selected = true;
                    return;
                }
            }
        }

        if (_tileList.Items.Count == 0)
        {
            return;
        }

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

    private void UpdateFovMode(PathDocument document)
    {
        bool hasTiles = document.Tiles.Count > 0;
        bool multi = TileDocumentHelper.IsMultiFovMode(document);
        _modeLabel.Text = multi ? "Mode: Multi-FOV mosaic" : "Mode: Single FOV";
        _captureHint.Text = multi
            ? "Anchor FOV is the first image. Frame and path use anchor coordinates. Additional FOV tiles are placed on the virtual mosaic canvas."
            : hasTiles
                ? document.FrameIsDefined
                    ? "Single FOV mode. You can add another FOV image to switch to multi-FOV mosaic."
                    : "Single FOV mode. Define Frame on the anchor image before adding more FOV images."
                : "Open an FOV image to start. Frame and path stay in sensor pixels.";
        _openImageButton.Visible = !hasTiles;
        _openImageButton.Enabled = true;
        _addFovButton.Visible = hasTiles;
        _addFovButton.Enabled = TileDocumentHelper.CanAddAnotherFovImage(document);
    }

    private void SyncTileEditors(PathDocument document, RobotCalibrationFile? calibration)
    {
        FovTile? tile = SelectedTile(document);
        SetTileEditorsEnabled(tile != null);
        if (tile == null)
        {
            _offsetLabel.Text = "Canvas offset: ！";
            _worldLabel.Text = "Tile (0,0) world: ！";
            return;
        }

        _captureX.Value = Clamp(_captureX, (decimal)tile.RobotX_mm);
        _captureY.Value = Clamp(_captureY, (decimal)tile.RobotY_mm);
        _captureR.Value = Clamp(_captureR, (decimal)tile.RobotR_deg);
        UpdateTileTransformInfo(document, calibration, tile);
    }

    private void UpdateTileTransformInfo(PathDocument document, RobotCalibrationFile? calibration, FovTile tile)
    {
        FovTile? anchor = TileDocumentHelper.GetAnchor(document);
        if (anchor == null)
        {
            _offsetLabel.Text = "Canvas offset: ！";
            _worldLabel.Text = "Tile (0,0) world: ！";
            return;
        }

        if (calibration?.PixelToRobot == null)
        {
            _offsetLabel.Text = "Canvas offset: load calibration";
            _worldLabel.Text = "Tile (0,0) world: load calibration";
            return;
        }

        try
        {
            (double worldX, double worldY) = TileTransformService.TilePixelToWorld(0, 0, tile, calibration);
            _worldLabel.Text = $"Tile (0,0) world: ({worldX:0.##}, {worldY:0.##}) mm";
            if (tile.Id == anchor.Id)
            {
                _offsetLabel.Text = "Canvas offset: (0, 0) px ！ anchor";
                return;
            }

            (double offsetX, double offsetY) = TileTransformService.GetTileCanvasOffset(tile, anchor, calibration);
            _offsetLabel.Text = $"Canvas offset: ({offsetX:0.#}, {offsetY:0.#}) px";
        }
        catch (Exception ex)
        {
            _offsetLabel.Text = $"Canvas offset: {ex.Message}";
            _worldLabel.Text = "Tile (0,0) world: ！";
        }
    }

    private FovTile? SelectedTile(PathDocument document) =>
        SelectedTileId is string id
            ? document.Tiles.FirstOrDefault(tile => tile.Id == id)
            : null;

    private void OnFrameDraftEdited()
    {
        if (!_updating && IsEditing)
        {
            FrameDraftChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnTilePoseEdited()
    {
        if (!_updating)
        {
            TilePoseChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SetFrameEditing(bool enabled)
    {
        IsEditing = enabled;
        _originX.Enabled = enabled;
        _originY.Enabled = enabled;
        _angle.Enabled = enabled;
        _moveShapesRadio.Enabled = enabled;
        _frameOnlyRadio.Enabled = enabled;
        _applyFrameButton.Enabled = enabled;
        _cancelFrameButton.Enabled = enabled;
        _editFrameButton.Enabled = !enabled;
        _invertY.Enabled = true;
    }

    private void SetTileEditorsEnabled(bool enabled)
    {
        _captureX.Enabled = enabled;
        _captureY.Enabled = enabled;
        _captureR.Enabled = enabled;
    }

    private static decimal Clamp(NumericUpDown control, decimal value) =>
        Math.Max(control.Minimum, Math.Min(control.Maximum, value));
}
