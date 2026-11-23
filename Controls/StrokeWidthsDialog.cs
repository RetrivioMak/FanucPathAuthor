using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public sealed partial class StrokeWidthsDialog : Form
{
    private readonly double _floor;
    private readonly List<double> _widths;

    public StrokeWidthsDialog(IReadOnlyList<double> widths, double minimum)
    {
        InitializeComponent();
        _floor = StrokeWidths.Floor(minimum);
        _widths = StrokeWidths.StoredList(widths, _floor);
        _caption.Text = $"Minimum: {StrokeWidths.Format(_floor)} mm";

        _addButton.Click += (_, _) => AddWidth();
        _deleteButton.Click += (_, _) => DeleteWidth();
        _editButton.Click += (_, _) => EditWidth();
        _list.SelectedIndexChanged += (_, _) => UpdateButtons();
        _list.DoubleClick += (_, _) => EditWidth();
        BindList(0);
    }

    public IReadOnlyList<double> Steps => _widths;

    private void AddWidth()
    {
        double suggestion = _list.SelectedIndex >= 0 ? _widths[_list.SelectedIndex] : _floor;
        if (!TryAsk("Add width", "Add", suggestion, out double width))
        {
            return;
        }

        int existing = IndexOf(width);
        if (existing >= 0)
        {
            BindList(existing);
            return;
        }

        int insertAt = _widths.FindIndex(value => value > width);
        if (insertAt < 0)
        {
            insertAt = _widths.Count;
        }

        _widths.Insert(insertAt, width);
        BindList(insertAt);
    }

    private void DeleteWidth()
    {
        int index = _list.SelectedIndex;
        if (index < 0 || IsFloor(index) || _widths.Count <= 1)
        {
            return;
        }

        _widths.RemoveAt(index);
        BindList(Math.Min(index, _widths.Count - 1));
    }

    private void EditWidth()
    {
        int index = _list.SelectedIndex;
        if (index < 0 || IsFloor(index))
        {
            return;
        }

        if (!TryAsk("Edit width", "OK", _widths[index], out double width))
        {
            return;
        }

        if (StrokeWidths.NearlyEqual(_widths[index], width))
        {
            return;
        }

        _widths.RemoveAt(index);
        int existing = IndexOf(width);
        if (existing >= 0)
        {
            BindList(existing);
            return;
        }

        int insertAt = _widths.FindIndex(value => value > width);
        if (insertAt < 0)
        {
            insertAt = _widths.Count;
        }

        _widths.Insert(insertAt, width);
        BindList(insertAt);
    }

    private void BindList(int selectedIndex)
    {
        _list.BeginUpdate();
        try
        {
            _list.Items.Clear();
            foreach (double width in _widths)
            {
                _list.Items.Add(StrokeWidths.Format(width));
            }
        }
        finally
        {
            _list.EndUpdate();
        }

        if (_widths.Count > 0)
        {
            _list.SelectedIndex = Math.Clamp(selectedIndex, 0, _widths.Count - 1);
        }

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        int index = _list.SelectedIndex;
        bool hasSelection = index >= 0;
        bool locked = !hasSelection || IsFloor(index);
        _editButton.Enabled = hasSelection && !locked;
        _deleteButton.Enabled = hasSelection && !locked && _widths.Count > 1;
    }

    private bool IsFloor(int index) =>
        index >= 0 && index < _widths.Count && StrokeWidths.NearlyEqual(_widths[index], _floor);

    private int IndexOf(double width)
    {
        for (int i = 0; i < _widths.Count; i++)
        {
            if (StrokeWidths.NearlyEqual(_widths[i], width))
            {
                return i;
            }
        }

        return -1;
    }

    private bool TryAsk(string title, string acceptText, double current, out double width)
    {
        width = current;
        decimal initial = Math.Round((decimal)current, 3, MidpointRounding.AwayFromZero);
        using var dialog = new ValuePromptDialog
        {
            Text = title,
            AcceptText = acceptText,
            Caption = "Width mm",
            Minimum = (decimal)_floor,
            Maximum = (decimal)StrokeWidths.MaxMm,
            Font = Font
        };
        dialog.Value = initial;
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        width = StrokeWidths.Normalize((double)dialog.Value);
        return width >= _floor - 0.0005;
    }
}
