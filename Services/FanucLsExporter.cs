using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public sealed class FanucLsExporter : IProgramExporter
{
    public string Id => "fanuc-ls";

    public string MenuLabel => "Export Fanuc .LS...";

    public string FileFilter => "Fanuc LS|*.LS;*.ls";

    public string DefaultFileName => "GLUE_PATH.LS";

    public string? Validate(PathDocument document, RobotCalibrationFile? calibration)
    {
        if (document.Workspace == WorkspaceKind.Teach)
        {
            return null;
        }

        if (calibration?.PixelToRobot == null)
        {
            return "Load a calibration JSON file first.";
        }

        if (document.Exporters.FanucLs.CoordinateMode != ExportCoordinateMode.UframeAbsolute)
        {
            return "Fanuc .LS export requires UFRAME / UTOOL absolute mode. Switch export settings or use CSV for offset modes.";
        }

        return null;
    }

    public void Export(ProgramExportRequest request)
    {
        IReadOnlyList<RobotWaypoint> trajectory = GeometryHelper.BuildWaypoints(request.Document, request.Calibration);
        if (trajectory.Count == 0)
        {
            throw new InvalidOperationException("No waypoints to export.");
        }

        FanucLsSettings settings = request.Document.Exporters.FanucLs;
        FanucExporter.ExportLs(
            trajectory,
            request.Calibration,
            Path.GetFileNameWithoutExtension(request.DestinationPath),
            request.DestinationPath,
            applyFrameYReflection: request.Document.Workspace != WorkspaceKind.Teach,
            userFrame: settings.UserFrame,
            userTool: settings.UserTool);
    }
}
