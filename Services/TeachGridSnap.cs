using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class TeachGridSnap
{
    public static (double X, double Y) Snap(double x, double y, TeachGridSettings grid)
    {
        if (grid.Step_mm <= 0)
        {
            return (x, y);
        }

        return (
            SnapAxis(x, grid.OriginX_mm, grid.Step_mm),
            SnapAxis(y, grid.OriginY_mm, grid.Step_mm));
    }

    public static double SnapAxis(double value, double origin, double step)
    {
        if (step <= 0)
        {
            return value;
        }

        return origin + Math.Round((value - origin) / step, MidpointRounding.AwayFromZero) * step;
    }
}
