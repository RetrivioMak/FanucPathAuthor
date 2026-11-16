using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class TeachWaypointExport
{
    public static WaypointExportValues Compute(double x_mm, double y_mm, ProcessDefaults process, FanucLsSettings fanuc)
    {
        double exportX = x_mm;
        double exportY = y_mm;
        double exportR = process.R_deg;
        if (fanuc.UseNeedleOffset)
        {
            exportX += fanuc.NeedleOffsetX_mm;
            exportY += fanuc.NeedleOffsetY_mm;
            exportR += fanuc.NeedleOffsetR_deg;
        }

        return new WaypointExportValues(exportX, exportY, exportR, x_mm, y_mm);
    }
}
