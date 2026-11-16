using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor;

public sealed partial class MainForm
{
    private readonly ListView _teachPointList = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        HideSelection = false
    };
    private readonly CheckBox _gridEnabled = new()
    {
        Text = "Snap to grid",
        AutoSize = true,
        Checked = true
    };
    private readonly NumericUpDown _gridStep = CreateNumeric(0.001m, 1000m, 0.1m, 3);
    private readonly Label _gridOriginLabel = new() { AutoSize = true, Text = "Grid origin: (0, 0) mm" };
    private readonly Label _teachHint = new()
    {
        AutoSize = true,
        MaximumSize = new Size(430, 0),
        ForeColor = Color.DimGray,
        Text = "Import teach points to start a Teach project. Coordinates are stored in millimeters."
    };
    private Button? _teachImportButton;
    private Button? _teachRemoveButton;
    private Button? _teachAlignButton;

    private TabPage BuildTeachTab()
    {
        _gridStep.Value = 0.2m;
        _teachPointList.Columns.Add("Name", 120);
        _teachPointList.Columns.Add("X mm", 90);
        _teachPointList.Columns.Add("Y mm", 90);

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 6)
        };
        _teachImportButton = new Button { Text = "Import points...", AutoSize = true };
        _teachRemoveButton = new Button { Text = "Remove", AutoSize = true };
        _teachAlignButton = new Button { Text = "Align grid", AutoSize = true };
        _teachImportButton.Click += (_, _) => ImportTeachPoints();
        _teachRemoveButton.Click += (_, _) => RemoveSelectedTeachPoint();
        _teachAlignButton.Click += (_, _) => AlignGridToSelectedTeachPoint();
        buttons.Controls.Add(_teachImportButton);
        buttons.Controls.Add(_teachRemoveButton);
        buttons.Controls.Add(_teachAlignButton);

        var gridLayout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Location = new Point(8, 18),
            Padding = new Padding(0, 4, 0, 0)
        };
        gridLayout.Controls.Add(_gridEnabled, 0, 0);
        gridLayout.SetColumnSpan(_gridEnabled, 2);
        gridLayout.Controls.Add(new Label { Text = "Step mm", AutoSize = true }, 0, 1);
        gridLayout.Controls.Add(_gridStep, 1, 1);
        gridLayout.Controls.Add(_gridOriginLabel, 0, 2);
        gridLayout.SetColumnSpan(_gridOriginLabel, 2);

        var gridGroup = new GroupBox
        {
            Text = "Grid",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8),
            Margin = new Padding(0)
        };
        gridGroup.Controls.Add(gridLayout);

        _teachHint.Dock = DockStyle.Fill;
        _teachHint.Margin = new Padding(0, 0, 0, 6);

        var stack = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Padding = new Padding(0, 8, 0, 0)
        };
        stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.Controls.Add(buttons, 0, 0);
        stack.Controls.Add(_teachHint, 0, 1);
        stack.Controls.Add(gridGroup, 0, 2);
        stack.Layout += (_, _) =>
        {
            int width = Math.Max(160, stack.ClientSize.Width - stack.Padding.Horizontal);
            if (_teachHint.MaximumSize.Width != width)
            {
                _teachHint.MaximumSize = new Size(width, 0);
            }

            if (buttons.MaximumSize.Width != width)
            {
                buttons.MaximumSize = new Size(width, 0);
            }
        };

        _gridEnabled.CheckedChanged += (_, _) =>
        {
            if (_updatingUi)
            {
                return;
            }

            _canvas.SetGridEnabled(_gridEnabled.Checked);
        };
        _gridStep.ValueChanged += (_, _) =>
        {
            if (_updatingUi)
            {
                return;
            }

            _canvas.SetGridStep((double)_gridStep.Value);
        };
        _teachPointList.MouseUp += OnTeachPointListMouseUp;

        var page = new TabPage("Teach Points") { Padding = new Padding(4) };
        page.Controls.Add(_teachPointList);
        page.Controls.Add(stack);
        return page;
    }

    private void OnTeachPointListMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        ListViewItem? item = _teachPointList.GetItemAt(e.X, e.Y);
        if (item == null)
        {
            return;
        }

        item.Selected = true;
        var menu = new ContextMenuStrip();
        menu.Items.Add("Align grid to this point", null, (_, _) => AlignGridToSelectedTeachPoint());
        menu.Closed += (_, _) => menu.Dispose();
        menu.Show(_teachPointList, e.Location);
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

        _canvas.AddReferencePoints(dialog.Points);
    }

    private void RemoveSelectedTeachPoint()
    {
        if (_teachPointList.SelectedItems.Count == 0 || _teachPointList.SelectedItems[0].Tag is not string id)
        {
            return;
        }

        _canvas.RemoveReferencePoint(id);
    }

    private void AlignGridToSelectedTeachPoint()
    {
        ReferencePoint? point = SelectedTeachPoint();
        if (point == null)
        {
            return;
        }

        _canvas.AlignGridTo(point.X_mm, point.Y_mm);
    }

    private ReferencePoint? SelectedTeachPoint()
    {
        if (_teachPointList.SelectedItems.Count == 0 || _teachPointList.SelectedItems[0].Tag is not string id)
        {
            return null;
        }

        return _canvas.Document.ReferencePoints.FirstOrDefault(point => point.Id == id);
    }

    private void RefreshTeachPointList()
    {
        string? selectedId = _teachPointList.SelectedItems.Count > 0
            ? _teachPointList.SelectedItems[0].Tag as string
            : null;
        PathDocument document = _canvas.Document;
        _teachPointList.Items.Clear();
        foreach (ReferencePoint point in document.ReferencePoints)
        {
            var item = new ListViewItem(point.Name);
            item.SubItems.Add(point.X_mm.ToString("0.000"));
            item.SubItems.Add(point.Y_mm.ToString("0.000"));
            item.Tag = point.Id;
            _teachPointList.Items.Add(item);
        }

        if (!string.IsNullOrWhiteSpace(selectedId))
        {
            foreach (ListViewItem item in _teachPointList.Items)
            {
                if (item.Tag is string id && id == selectedId)
                {
                    item.Selected = true;
                    break;
                }
            }
        }

        _updatingUi = true;
        try
        {
            _gridEnabled.Checked = document.Grid.Enabled;
            _gridStep.Value = Clamp(_gridStep, (decimal)document.Grid.Step_mm);
            _gridOriginLabel.Text = document.Grid.IsAligned
                ? $"Grid origin: ({document.Grid.OriginX_mm:0.###}, {document.Grid.OriginY_mm:0.###}) mm"
                : "Grid origin: robot (0, 0) until a point is aligned";
        }
        finally
        {
            _updatingUi = false;
        }

        bool teachBlocked = document.Workspace == WorkspaceKind.Cv;
        bool teachActive = document.Workspace == WorkspaceKind.Teach;
        if (_teachImportButton != null)
        {
            _teachImportButton.Enabled = !teachBlocked;
        }

        if (_teachRemoveButton != null)
        {
            _teachRemoveButton.Enabled = teachActive;
        }

        if (_teachAlignButton != null)
        {
            _teachAlignButton.Enabled = teachActive && document.ReferencePoints.Count > 0;
        }

        _gridEnabled.Enabled = teachActive;
        _gridStep.Enabled = teachActive;
        _teachHint.Text = teachBlocked
            ? "This project is CV. Start a new project to add teach points."
            : teachActive
                ? "Right-click a point to align the grid. With the grid on, new points snap to it. A reference or vertex still wins when the cursor is close."
                : "Import teach points to start a Teach project. Coordinates are stored in millimeters.";
    }

    private bool EnsureWorkspace(WorkspaceKind kind)
    {
        if (WorkspaceGuard.CanAdopt(_canvas.Document.Workspace, kind))
        {
            return true;
        }

        MessageBox.Show(
            this,
            WorkspaceGuard.RefusalMessage(_canvas.Document.Workspace),
            "Project type",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        return false;
    }

    private void NewProject(object? sender, EventArgs e)
    {
        PathDocument document = _canvas.Document;
        bool hasContent = document.Workspace != WorkspaceKind.Unset ||
                          document.Segments.Count > 0 ||
                          document.Tiles.Count > 0 ||
                          document.ReferencePoints.Count > 0;
        if (hasContent &&
            MessageBox.Show(
                this,
                "Start a new project? The current path stays on disk until you save this one.",
                "New project",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        CancelFrameEdit();
        _canvas.ResetProject();
        _calibration = null;
        _calibrationPath = null;
        _currentPathFile = null;
        _calibrationLabel.Text = "Calibration: not loaded";
        _statusLabel.Text = "New project. Open an FOV image or add teach points.";
    }
}
