using DispensePathAuthor.Models;

namespace DispensePathAuthor.Controls;

public partial class TeachProjectPanel : UserControl
{
    private bool _updating;

    public TeachProjectPanel()
    {
        InitializeComponent();
        _importButton.Click += (_, _) => ImportRequested?.Invoke(this, EventArgs.Empty);
        _removeButton.Click += (_, _) => RemoveRequested?.Invoke(this, EventArgs.Empty);
        _alignButton.Click += (_, _) => AlignRequested?.Invoke(this, EventArgs.Empty);
        _gridEnabled.CheckedChanged += (_, _) =>
        {
            if (!_updating)
            {
                GridEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        };
        _gridStep.ValueChanged += (_, _) =>
        {
            if (!_updating)
            {
                GridStepChanged?.Invoke(this, EventArgs.Empty);
            }
        };
        _pointList.MouseUp += OnPointListMouseUp;
    }

    public event EventHandler? ImportRequested;

    public event EventHandler? RemoveRequested;

    public event EventHandler? AlignRequested;

    public event EventHandler? GridEnabledChanged;

    public event EventHandler? GridStepChanged;

    public string? SelectedPointId =>
        _pointList.SelectedItems.Count == 0 ? null : _pointList.SelectedItems[0].Tag as string;

    public bool GridEnabled => _gridEnabled.Checked;

    public double GridStepMillimeters => (double)_gridStep.Value;

    public void Show(PathDocument document)
    {
        string? selectedId = SelectedPointId;
        _updating = true;
        try
        {
            _pointList.Items.Clear();
            foreach (ReferencePoint point in document.ReferencePoints)
            {
                var item = new ListViewItem(point.Name);
                item.SubItems.Add(point.X_mm.ToString("0.000"));
                item.SubItems.Add(point.Y_mm.ToString("0.000"));
                item.Tag = point.Id;
                _pointList.Items.Add(item);
            }

            if (!string.IsNullOrWhiteSpace(selectedId))
            {
                foreach (ListViewItem item in _pointList.Items)
                {
                    if (item.Tag is string id && id == selectedId)
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }

            _gridEnabled.Checked = document.Grid.Enabled;
            _gridStep.Value = Math.Max(_gridStep.Minimum, Math.Min(_gridStep.Maximum, (decimal)document.Grid.Step_mm));
            _originLabel.Text = document.Grid.IsAligned
                ? $"Grid origin: ({document.Grid.OriginX_mm:0.###}, {document.Grid.OriginY_mm:0.###}) mm"
                : "Grid origin: robot (0, 0) until a point is aligned";
        }
        finally
        {
            _updating = false;
        }

        _removeButton.Enabled = document.ReferencePoints.Count > 0 && SelectedPointId != null;
        _alignButton.Enabled = document.ReferencePoints.Count > 0;
    }

    private void OnPointListMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        ListViewItem? item = _pointList.GetItemAt(e.X, e.Y);
        if (item == null)
        {
            return;
        }

        item.Selected = true;
        var menu = new ContextMenuStrip();
        menu.Items.Add("Align grid to this point", null, (_, _) => AlignRequested?.Invoke(this, EventArgs.Empty));
        menu.Closed += (_, _) => menu.Dispose();
        menu.Show(_pointList, e.Location);
    }
}
