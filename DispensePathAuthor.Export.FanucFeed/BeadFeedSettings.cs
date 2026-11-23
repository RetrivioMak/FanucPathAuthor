namespace DispensePathAuthor.Export.FanucFeed;

public sealed class BeadFeedSettings
{
    public double NeedleDiameter_mm { get; init; } = BeadFeed.DefaultNeedleDiameter_mm;

    public double Flow_mm3_s { get; init; } = BeadFeed.DefaultFlow(BeadFeed.DefaultNeedleDiameter_mm);

    public double TravelSpeed_mm_s { get; init; } = 200;

    public double MinSpeed_mm_s { get; init; } = 1;

    public double MaxSpeed_mm_s { get; init; } = 2000;

    public static BeadFeedSettings CreateDefault() => new();
}
