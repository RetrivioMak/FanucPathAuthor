using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public enum SegmentPropertyField
{
    GlueOn,
    Speed,
    X1,
    Y1,
    X2,
    Y2,
    X3,
    Y3,
    Width
}

/// <summary>One editable point of a segment. Which points exist depends only on <see cref="SegmentKind"/>.</summary>
public readonly record struct SegmentPoint(string Name, SegmentPropertyField X, SegmentPropertyField Y);

public static class SegmentProperties
{
    private const double ValueTolerance = 0.0001;

    public static string TypeName(SegmentKind kind) =>
        kind switch
        {
            SegmentKind.Dot => "Dot",
            SegmentKind.Line => "Line",
            SegmentKind.Arc => "Arc",
            _ => kind.ToString()
        };

    public static IReadOnlyList<SegmentPoint> PointsOf(SegmentKind kind) =>
        kind switch
        {
            SegmentKind.Dot => [new("Point", SegmentPropertyField.X1, SegmentPropertyField.Y1)],
            SegmentKind.Line =>
            [
                new("Start", SegmentPropertyField.X1, SegmentPropertyField.Y1),
                new("End", SegmentPropertyField.X2, SegmentPropertyField.Y2)
            ],
            SegmentKind.Arc =>
            [
                new("Start", SegmentPropertyField.X1, SegmentPropertyField.Y1),
                new("Mid", SegmentPropertyField.X2, SegmentPropertyField.Y2),
                new("End", SegmentPropertyField.X3, SegmentPropertyField.Y3)
            ],
            _ => []
        };

    public static double Read(PathSegment segment, SegmentPropertyField field) =>
        field switch
        {
            SegmentPropertyField.Speed => segment.Speed_mm_s,
            SegmentPropertyField.X1 => segment.X1,
            SegmentPropertyField.Y1 => segment.Y1,
            SegmentPropertyField.X2 => segment.X2,
            SegmentPropertyField.Y2 => segment.Y2,
            SegmentPropertyField.X3 => segment.X3,
            SegmentPropertyField.Y3 => segment.Y3,
            SegmentPropertyField.Width => segment.Width_mm,
            _ => 0
        };

    public static bool WouldChange(PathSegment segment, SegmentPropertyField field, double number, bool flag) =>
        field switch
        {
            SegmentPropertyField.GlueOn => segment.GlueOn != flag,
            SegmentPropertyField.Speed => !Nearly(segment.Speed_mm_s, number),
            SegmentPropertyField.X1 => !Nearly(segment.X1, number),
            SegmentPropertyField.Y1 => !Nearly(segment.Y1, number),
            SegmentPropertyField.X2 => !Nearly(segment.X2, number),
            SegmentPropertyField.Y2 => !Nearly(segment.Y2, number),
            SegmentPropertyField.X3 => !Nearly(segment.X3, number),
            SegmentPropertyField.Y3 => !Nearly(segment.Y3, number),
            SegmentPropertyField.Width => segment.Kind != SegmentKind.Dot && !Nearly(segment.Width_mm, number),
            _ => false
        };

    public static void Apply(PathSegment segment, SegmentPropertyField field, double number, bool flag)
    {
        switch (field)
        {
            case SegmentPropertyField.GlueOn:
                segment.GlueOn = flag;
                break;
            case SegmentPropertyField.Speed:
                segment.Speed_mm_s = number;
                break;
            case SegmentPropertyField.X1:
                segment.X1 = number;
                break;
            case SegmentPropertyField.Y1:
                segment.Y1 = number;
                break;
            case SegmentPropertyField.X2:
                segment.X2 = number;
                break;
            case SegmentPropertyField.Y2:
                segment.Y2 = number;
                break;
            case SegmentPropertyField.X3:
                segment.X3 = number;
                break;
            case SegmentPropertyField.Y3:
                segment.Y3 = number;
                break;
            case SegmentPropertyField.Width when segment.Kind != SegmentKind.Dot:
                segment.Width_mm = number;
                break;
        }
    }

    private static bool Nearly(double left, double right) => Math.Abs(left - right) <= ValueTolerance;
}
