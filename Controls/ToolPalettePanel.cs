using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public sealed class ToolPalettePanel : Panel
{
    private const int PaletteItemHeight = 78;

    private readonly ToolPaletteButton _select = CreateIconToggle(ToolIconFactory.Select, DrawTool.Select);
    private readonly ToolPaletteButton _move = CreateIconToggle(ToolIconFactory.Move, DrawTool.Move);
    private readonly ToolPaletteButton _frame = CreateIconToggle(ToolIconFactory.Frame, DrawTool.SetFrameOrigin);
    private readonly ToolPaletteButton _dot = CreateToggle("Dot", ToolIconFactory.Dot, DrawTool.Dot);
    private readonly ToolPaletteButton _line = CreateToggle("Line", ToolIconFactory.Line, DrawTool.Line);
    private readonly ToolPaletteButton _arc = CreateToggle("Arc", ToolIconFactory.Arc, DrawTool.Arc);
    private readonly ToolPaletteButton _rect = CreateToggle("Rect", ToolIconFactory.Rect, DrawTool.Rectangle);
    private readonly ToolPaletteButton _circle = CreateToggle("Circ", ToolIconFactory.Circle, DrawTool.Circle);
    private readonly ToolPaletteButton _snap = CreateIndependentToggle("Snp", ToolIconFactory.Snap);
    private readonly ToolPaletteButton _showGrid = CreateIconIndependentToggle(ToolIconFactory.Grid);
    private readonly ToolTip _tips = new()
    {
        InitialDelay = 400,
        ReshowDelay = 200,
        ShowAlways = true
    };
    private readonly ComboBox _gridStep = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        IntegralHeight = false,
        MaxDropDownItems = 12,
        Width = 96
    };
    private readonly ComboBox _width = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        IntegralHeight = false,
        MaxDropDownItems = 12,
        Width = 96
    };
    private readonly ComboBox _zDelta = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        IntegralHeight = false,
        MaxDropDownItems = 12,
        Width = 96
    };
    private readonly ToolPaletteButton _fit = CreateAction("Fit", ToolIconFactory.Fit);
    private readonly ToolPaletteButton _simPlay = CreateIconAction(ToolIconFactory.SimPlay);
    private readonly ToolPaletteButton _simPause = CreateIconAction(ToolIconFactory.SimPause);
    private readonly ToolPaletteButton _simStop = CreateIconAction(ToolIconFactory.SimStop);
    private readonly TrackBar _simSpeed = new()
    {
        Minimum = 10,
        Maximum = 150,
        Value = 100,
        TickFrequency = 14,
        Width = 112,
        Height = 28,
        AutoSize = false
    };
    private readonly Label _simSpeedCaption = new()
    {
        AutoSize = true,
        Text = "Speed 1.0x",
        Font = new Font("Segoe UI", 8f),
        ForeColor = Color.DimGray
    };

    private bool _gridUpdating;
    private bool _widthUpdating;
    private bool _zUpdating;

    public event EventHandler<DrawTool>? ToolSelected;
    public event EventHandler<bool>? SnapToPointChanged;
    public event EventHandler<bool>? ShowGridChanged;
    public event EventHandler<double>? GridStepChanged;
    public event EventHandler<double>? WidthChanged;
    public event EventHandler<double>? ZDeltaChanged;
    public event EventHandler? FitRequested;
    public event EventHandler? SimulationPlayRequested;
    public event EventHandler? SimulationPauseRequested;
    public event EventHandler? SimulationStopRequested;
    public event EventHandler<double>? SimulationSpeedMultiplierChanged;

    public ToolPalettePanel()
    {
        Dock = DockStyle.Top;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(10, 6, 10, 8);
        BackColor = Color.FromArgb(245, 245, 245);

        var root = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Dock = DockStyle.Top,
            Padding = new Padding(0)
        };

        root.Controls.Add(CreateGroup("Pointer", _select, _move));
        root.Controls.Add(CreateGroup("Frame", _frame));
        root.Controls.Add(CreateGroup("Draw", _dot, _line, _arc, _rect, _circle, _snap, CreateWidthHost(), CreateZHost()));
        root.Controls.Add(CreateGridGroup());
        root.Controls.Add(CreateGroup("View", _fit));
        root.Controls.Add(CreateSimulationGroup());

        Controls.Add(root);

        _select.Click += (_, _) => SelectTool(DrawTool.Select);
        _move.Click += (_, _) => SelectTool(DrawTool.Move);
        _frame.Click += (_, _) => SelectTool(DrawTool.SetFrameOrigin);
        _dot.Click += (_, _) => SelectTool(DrawTool.Dot);
        _line.Click += (_, _) => SelectTool(DrawTool.Line);
        _arc.Click += (_, _) => SelectTool(DrawTool.Arc);
        _rect.Click += (_, _) => SelectTool(DrawTool.Rectangle);
        _circle.Click += (_, _) => SelectTool(DrawTool.Circle);
        _snap.Click += (_, _) => SnapToPointChanged?.Invoke(this, _snap.IsChecked);
        _showGrid.Click += (_, _) =>
        {
            if (!_gridUpdating)
            {
                ShowGridChanged?.Invoke(this, _showGrid.IsChecked);
            }
        };
        _gridStep.SelectedIndexChanged += (_, _) =>
        {
            if (!_gridUpdating &&
                _gridStep.SelectedItem is string text &&
                GridSteps.TryParse(text, out double step))
            {
                GridStepChanged?.Invoke(this, step);
            }
        };
        _width.SelectedIndexChanged += (_, _) =>
        {
            if (!_widthUpdating &&
                _width.SelectedItem is string text &&
                StrokeWidths.TryParse(text, StrokeWidths.MinMm, out double width))
            {
                WidthChanged?.Invoke(this, width);
            }
        };
        _zDelta.SelectedIndexChanged += (_, _) =>
        {
            if (!_zUpdating &&
                _zDelta.SelectedItem is string text &&
                ZOffsets.TryParse(text, out double offset))
            {
                ZDeltaChanged?.Invoke(this, offset);
            }
        };
        _fit.Click += (_, _) => FitRequested?.Invoke(this, EventArgs.Empty);
        BindGrid(true, TeachGridSettings.DefaultStepsMm, 0.2);

        _simPlay.Click += (_, _) => SimulationPlayRequested?.Invoke(this, EventArgs.Empty);
        _simPause.Click += (_, _) => SimulationPauseRequested?.Invoke(this, EventArgs.Empty);
        _simStop.Click += (_, _) => SimulationStopRequested?.Invoke(this, EventArgs.Empty);
        _simSpeed.ValueChanged += (_, _) =>
        {
            double multiplier = _simSpeed.Value / 100.0;
            _simSpeedCaption.Text = $"Speed {multiplier:0.#}x";
            SimulationSpeedMultiplierChanged?.Invoke(this, multiplier);
        };
        SetToolTips();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tips.Dispose();
        }

        base.Dispose(disposing);
    }

    private void SetToolTips()
    {
        _tips.SetToolTip(_select, "Select. Pick a point or a shape.");
        _tips.SetToolTip(_move, "Move. Drag a point of a segment.");
        _tips.SetToolTip(_frame, "Frame. Set the frame origin on the canvas.");
        _tips.SetToolTip(_dot, "Dot. Place a single point.");
        _tips.SetToolTip(_line, "Line. Draw a straight segment from two points.");
        _tips.SetToolTip(_arc, "Arc. Draw an arc through three points.");
        _tips.SetToolTip(_rect, "Rectangle. Draw a rectangle from two opposite corners.");
        _tips.SetToolTip(_circle, "Circle. Draw a circle from the center and a radius point.");
        _tips.SetToolTip(_snap, "Snap. Snap the cursor to points and grid intersections.");
        _tips.SetToolTip(_width, "Width, mm. Stroke used for the next line or arc.");
        _tips.SetToolTip(_zDelta, "Z offset, mm. Added to project Z for the next segment. 0 uses project Z.");
        _tips.SetToolTip(_showGrid, "Grid. Show or hide the grid.");
        _tips.SetToolTip(_gridStep, "Grid step, mm. Choose the distance between grid points.");
        _tips.SetToolTip(_fit, "Fit. Fit the whole path in the view.");
        _tips.SetToolTip(_simPlay, "Play. Run the dispense simulation.");
        _tips.SetToolTip(_simPause, "Pause. Pause the dispense simulation.");
        _tips.SetToolTip(_simStop, "Stop. Stop the dispense simulation.");
        _tips.SetToolTip(_simSpeed, "Speed. Simulation playback speed.");
    }

    public void BindGrid(bool show, IReadOnlyList<double> steps, double selected)
    {
        _gridUpdating = true;
        try
        {
            _showGrid.IsChecked = show;
            List<double> values = GridSteps.NormalizeList(steps, selected, out double current);
            _gridStep.Items.Clear();
            foreach (double value in values)
            {
                _gridStep.Items.Add(GridSteps.Format(value));
            }

            _gridStep.SelectedItem = GridSteps.Format(current);
        }
        finally
        {
            _gridUpdating = false;
        }
    }

    public void BindWidths(IReadOnlyList<double> steps, double selected)
    {
        _widthUpdating = true;
        try
        {
            _width.Items.Clear();
            foreach (double value in steps)
            {
                _width.Items.Add(StrokeWidths.Format(value));
            }

            _width.SelectedItem = StrokeWidths.Format(selected);
        }
        finally
        {
            _widthUpdating = false;
        }
    }

    public void BindZDeltas(IReadOnlyList<double> offsets, double selected)
    {
        _zUpdating = true;
        try
        {
            _zDelta.Items.Clear();
            foreach (double value in offsets)
            {
                _zDelta.Items.Add(ZOffsets.Format(value));
            }

            _zDelta.SelectedItem = ZOffsets.Format(selected);
        }
        finally
        {
            _zUpdating = false;
        }
    }

    public void SetShowGrid(bool show)
    {
        if (_showGrid.IsChecked == show)
        {
            return;
        }

        _gridUpdating = true;
        try
        {
            _showGrid.IsChecked = show;
        }
        finally
        {
            _gridUpdating = false;
        }
    }

    public void SetSnapEnabled(bool enabled)
    {
        if (_snap.IsChecked != enabled)
        {
            _snap.IsChecked = enabled;
        }

        SnapToPointChanged?.Invoke(this, enabled);
    }

    public void SetActiveTool(DrawTool tool)
    {
        _select.IsChecked = tool == DrawTool.Select;
        _move.IsChecked = tool == DrawTool.Move;
        _frame.IsChecked = tool == DrawTool.SetFrameOrigin;
        _dot.IsChecked = tool == DrawTool.Dot;
        _line.IsChecked = tool == DrawTool.Line;
        _arc.IsChecked = tool == DrawTool.Arc;
        _rect.IsChecked = tool == DrawTool.Rectangle;
        _circle.IsChecked = tool == DrawTool.Circle;
    }

    public void SetSimulationState(SimulationUiState state)
    {
        _simPlay.Enabled = state.CanPlay;
        _simPause.Enabled = state.CanPause;
        _simStop.Enabled = state.CanStop;
    }

    private GroupBox CreateGridGroup()
    {
        var stepHost = new Panel
        {
            Width = 108,
            Height = PaletteItemHeight,
            MinimumSize = new Size(108, PaletteItemHeight),
            MaximumSize = new Size(108, PaletteItemHeight),
            Margin = new Padding(4, 2, 4, 2)
        };
        var caption = new Label
        {
            AutoSize = true,
            Text = "Step mm",
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.DimGray,
            Location = new Point(4, 10)
        };
        _gridStep.Location = new Point(4, 34);
        stepHost.Controls.Add(caption);
        stepHost.Controls.Add(_gridStep);
        return CreateGroup("Grid", _showGrid, stepHost);
    }

    private Control CreateWidthHost()
    {
        var host = new Panel
        {
            Width = 108,
            Height = PaletteItemHeight,
            MinimumSize = new Size(108, PaletteItemHeight),
            MaximumSize = new Size(108, PaletteItemHeight),
            Margin = new Padding(4, 2, 4, 2)
        };
        var caption = new Label
        {
            AutoSize = true,
            Text = "Width mm",
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.DimGray,
            Location = new Point(4, 10)
        };
        _width.Location = new Point(4, 34);
        host.Controls.Add(caption);
        host.Controls.Add(_width);
        return host;
    }

    private Control CreateZHost()
    {
        var host = new Panel
        {
            Width = 108,
            Height = PaletteItemHeight,
            MinimumSize = new Size(108, PaletteItemHeight),
            MaximumSize = new Size(108, PaletteItemHeight),
            Margin = new Padding(4, 2, 4, 2)
        };
        var caption = new Label
        {
            AutoSize = true,
            Text = "Z mm",
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.DimGray,
            Location = new Point(4, 10)
        };
        _zDelta.Location = new Point(4, 34);
        host.Controls.Add(caption);
        host.Controls.Add(_zDelta);
        return host;
    }

    private GroupBox CreateSimulationGroup()
    {
        var speedPanel = new Panel
        {
            Width = 120,
            Height = PaletteItemHeight,
            MinimumSize = new Size(120, PaletteItemHeight),
            MaximumSize = new Size(120, PaletteItemHeight),
            Margin = new Padding(4, 2, 4, 2)
        };
        _simSpeedCaption.Location = new Point(0, 10);
        _simSpeed.Location = new Point(4, 34);
        speedPanel.Controls.Add(_simSpeedCaption);
        speedPanel.Controls.Add(_simSpeed);

        return CreateGroup("Dispense Simulation", _simPlay, _simPause, _simStop, speedPanel);
    }

    private void SelectTool(DrawTool tool)
    {
        SetActiveTool(tool);
        ToolSelected?.Invoke(this, tool);
    }

    private static ToolPaletteButton CreateIconToggle(Image icon, DrawTool tool)
    {
        var button = new ToolPaletteButton
        {
            Image = icon,
            Tag = tool
        };
        button.ConfigureIconOnly();
        return button;
    }

    private static ToolPaletteButton CreateToggle(string text, Image icon, DrawTool tool)
    {
        return new ToolPaletteButton
        {
            Text = text,
            Image = icon,
            Tag = tool
        };
    }

    private static ToolPaletteButton CreateAction(string text, Image icon)
    {
        return new ToolPaletteButton
        {
            Text = text,
            Image = icon
        };
    }

    private static ToolPaletteButton CreateIconAction(Image icon)
    {
        var button = new ToolPaletteButton { Image = icon };
        button.ConfigureIconOnly();
        return button;
    }

    private static ToolPaletteButton CreateIconIndependentToggle(Image icon)
    {
        var button = new ToolPaletteButton
        {
            Image = icon,
            IndependentToggle = true
        };
        button.ConfigureIconOnly();
        return button;
    }

    private static ToolPaletteButton CreateIndependentToggle(string text, Image icon)
    {
        return new ToolPaletteButton
        {
            Text = text,
            Image = icon,
            IndependentToggle = true
        };
    }

    private static GroupBox CreateGroup(string title, params Control[] buttons)
    {
        var group = new GroupBox
        {
            Text = title,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(10, 8, 10, 10),
            Margin = new Padding(0, 0, 12, 0),
            Font = new Font("Segoe UI", 9f, FontStyle.Regular)
        };

        var flow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
            MinimumSize = new Size(0, PaletteItemHeight + 4)
        };

        flow.Controls.AddRange(buttons);
        group.Controls.Add(flow);
        return group;
    }
}

public readonly struct SimulationUiState
{
    public bool CanPlay { get; init; }
    public bool CanPause { get; init; }
    public bool CanStop { get; init; }
}
