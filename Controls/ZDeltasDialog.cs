using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public sealed partial class ZDeltasDialog : Form
{
    private readonly List<double> _offsets;

    public ZDeltasDialog(IReadOnlyList<double> offsets)
    {
        InitializeComponent();
        Icon = AppIcon.Load();
        _offsets = ZOffsets.StoredList(offsets);
        _caption.Text = "Offset added to project Z. 0 keeps the project value.";

        _addButton.Click += (_, _) => AddOffset();
        _deleteButton.Click += (_, _) => DeleteOffset();
        _editButton.Click += (_, _) => EditOffset();
        _list.SelectedIndexChanged += (_, _) => UpdateButtons();
        _list.DoubleClick += (_, _) => EditOffset();
        BindList(IndexOf(0));
    }

    public IReadOnlyList<double> Steps => _offsets;

    private void AddOffset()
    {
        double suggestion = _list.SelectedIndex >= 0 ? _offsets[_list.SelectedIndex] : 0;
        if (!TryAsk("Add Z offset", "Add", suggestion, out double offset))
        {
            return;
        }

        int existing = IndexOf(offset);
        if (existing >= 0)
        {
            BindList(existing);
            return;
        }

        int insertAt = _offsets.FindIndex(value => value > offset);
        if (insertAt < 0)
        {
            insertAt = _offsets.Count;
        }

        _offsets.Insert(insertAt, offset);
        BindList(insertAt);
    }

    private void DeleteOffset()
    {
        int index = _list.SelectedIndex;
        if (index < 0 || IsZero(index) || _offsets.Count <= 1)
        {
            return;
        }

        _offsets.RemoveAt(index);
        BindList(Math.Min(index, _offsets.Count - 1));
    }

    private void EditOffset()
    {
        int index = _list.SelectedIndex;
        if (index < 0 || IsZero(index))
        {
            return;
        }

        if (!TryAsk("Edit Z offset", "OK", _offsets[index], out double offset))
        {
            return;
        }

        if (ZOffsets.NearlyEqual(_offsets[index], offset))
        {
            return;
        }

        _offsets.RemoveAt(index);
        int existing = IndexOf(offset);
        if (existing >= 0)
        {
            BindList(existing);
            return;
        }

        int insertAt = _offsets.FindIndex(value => value > offset);
        if (insertAt < 0)
        {
            insertAt = _offsets.Count;
        }

        _offsets.Insert(insertAt, offset);
        BindList(insertAt);
    }

    private void BindList(int selectedIndex)
    {
        _list.BeginUpdate();
        try
        {
            _list.Items.Clear();
            foreach (double offset in _offsets)
            {
                _list.Items.Add(ZOffsets.Format(offset));
            }
        }
        finally
        {
            _list.EndUpdate();
        }

        if (_offsets.Count > 0)
        {
            _list.SelectedIndex = Math.Clamp(selectedIndex, 0, _offsets.Count - 1);
        }

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        int index = _list.SelectedIndex;
        bool hasSelection = index >= 0;
        bool locked = !hasSelection || IsZero(index);
        _editButton.Enabled = hasSelection && !locked;
        _deleteButton.Enabled = hasSelection && !locked && _offsets.Count > 1;
    }

    private bool IsZero(int index) =>
        index >= 0 && index < _offsets.Count && ZOffsets.NearlyEqual(_offsets[index], 0);

    private int IndexOf(double offset)
    {
        for (int i = 0; i < _offsets.Count; i++)
        {
            if (ZOffsets.NearlyEqual(_offsets[i], offset))
            {
                return i;
            }
        }

        return -1;
    }

    private bool TryAsk(string title, string acceptText, double current, out double offset)
    {
        offset = current;
        decimal initial = Math.Round((decimal)current, 3, MidpointRounding.AwayFromZero);
        using var dialog = new ValuePromptDialog
        {
            Text = title,
            AcceptText = acceptText,
            Caption = "Z offset mm",
            Minimum = (decimal)ZOffsets.MinMm,
            Maximum = (decimal)ZOffsets.MaxMm,
            Font = Font
        };
        dialog.Value = initial;
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        offset = ZOffsets.Normalize((double)dialog.Value);
        return offset >= ZOffsets.MinMm - 0.0005 && offset <= ZOffsets.MaxMm + 0.0005;
    }
}
