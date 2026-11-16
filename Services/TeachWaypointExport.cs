using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class TeachWaypointExport
{
    public static WaypointExportValues Compute(double x_mm, double y_mm, ProcessDefaults defaults)
    {
        double exportX = x_mm;
        double exportY = y_mm;
        double exportR = defaults.R_deg;
        if (defaults.UseNeedleOffset)
        {
            exportX += defaults.NeedleOffsetX_mm;
            exportY += defaults.NeedleOffsetY_mm;
            exportR += defaults.NeedleOffsetR_deg;
        }

        return new WaypointExportValues(exportX, exportY, exportR, x_mm, y_mm);
    }
}
