using System.Globalization;
using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public sealed class SegmentPropertyEditEventArgs : EventArgs
{
    public required int Index { get; init; }
    public required SegmentPropertyField Field { get; init; }
    public required double Number { get; init; }
    public required bool Flag { get; init; }
}

public partial class SegmentListPanel : UserControl
{
    private const decimal CoordinateMin = -1_000_000m;
    private const decimal CoordinateMax = 1_000_000m;
    private const decimal SpeedMax = 1_000_000m;
    private const int PropertiesMinHeight = 140;
    private const int PropertiesDefaultHeight = 320;
    private const int ListMinHeight = 140;

    private static readonly Color SelectedNameColor = Color.FromArgb(173, 214, 255);

    private readonly DataGridView _propertyGrid;
    private readonly Label _propertiesCaption;
    private readonly Panel _propertiesHost;
    private string? _editOriginal;
    private double _widthFloor = StrokeWidths.MinMm;
    private bool _updating;
    private bool _committing;

    public SegmentListPanel()
    {
        InitializeComponent();
        _propertyGrid = CreatePropertyGrid();
        _propertiesCaption = new Label
        {
            Dock = DockStyle.Top,
            ForeColor = Color.FromArgb(30, 70, 140),
            Height = 26,
            Padding = new Padding(6, 0, 0, 0),
            Text = "Properties",
            TextAlign = ContentAlignment.MiddleLeft
        };
        _propertiesHost = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = PropertiesDefaultHeight,
            Name = "propertiesHost"
        };
        var resizeGrip = new PropertiesResizeGrip(_propertiesHost, PropertiesMinHeight, ListMinHeight);
        _propertiesHost.Controls.Add(_propertyGrid);
        _propertiesHost.Controls.Add(_propertiesCaption);
        _propertiesHost.Controls.Add(resizeGrip);
        Controls.Add(_propertiesHost);

        _propertyGrid.CellBeginEdit += OnPropertyBeginEdit;
        _propertyGrid.CellEndEdit += OnPropertyEndEdit;
        _propertyGrid.CellValidating += OnPropertyValidating;
        _propertyGrid.CellFormatting += OnPropertyFormatting;
        _propertyGrid.DataError += (_, e) => e.ThrowException = false;

        _segmentList.SelectedIndexChanged += (_, _) =>
        {
            if (_updating)
            {
                return;
            }

            UpdateReorderButtons(_segmentList.Items.Count);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        };
        _moveUpButton.Click += (_, _) => MoveUpRequested?.Invoke(this, EventArgs.Empty);
        _moveDownButton.Click += (_, _) => MoveDownRequested?.Invoke(this, EventArgs.Empty);
        ShowProperties(null, -1, "mm");
    }

    public event EventHandler? SelectionChanged;

    public event EventHandler? MoveUpRequested;

    public event EventHandler? MoveDownRequested;

    public event EventHandler<SegmentPropertyEditEventArgs>? PropertiesEdited;

    public int? SelectedIndex =>
        _segmentList.SelectedItems.Count == 1 && _segmentList.SelectedItems[0].Tag is int index
            ? index
            : null;

    public bool IsEditingProperties => _propertyGrid.ContainsFocus;

    public void Show(PathDocument document, int? preferredSelection)
    {
        bool millimeters = document.Workspace == WorkspaceKind.Teach || FrameMillimeters.StoresMillimeters(document);
        double gapThreshold = millimeters
            ? PathSequenceService.TeachGapThresholdMm
            : PathSequenceService.DefaultGapThresholdPx;
        string gapUnit = millimeters ? "mm" : "px";
        _widthFloor = StrokeWidths.Floor(document.Defaults.MinWidth_mm);
        IReadOnlyList<SequenceGap> gaps = PathSequenceService.AnalyzeGaps(document.Segments);
        int selected = preferredSelection ?? SelectedIndex ?? -1;

        _updating = true;
        try
        {
            if (_segmentList.Items.Count == document.Segments.Count)
            {
                for (int i = 0; i < document.Segments.Count; i++)
                {
                    WriteRow(_segmentList.Items[i], i, document.Segments[i], gaps, gapThreshold, gapUnit);
                }
            }
            else
            {
                _segmentList.BeginUpdate();
                try
                {
                    _segmentList.Items.Clear();
                    for (int i = 0; i < document.Segments.Count; i++)
                    {
                        var item = new ListViewItem((i + 1).ToString());
                        item.SubItems.Add(string.Empty);
                        item.SubItems.Add(string.Empty);
                        item.SubItems.Add(string.Empty);
                        WriteRow(item, i, document.Segments[i], gaps, gapThreshold, gapUnit);
                        _segmentList.Items.Add(item);
                    }
                }
                finally
                {
                    _segmentList.EndUpdate();
                }
            }

            SelectRow(selected);
            PathSegment? segment = selected >= 0 && selected < document.Segments.Count
                ? document.Segments[selected]
                : null;
            ShowProperties(segment, selected, gapUnit);
        }
        finally
        {
            _updating = false;
        }

        UpdateGapSummary(gaps, gapThreshold, gapUnit);
        UpdateReorderButtons(document.Segments.Count);
    }

    private static DataGridView CreatePropertyGrid()
    {
        var grid = new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            Dock = DockStyle.Fill,
            EditMode = DataGridViewEditMode.EditOnEnter,
            EnableHeadersVisualStyles = true,
            MultiSelect = false,
            Name = "propertyGrid",
            RowHeadersVisible = false,
            ScrollBars = ScrollBars.Vertical,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ShowCellToolTips = true,
            StandardTab = true
        };
        grid.ColumnHeadersHeight = 32;
        grid.RowTemplate.Height = 32;
        grid.DefaultCellStyle.Padding = new Padding(6, 4, 4, 4);
        grid.DefaultCellStyle.SelectionBackColor = SystemColors.Window;
        grid.DefaultCellStyle.SelectionForeColor = SystemColors.WindowText;
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Name",
            HeaderText = "Name",
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            FillWeight = 58
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Value",
            HeaderText = "Value",
            SortMode = DataGridViewColumnSortMode.NotSortable,
            FillWeight = 42
        });
        return grid;
    }

    private void ShowProperties(PathSegment? segment, int index, string unit)
    {
        if (segment == null || index < 0)
        {
            _propertiesCaption.Text = "Properties";
            if (_propertyGrid.Rows.Count > 0 && !_propertyGrid.IsCurrentCellInEditMode)
            {
                _propertyGrid.Rows.Clear();
            }

            return;
        }

        _propertiesCaption.Text = $"Properties — #{index + 1}";
        IReadOnlyList<PropertyLine> lines = BuildLines(segment.Kind, unit);
        if (!SameLines(lines))
        {
            if (_propertyGrid.IsCurrentCellInEditMode)
            {
                return;
            }

            RebuildRows(segment, lines);
            return;
        }

        UpdateValues(segment);
    }

    private static IReadOnlyList<PropertyLine> BuildLines(SegmentKind kind, string unit)
    {
        var lines = new List<PropertyLine>
        {
            new("Type", null, PropertyEditor.ReadOnlyText),
            new("Glue", SegmentPropertyField.GlueOn, PropertyEditor.Bool),
            new("Speed mm/s", SegmentPropertyField.Speed, PropertyEditor.Number)
        };
        if (kind is SegmentKind.Line or SegmentKind.Arc)
        {
            lines.Add(new("Width mm", SegmentPropertyField.Width, PropertyEditor.Number));
        }
        foreach (SegmentPoint point in SegmentProperties.PointsOf(kind))
        {
            lines.Add(new($"{point.Name} X {unit}", point.X, PropertyEditor.Number));
            lines.Add(new($"{point.Name} Y {unit}", point.Y, PropertyEditor.Number));
        }

        return lines;
    }

    private bool SameLines(IReadOnlyList<PropertyLine> lines)
    {
        if (_propertyGrid.Rows.Count != lines.Count)
        {
            return false;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            if (_propertyGrid.Rows[i].Tag is not PropertyLine existing || existing != lines[i])
            {
                return false;
            }
        }

        return true;
    }

    private void RebuildRows(PathSegment segment, IReadOnlyList<PropertyLine> lines)
    {
        _propertyGrid.Rows.Clear();
        foreach (PropertyLine line in lines)
        {
            int added = _propertyGrid.Rows.Add();
            DataGridViewRow row = _propertyGrid.Rows[added];
            row.Tag = line;
            row.Cells[0].Value = line.Name;
            row.Cells[0].ToolTipText = line.Editor == PropertyEditor.ReadOnlyText ? "Fixed" : string.Empty;
            if (line.Editor == PropertyEditor.Bool)
            {
                var combo = new DataGridViewComboBoxCell
                {
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                    FlatStyle = FlatStyle.Flat
                };
                combo.Items.Add("On");
                combo.Items.Add("Off");
                row.Cells[1] = combo;
            }
            else if (line.Editor == PropertyEditor.ReadOnlyText)
            {
                row.Cells[1].ReadOnly = true;
            }

            row.Cells[1].Value = FormatValue(segment, line);
        }
    }

    private void UpdateValues(PathSegment segment)
    {
        foreach (DataGridViewRow row in _propertyGrid.Rows)
        {
            if (row.Tag is not PropertyLine line)
            {
                continue;
            }

            if (_committing &&
                _propertyGrid.CurrentCell is DataGridViewCell current &&
                current.RowIndex == row.Index &&
                current.ColumnIndex == 1)
            {
                continue;
            }

            string text = FormatValue(segment, line);
            if (!string.Equals(row.Cells[1].Value?.ToString(), text, StringComparison.Ordinal))
            {
                row.Cells[1].Value = text;
            }
        }
    }

    private string FormatValue(PathSegment segment, PropertyLine line)
    {
        if (line.Field is not SegmentPropertyField field)
        {
            return SegmentProperties.TypeName(segment.Kind);
        }

        if (line.Editor == PropertyEditor.Bool)
        {
            return segment.GlueOn ? "On" : "Off";
        }

        double number = SegmentProperties.Read(segment, field);
        if (field == SegmentPropertyField.Width && number <= 0)
        {
            number = _widthFloor;
        }

        return FormatNumber(number);
    }

    private static string FormatNumber(double value) =>
        value.ToString("0.###", CultureInfo.CurrentCulture);

    private void OnPropertyBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 1)
        {
            return;
        }

        _editOriginal = _propertyGrid.Rows[e.RowIndex].Cells[1].Value?.ToString();
    }

    private void OnPropertyValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        if (_updating || e.RowIndex < 0 || e.ColumnIndex != 1)
        {
            return;
        }

        if (_propertyGrid.Rows[e.RowIndex].Tag is not PropertyLine line ||
            line.Editor != PropertyEditor.Number)
        {
            _propertyGrid.Rows[e.RowIndex].ErrorText = string.Empty;
            return;
        }

        if (!TryParseNumber(Convert.ToString(e.FormattedValue, CultureInfo.CurrentCulture), out _))
        {
            e.Cancel = true;
            _propertyGrid.Rows[e.RowIndex].ErrorText = "Enter a number";
            return;
        }

        _propertyGrid.Rows[e.RowIndex].ErrorText = string.Empty;
    }

    private void OnPropertyEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (_updating || e.RowIndex < 0 || e.ColumnIndex != 1)
        {
            return;
        }

        if (_propertyGrid.Rows[e.RowIndex].Tag is not PropertyLine line ||
            line.Field is not SegmentPropertyField field)
        {
            return;
        }

        string? text = _propertyGrid.Rows[e.RowIndex].Cells[1].Value?.ToString();
        if (string.Equals(text, _editOriginal, StringComparison.Ordinal))
        {
            return;
        }

        switch (line.Editor)
        {
            case PropertyEditor.Bool:
                Commit(field, 0, string.Equals(text, "On", StringComparison.Ordinal));
                break;
            case PropertyEditor.Number when TryParseNumber(text, out double number):
                Commit(field, Clamp(field, number), false);
                break;
        }
    }

    private void OnPropertyFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.CellStyle == null || !_propertyGrid.Rows[e.RowIndex].Selected)
        {
            return;
        }

        if (e.ColumnIndex == 0)
        {
            e.CellStyle.BackColor = SelectedNameColor;
            e.CellStyle.SelectionBackColor = SelectedNameColor;
            e.CellStyle.SelectionForeColor = Color.Black;
            return;
        }

        Color background = e.CellStyle.BackColor;
        e.CellStyle.SelectionBackColor = background;
        e.CellStyle.SelectionForeColor = Color.Black;
    }

    private void Commit(SegmentPropertyField field, double number, bool flag)
    {
        if (_updating || SelectedIndex is not int index)
        {
            return;
        }

        _committing = true;
        try
        {
            PropertiesEdited?.Invoke(this, new SegmentPropertyEditEventArgs
            {
                Index = index,
                Field = field,
                Number = number,
                Flag = flag
            });
        }
        finally
        {
            _committing = false;
        }
    }

    private double Clamp(SegmentPropertyField field, double value)
    {
        if (field == SegmentPropertyField.Width)
        {
            if (!double.IsFinite(value))
            {
                return _widthFloor;
            }

            return Math.Min(StrokeWidths.MaxMm, Math.Max(_widthFloor, value));
        }

        double minimum = field == SegmentPropertyField.Speed ? 0 : (double)CoordinateMin;
        double maximum = field == SegmentPropertyField.Speed ? (double)SpeedMax : (double)CoordinateMax;
        if (!double.IsFinite(value))
        {
            return minimum;
        }

        return Math.Min(maximum, Math.Max(minimum, value));
    }

    private static bool TryParseNumber(string? text, out double value)
    {
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
        {
            return true;
        }

        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private void SelectRow(int selected)
    {
        if (selected >= 0 && selected < _segmentList.Items.Count)
        {
            ListViewItem item = _segmentList.Items[selected];
            bool changed = !item.Selected || _segmentList.SelectedItems.Count != 1;
            if (changed)
            {
                _segmentList.SelectedItems.Clear();
                item.Selected = true;
            }

            if (changed && !_committing)
            {
                item.EnsureVisible();
            }

            return;
        }

        if (_segmentList.SelectedItems.Count > 0)
        {
            _segmentList.SelectedItems.Clear();
        }
    }

    private static void WriteRow(
        ListViewItem item,
        int index,
        PathSegment segment,
        IReadOnlyList<SequenceGap> gaps,
        double gapThreshold,
        string gapUnit)
    {
        item.Text = (index + 1).ToString();
        item.SubItems[1].Text = SegmentProperties.TypeName(segment.Kind);
        item.SubItems[2].Text = segment.DisplayName;
        bool gap = false;
        if (index < gaps.Count)
        {
            SequenceGap sequenceGap = gaps[index];
            item.SubItems[3].Text = FormatGapDistance(sequenceGap.Distance_px, gapUnit);
            gap = sequenceGap.HasGap(gapThreshold);
        }
        else
        {
            item.SubItems[3].Text = "—";
        }

        item.ForeColor = gap ? Color.DarkOrange : SystemColors.WindowText;
        item.Tag = index;
    }

    private void UpdateReorderButtons(int segmentCount)
    {
        if (SelectedIndex is not int index)
        {
            _moveUpButton.Enabled = false;
            _moveDownButton.Enabled = false;
            return;
        }

        _moveUpButton.Enabled = PathSequenceService.CanMoveSegment(segmentCount, index, -1);
        _moveDownButton.Enabled = PathSequenceService.CanMoveSegment(segmentCount, index, 1);
    }

    private void UpdateGapSummary(IReadOnlyList<SequenceGap> gaps, double threshold, string unit)
    {
        int gapCount = gaps.Count(gap => gap.HasGap(threshold));
        if (gaps.Count == 0)
        {
            _gapSummaryLabel.Text = "Sequence: single segment";
            _gapSummaryLabel.ForeColor = Color.DimGray;
            return;
        }

        string limit = unit == "mm" ? threshold.ToString("0.###") : threshold.ToString("F0");
        if (gapCount == 0)
        {
            _gapSummaryLabel.Text = $"Sequence: all connected (≤ {limit} {unit})";
            _gapSummaryLabel.ForeColor = Color.DimGray;
            return;
        }

        _gapSummaryLabel.Text = $"{gapCount} sequence gap(s) (> {limit} {unit})";
        _gapSummaryLabel.ForeColor = Color.DarkOrange;
    }

    private static string FormatGapDistance(double distance, string unit) =>
        distance < 0.0005 ? $"0 {unit}" : unit == "mm" ? $"{distance:0.000} mm" : $"{distance:F1} px";

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_propertiesHost == null)
        {
            return;
        }

        int max = Math.Max(PropertiesMinHeight, ClientSize.Height - ListMinHeight);
        if (_propertiesHost.Height > max)
        {
            _propertiesHost.Height = max;
        }
    }

    private enum PropertyEditor
    {
        ReadOnlyText,
        Number,
        Bool
    }

    private readonly record struct PropertyLine(string Name, SegmentPropertyField? Field, PropertyEditor Editor);

    private sealed class PropertiesResizeGrip : Control
    {
        private readonly Panel _host;
        private readonly int _minHeight;
        private readonly int _listMinHeight;
        private bool _dragging;
        private int _startY;
        private int _startHeight;

        public PropertiesResizeGrip(Panel host, int minHeight, int listMinHeight)
        {
            _host = host;
            _minHeight = minHeight;
            _listMinHeight = listMinHeight;
            Height = 8;
            Dock = DockStyle.Top;
            Cursor = Cursors.HSplit;
            BackColor = Color.FromArgb(220, 220, 220);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _host.Parent != null)
            {
                _dragging = true;
                _startY = PointToScreen(e.Location).Y;
                _startHeight = _host.Height;
                Capture = true;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging && _host.Parent != null)
            {
                int delta = _startY - PointToScreen(e.Location).Y;
                int max = Math.Max(_minHeight, _host.Parent.ClientSize.Height - _listMinHeight);
                _host.Height = Math.Clamp(_startHeight + delta, _minHeight, max);
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_dragging)
            {
                _dragging = false;
                Capture = false;
            }

            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int center = Width / 2;
            int mid = Height / 2;
            using var pen = new Pen(Color.FromArgb(130, 130, 130));
            for (int x = center - 18; x <= center + 18; x += 6)
            {
                e.Graphics.DrawLine(pen, x, mid, x + 2, mid);
            }
        }
    }
}
