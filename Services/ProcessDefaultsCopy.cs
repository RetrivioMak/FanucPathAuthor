using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class ProcessDefaultsCopy
{
    public static ProcessDefaults Clone(ProcessDefaults source) =>
        new()
        {
            Z_mm = source.Z_mm,
            W_deg = source.W_deg,
            P_deg = source.P_deg,
            R_deg = source.R_deg,
            Speed_mm_s = source.Speed_mm_s,
            GlueOn = source.GlueOn,
            CoordinateMode = source.CoordinateMode,
            UseFovCenterNeedleOffset = source.UseFovCenterNeedleOffset,
            LocalRotation = source.LocalRotation,
            UseNeedleOffset = source.UseNeedleOffset,
            NeedleOffsetX_mm = source.NeedleOffsetX_mm,
            NeedleOffsetY_mm = source.NeedleOffsetY_mm,
            NeedleOffsetR_deg = source.NeedleOffsetR_deg
        };
}
