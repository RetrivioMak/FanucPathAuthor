using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public partial class SegmentListPanel : UserControl
{
    private bool _updating;

    public SegmentListPanel()
    {
        InitializeComponent();
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
    }

    public event EventHandler? SelectionChanged;

    public event EventHandler? MoveUpRequested;

    public event EventHandler? MoveDownRequested;

    public int? SelectedIndex =>
        _segmentList.SelectedItems.Count == 1 && _segmentList.SelectedItems[0].Tag is int index
            ? index
            : null;

    public void Show(PathDocument document, int? preferredSelection)
    {
        bool millimeters = document.Workspace == WorkspaceKind.Teach || FrameMillimeters.StoresMillimeters(document);
        double gapThreshold = millimeters
            ? PathSequenceService.TeachGapThresholdMm
            : PathSequenceService.DefaultGapThresholdPx;
        string gapUnit = millimeters ? "mm" : "px";
        IReadOnlyList<SequenceGap> gaps = PathSequenceService.AnalyzeGaps(document.Segments);
        int selected = preferredSelection ?? SelectedIndex ?? -1;

        _updating = true;
        try
        {
            _segmentList.Items.Clear();
            for (int i = 0; i < document.Segments.Count; i++)
            {
                PathSegment segment = document.Segments[i];
                var item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(segment.Kind.ToString());
                item.SubItems.Add(segment.DisplayName);
                if (i < gaps.Count)
                {
                    SequenceGap gap = gaps[i];
                    item.SubItems.Add(FormatGapDistance(gap.Distance_px, gapUnit));
                    if (gap.HasGap(gapThreshold))
                    {
                        item.ForeColor = Color.DarkOrange;
                    }
                }
                else
                {
                    item.SubItems.Add("¡ª");
                }

                item.Tag = i;
                _segmentList.Items.Add(item);
            }

            if (selected >= 0 && selected < _segmentList.Items.Count)
            {
                _segmentList.Items[selected].Selected = true;
            }
        }
        finally
        {
            _updating = false;
        }

        UpdateGapSummary(gaps, gapThreshold, gapUnit);
        UpdateReorderButtons(document.Segments.Count);
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
            _gapSummaryLabel.Text = $"Sequence: all connected (¡Ü {limit} {unit})";
            _gapSummaryLabel.ForeColor = Color.DimGray;
            return;
        }

        _gapSummaryLabel.Text = $"{gapCount} sequence gap(s) (> {limit} {unit})";
        _gapSummaryLabel.ForeColor = Color.DarkOrange;
    }

    private static string FormatGapDistance(double distance, string unit) =>
        distance < 0.0005 ? $"0 {unit}" : unit == "mm" ? $"{distance:0.000} mm" : $"{distance:F1} px";
}
