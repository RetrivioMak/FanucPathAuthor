using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class ProcessDefaultsCopy
{
    public static ProcessDefaults Clone(ProcessDefaults source) =>
        new()
        {
            Z_mm = source.Z_mm,
            W_deg = source.W_deg,
            P_deg = source.P_deg,
            R_deg = source.R_deg,
            GlueOn = source.GlueOn,
            MinWidth_mm = source.MinWidth_mm,
            ReferenceMmPerPx = source.ReferenceMmPerPx,
            Widths_mm = new List<double>(source.Widths_mm),
            ZDeltas_mm = new List<double>(source.ZDeltas_mm)
        };
}
