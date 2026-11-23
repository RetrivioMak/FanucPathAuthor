using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Export.FanucFeed;

public static class BeadFeed
{
    public const double DefaultNeedleDiameter_mm = 0.48;

    /// <summary>
    /// Speed that lays a semicircular bead as wide as the needle at <see cref="DefaultFlow"/>.
    /// </summary>
    public const double ReferenceBeadSpeed_mm_s = 100;

    public static double AreaMm2(double widthMm) => Math.PI * widthMm * widthMm / 8.0;

    public static double DefaultFlow(double needleDiameterMm) =>
        ReferenceBeadSpeed_mm_s * AreaMm2(needleDiameterMm);

    public static double BeadSpeed(double widthMm, double flowMm3PerSec)
    {
        if (widthMm <= 0 || flowMm3PerSec <= 0 ||
            !double.IsFinite(widthMm) || !double.IsFinite(flowMm3PerSec))
        {
            return double.NaN;
        }

        return flowMm3PerSec / AreaMm2(widthMm);
    }

    public static double MotionSpeed(FanucExportPoint point, int index, BeadFeedSettings settings)
    {
        if (UsesTravel(point, index))
        {
            return settings.TravelSpeed_mm_s;
        }

        double speed = BeadSpeed(point.Width_mm, settings.Flow_mm3_s);
        if (!double.IsFinite(speed))
        {
            return settings.TravelSpeed_mm_s;
        }

        return Math.Clamp(speed, settings.MinSpeed_mm_s, settings.MaxSpeed_mm_s);
    }

    public static bool UsesTravel(FanucExportPoint point, int index)
    {
        if (index <= 0 || point.Width_mm <= 0)
        {
            return true;
        }

        return point.Role.Equals("start", StringComparison.OrdinalIgnoreCase) ||
               point.Role.Equals("dot", StringComparison.OrdinalIgnoreCase);
    }
}
