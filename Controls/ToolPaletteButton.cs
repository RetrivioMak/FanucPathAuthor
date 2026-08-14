namespace FanucPathAuthor.Controls;

public sealed class ToolPaletteButton : Button
{
    private bool _isChecked;

    public bool IndependentToggle { get; set; }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value)
            {
                return;
            }

            _isChecked = value;
            UpdateVisualState();
        }
    }

    public ToolPaletteButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 1;
        FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
        TextImageRelation = TextImageRelation.ImageAboveText;
        ImageAlign = ContentAlignment.TopCenter;
        TextAlign = ContentAlignment.BottomCenter;
        MinimumSize = new Size(78, 78);
        Size = new Size(78, 78);
        Margin = new Padding(4, 2, 4, 2);
        Padding = new Padding(4, 8, 4, 6);
        Font = new Font("Segoe UI", 9f);
        UseVisualStyleBackColor = true;
        UpdateVisualState();
    }

    public void ConfigureIconOnly()
    {
        Text = string.Empty;
        TextImageRelation = TextImageRelation.Overlay;
        ImageAlign = ContentAlignment.MiddleCenter;
        Padding = new Padding(6);
    }

    protected override void OnClick(EventArgs e)
    {
        if (IndependentToggle)
        {
            IsChecked = !IsChecked;
        }
        else if (Tag is DrawTool)
        {
            IsChecked = true;
        }

        base.OnClick(e);
    }

    private void UpdateVisualState()
    {
        BackColor = _isChecked ? Color.FromArgb(210, 228, 255) : SystemColors.Control;
        FlatAppearance.BorderColor = _isChecked
            ? Color.FromArgb(0, 102, 204)
            : Color.FromArgb(180, 180, 180);
    }
}
