using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public sealed class StrokeWidthsDialog : Form
{
    private readonly double _floor;
    private readonly List<double> _widths;
    private readonly ListBox _list = new()
    {
        IntegralHeight = false,
        Dock = DockStyle.Fill
    };
    private readonly Button _deleteButton = new()
    {
        Text = "Delete",
        AutoSize = true,
        MinimumSize = new Size(96, 32)
    };
    private readonly Button _editButton = new()
    {
        Text = "Edit",
        AutoSize = true,
        MinimumSize = new Size(96, 32)
    };

    public StrokeWidthsDialog(IReadOnlyList<double> widths, double minimum)
    {
        _floor = StrokeWidths.Floor(minimum);
        _widths = StrokeWidths.StoredList(widths, _floor);

        Text = "Stroke widths";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(380, 420);
        Padding = new Padding(12);

        var caption = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Text = $"Minimum: {StrokeWidths.Format(_floor)} mm"
        };
        var addButton = new Button
        {
            Text = "Add",
            AutoSize = true,
            MinimumSize = new Size(96, 32)
        };
        var ok = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            AutoSize = true,
            MinimumSize = new Size(96, 32)
        };
        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            AutoSize = true,
            MinimumSize = new Size(96, 32)
        };
        var commands = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };
        commands.Controls.Add(addButton);
        commands.Controls.Add(_deleteButton);
        commands.Controls.Add(_editButton);
        var close = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };
        close.Controls.Add(ok);
        close.Controls.Add(cancel);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(caption, 0, 0);
        layout.Controls.Add(_list, 0, 1);
        layout.Controls.Add(commands, 0, 2);
        layout.Controls.Add(close, 0, 3);
        Controls.Add(layout);

        AcceptButton = ok;
        CancelButton = cancel;
        addButton.Click += (_, _) => AddWidth();
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
        using var dialog = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            AutoScaleMode = AutoScaleMode.Font,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Font = Font,
            Padding = new Padding(20, 16, 20, 16)
        };
        var label = new Label
        {
            Text = "Width mm",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        var input = new NumericUpDown
        {
            DecimalPlaces = 3,
            Increment = 0.1m,
            Minimum = (decimal)_floor,
            Maximum = (decimal)StrokeWidths.MaxMm,
            ThousandsSeparator = false,
            TextAlign = HorizontalAlignment.Left,
            Dock = DockStyle.Top,
            Width = 280,
            Margin = new Padding(0, 0, 0, 16)
        };
        decimal initial = Math.Round((decimal)current, 3, MidpointRounding.AwayFromZero);
        input.Value = Math.Min(input.Maximum, Math.Max(input.Minimum, initial));
        var ok = new Button
        {
            Text = acceptText,
            DialogResult = DialogResult.OK,
            AutoSize = true,
            MinimumSize = new Size(96, 32),
            Margin = new Padding(8, 0, 0, 0)
        };
        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            AutoSize = true,
            MinimumSize = new Size(96, 32)
        };
        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            MinimumSize = new Size(300, 0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(label, 0, 0);
        layout.Controls.Add(input, 0, 1);
        layout.Controls.Add(buttons, 0, 2);
        dialog.Controls.Add(layout);
        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;
        dialog.Shown += (_, _) =>
        {
            input.Focus();
            input.Select(0, input.Text.Length);
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        width = StrokeWidths.Normalize((double)input.Value);
        return width >= _floor - 0.0005;
    }
}
