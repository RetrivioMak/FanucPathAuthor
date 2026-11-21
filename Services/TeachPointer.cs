using System.Drawing;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class TeachPointer
{
    public static (double X, double Y, bool SnappedToPoint) Resolve(
        double x,
        double y,
        IReadOnlyList<(double X, double Y)> points,
        TeachGridSettings grid,
        Func<double, double, PointF> toScreen,
        double pickRadiusScreenPx,
        bool snapEnabled)
    {
        if (!snapEnabled)
        {
            return (x, y, false);
        }

        if (SnapPointHelper.TryFindNearestScreen(
                points.Select(static point => (point.X, point.Y)).ToList(),
                x,
                y,
                toScreen,
                pickRadiusScreenPx,
                out double snapX,
                out double snapY))
        {
            return (snapX, snapY, true);
        }

        (double gridX, double gridY) = TeachGridSnap.Snap(x, y, grid);
        return (gridX, gridY, false);
    }
}
