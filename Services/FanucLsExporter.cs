using DispensePathAuthor.Contracts;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public sealed class FanucLsExporter : IProgramExporter
{
    public string Id => "fanuc-ls";

    public string MenuLabel => "Export Fanuc .LS...";

    public string SettingsTitle => "Fanuc .LS";

    public string FileFilter => "Fanuc LS|*.LS;*.ls";

    public string DefaultFileName => "GLUE_PATH.LS";

    public IReadOnlyList<string> RequiredChannels { get; } = ["X", "Y"];

    public UserControl? CreateSettingsPage() => null;

    public string? Validate(ProgramExportRequest request)
    {
        if (request.Host is not ExportHost host)
        {
            return "Fanuc export is missing the project.";
        }

        if (host.Document.Workspace == WorkspaceKind.Teach)
        {
            return null;
        }

        if (host.Calibration?.PixelToRobot == null)
        {
            return "Load a calibration JSON file first.";
        }

        if (host.Document.Exporters.FanucLs.CoordinateMode != ExportCoordinateMode.UframeAbsolute)
        {
            return "Fanuc .LS export requires UFRAME / UTOOL absolute mode. Switch export settings or use CSV for offset modes.";
        }

        return null;
    }

    public void Export(ProgramExportRequest request)
    {
        if (request.Host is not ExportHost host)
        {
            throw new InvalidOperationException("Fanuc export is missing the project.");
        }

        IReadOnlyList<RobotWaypoint> trajectory = GeometryHelper.BuildWaypoints(host.Document, host.Calibration);
        if (trajectory.Count == 0)
        {
            throw new InvalidOperationException("No waypoints to export.");
        }

        FanucLsSettings settings = host.Document.Exporters.FanucLs;
        FanucExporter.ExportLs(
            trajectory,
            host.Calibration,
            Path.GetFileNameWithoutExtension(request.DestinationPath),
            request.DestinationPath,
            applyFrameYReflection: host.Document.Workspace != WorkspaceKind.Teach,
            userFrame: settings.UserFrame,
            userTool: settings.UserTool);
    }
}
