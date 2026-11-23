using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public sealed partial class GridStepsDialog : Form
{
    private readonly List<double> _steps;
    private int _activeIndex;

    public GridStepsDialog(IReadOnlyList<double> steps, double activeStep)
    {
        InitializeComponent();
        Icon = AppIcon.Load();
        _steps = GridSteps.NormalizeList(steps, activeStep, out double current);
        _activeIndex = IndexOf(current);
        if (_activeIndex < 0)
        {
            _activeIndex = 0;
        }

        _addButton.Click += (_, _) => AddStep();
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
        decimal initial = Math.Round((decimal)current, 3, MidpointRounding.AwayFromZero);
        using var dialog = new ValuePromptDialog
        {
            Text = title,
            AcceptText = acceptText,
            Caption = "Step mm",
            Minimum = (decimal)GridSteps.MinMm,
            Maximum = (decimal)GridSteps.MaxMm,
            Font = Font
        };
        dialog.Value = initial;
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        step = GridSteps.Normalize((double)dialog.Value);
        return true;
    }
}
