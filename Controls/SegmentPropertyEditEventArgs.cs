using DispensePathAuthor.Services;

namespace DispensePathAuthor.Controls;

public sealed class SegmentPropertyEditEventArgs : EventArgs
{
    public required int Index { get; init; }
    public required SegmentPropertyField Field { get; init; }
    public required double Number { get; init; }
    public required bool Flag { get; init; }
}
