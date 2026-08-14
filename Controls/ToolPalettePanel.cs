namespace FanucPathAuthor.Controls;

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

    public event EventHandler<DrawTool>? ToolSelected;
    public event EventHandler<bool>? SnapToPointChanged;
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
        root.Controls.Add(CreateGroup("Draw", _dot, _line, _arc, _rect, _circle, _snap));
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
        _fit.Click += (_, _) => FitRequested?.Invoke(this, EventArgs.Empty);

        _simPlay.Click += (_, _) => SimulationPlayRequested?.Invoke(this, EventArgs.Empty);
        _simPause.Click += (_, _) => SimulationPauseRequested?.Invoke(this, EventArgs.Empty);
        _simStop.Click += (_, _) => SimulationStopRequested?.Invoke(this, EventArgs.Empty);
        _simSpeed.ValueChanged += (_, _) =>
        {
            double multiplier = _simSpeed.Value / 100.0;
            _simSpeedCaption.Text = $"Speed {multiplier:0.#}x";
            SimulationSpeedMultiplierChanged?.Invoke(this, multiplier);
        };
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
