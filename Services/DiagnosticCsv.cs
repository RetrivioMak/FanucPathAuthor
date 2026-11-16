using System.Globalization;
using System.Text;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class DiagnosticCsv
{
    public static void Write(IReadOnlyList<RobotWaypoint> waypoints, string filePath)
    {
        IReadOnlyList<RobotWaypoint> exportWaypoints = waypoints.Where(static wp => !wp.IsPreviewOnly).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("Index,Role,ExportX_mm,ExportY_mm,ExportR_deg,GlueOn,LocalX_px,LocalY_px,SensorX_px,SensorY_px");

        foreach (RobotWaypoint wp in exportWaypoints)
        {
            sb.Append(wp.Index).Append(',')
                .Append(Escape(wp.Role)).Append(',')
                .Append(Format(wp.ExportX_mm)).Append(',')
                .Append(Format(wp.ExportY_mm)).Append(',')
                .Append(Format(wp.ExportR_deg)).Append(',')
                .Append(wp.GlueOn ? "1" : "0").Append(',')
                .Append(Format(wp.LocalX_px)).Append(',')
                .Append(Format(wp.LocalY_px)).Append(',')
                .Append(Format(wp.SensorX_px)).Append(',')
                .Append(Format(wp.SensorY_px))
                .AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private static string Format(double value) =>
        double.IsNaN(value)
            ? string.Empty
            : value.ToString("0.###", CultureInfo.InvariantCulture);

    private static string Escape(string value) =>
        value.Contains(',') ? $"\"{value}\"" : value;
}
