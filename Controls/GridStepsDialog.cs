using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public sealed class GridStepsDialog : Form
{
    private readonly List<double> _steps;
    private readonly ListBox _list = new()
    {
        IntegralHeight = false,
        Dock = DockStyle.Fill
    };
    private readonly Label _activeLabel = new()
    {
        AutoSize = true,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleLeft
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
    private int _activeIndex;

    public GridStepsDialog(IReadOnlyList<double> steps, double activeStep)
    {
        _steps = GridSteps.NormalizeList(steps, activeStep, out double current);
        _activeIndex = IndexOf(current);
        if (_activeIndex < 0)
        {
            _activeIndex = 0;
        }

        Text = "Grid steps";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(380, 420);
        Padding = new Padding(12);

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
        layout.Controls.Add(_activeLabel, 0, 0);
        layout.Controls.Add(_list, 0, 1);
        layout.Controls.Add(commands, 0, 2);
        layout.Controls.Add(close, 0, 3);
        Controls.Add(layout);

        AcceptButton = ok;
        CancelButton = cancel;
        addButton.Click += (_, _) => AddStep();
        _deleteButton.Click += (_, _) => DeleteStep();
        _editButton.Click += (_, _) => EditStep();
        _list.SelectedIndexChanged += (_, _) => UpdateButtons();
        _list.DoubleClick += (_, _) => EditStep();
        BindList(_activeIndex);
    }

    public double SelectedStep => _steps[Math.Clamp(_activeIndex, 0, _steps.Count - 1)];

    public IReadOnlyList<double> Steps => _steps;

    private void AddStep()
    {
        double suggestion = _list.SelectedIndex >= 0 ? _steps[_list.SelectedIndex] : SelectedStep;
        if (!TryAsk("Add grid step", "Add", suggestion, out double step))
        {
            return;
        }

        int existing = IndexOf(step);
        if (existing >= 0)
        {
            BindList(existing);
            return;
        }

        int insertAt = _steps.FindIndex(value => value > step);
        if (insertAt < 0)
        {
            insertAt = _steps.Count;
        }

        _steps.Insert(insertAt, step);
        if (insertAt <= _activeIndex)
        {
            _activeIndex++;
        }

        BindList(insertAt);
    }

    private void DeleteStep()
    {
        int index = _list.SelectedIndex;
        if (index < 0 || _steps.Count <= 1)
        {
            return;
        }

        _steps.RemoveAt(index);
        if (index < _activeIndex)
        {
            _activeIndex--;
        }
        else if (index == _activeIndex)
        {
            _activeIndex = Math.Min(index, _steps.Count - 1);
        }

        BindList(Math.Min(index, _steps.Count - 1));
    }

    private void EditStep()
    {
        int index = _list.SelectedIndex;
        if (index < 0)
        {
            return;
        }

        if (!TryAsk("Edit grid step", "OK", _steps[index], out double step))
        {
            return;
        }

        if (GridSteps.NearlyEqual(_steps[index], step))
        {
            return;
        }

        bool activeEdited = index == _activeIndex;
        _steps.RemoveAt(index);
        if (index < _activeIndex)
        {
            _activeIndex--;
        }

        int existing = IndexOf(step);
        int select;
        if (existing >= 0)
        {
            if (activeEdited)
            {
                _activeIndex = existing;
            }

            select = existing;
        }
        else
        {
            int insertAt = _steps.FindIndex(value => value > step);
            if (insertAt < 0)
            {
                insertAt = _steps.Count;
            }

            _steps.Insert(insertAt, step);
            if (activeEdited)
            {
                _activeIndex = insertAt;
            }
            else if (insertAt <= _activeIndex)
            {
                _activeIndex++;
            }

            select = insertAt;
        }

        BindList(select);
    }

    private void BindList(int selectedIndex)
    {
        _list.BeginUpdate();
        try
        {
            _list.Items.Clear();
            foreach (double step in _steps)
            {
                _list.Items.Add(GridSteps.Format(step));
            }
        }
        finally
        {
            _list.EndUpdate();
        }

        if (_steps.Count > 0)
        {
            _list.SelectedIndex = Math.Clamp(selectedIndex, 0, _steps.Count - 1);
        }

        _activeLabel.Text = $"Active step: {GridSteps.Format(SelectedStep)} mm";
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool hasSelection = _list.SelectedIndex >= 0;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection && _steps.Count > 1;
    }

    private int IndexOf(double step)
    {
        for (int i = 0; i < _steps.Count; i++)
        {
            if (GridSteps.NearlyEqual(_steps[i], step))
            {
                return i;
            }
        }

        return -1;
    }

    private bool TryAsk(string title, string acceptText, double current, out double step)
    {
        step = current;
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
            Text = "Step mm",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        var input = new NumericUpDown
        {
            DecimalPlaces = 3,
            Increment = 0.1m,
            Minimum = (decimal)GridSteps.MinMm,
            Maximum = (decimal)GridSteps.MaxMm,
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

        step = GridSteps.Normalize((double)input.Value);
        return true;
    }
}
