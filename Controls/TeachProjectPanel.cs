using DispensePathAuthor.Models;

namespace DispensePathAuthor.Controls;

public partial class TeachProjectPanel : UserControl
{
    public TeachProjectPanel()
    {
        InitializeComponent();
        _importButton.Click += (_, _) => ImportRequested?.Invoke(this, EventArgs.Empty);
        _removeButton.Click += (_, _) => RemoveRequested?.Invoke(this, EventArgs.Empty);
        _alignButton.Click += (_, _) => AlignRequested?.Invoke(this, EventArgs.Empty);
        _pointList.MouseUp += OnPointListMouseUp;
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        int textWidth = Math.Max(140, ClientSize.Width - Padding.Horizontal - 36);
        var limit = new Size(textWidth, 0);
        if (_hint.MaximumSize != limit)
        {
            _hint.MaximumSize = limit;
        }

        base.OnLayout(e);
    }

    public event EventHandler? ImportRequested;

    public event EventHandler? RemoveRequested;

    public event EventHandler? AlignRequested;

    public string? SelectedPointId =>
        _pointList.SelectedItems.Count == 0 ? null : _pointList.SelectedItems[0].Tag as string;

    public void Show(PathDocument document)
    {
        string? selectedId = SelectedPointId;
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
        menu.Items.Add("Set as grid origin", null, (_, _) => AlignRequested?.Invoke(this, EventArgs.Empty));
        menu.Closed += (_, _) =>
        {
            if (IsHandleCreated && !IsDisposed)
            {
                BeginInvoke(() =>
                {
                    if (!menu.IsDisposed)
                    {
                        menu.Dispose();
                    }
                });
            }
        };
        menu.Show(_pointList, e.Location);
    }
}
