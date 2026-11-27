using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Export.Fanuc;

public sealed class FanucLsExporter : IInteractiveProgramExporter
{
    public string Id => "fanuc-ls";

    public string MenuLabel => "Fanuc";

    public string SettingsTitle => "Fanuc .LS";

    public string FileFilter => "Fanuc LS|*.LS;*.ls";

    public string DefaultFileName => "GLUE_PATH.LS";

    public IReadOnlyList<string> RequiredChannels { get; } = ["X", "Y"];

    public Control? CreateSettingsPage() => null;

    public string? Validate(ProgramExportRequest request) => null;

    public void Export(ProgramExportRequest request) =>
        throw new InvalidOperationException("Fanuc export runs from its own dialog.");

    public string? TryRun(IWin32Window owner, IFanucExportSession session)
    {
        using var dialog = new FanucExportDialog(session.Settings, session.Calibration, session.IsTeachWorkspace);
        if (dialog.ShowDialog(owner) != DialogResult.OK || dialog.Settings == null)
        {
            return null;
        }

        session.ApplySettings(dialog.Settings);

        IReadOnlyList<FanucExportPoint> points = session.CreatePoints();
        if (!points.Any(static point => !point.IsPreviewOnly))
        {
            MessageBox.Show(owner, "No segments to export.", MenuLabel, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        if (!session.IsTeachWorkspace && !session.Calibration.HasTransform)
        {
            MessageBox.Show(
                owner,
                "Load a calibration JSON file first.",
                MenuLabel,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return null;
        }

        using var save = new SaveFileDialog
        {
            Filter = "Fanuc LS (*.ls)|*.ls|CSV (*.csv)|*.csv",
            FileName = DefaultFileName
        };
        if (save.ShowDialog(owner) != DialogResult.OK)
        {
            return null;
        }

        bool csv = save.FilterIndex == 2;
        if (!csv &&
            !session.IsTeachWorkspace &&
            dialog.Settings.CoordinateMode != FanucCoordinateMode.UframeAbsolute)
        {
            MessageBox.Show(
                owner,
                "Fanuc .LS export requires UFRAME / UTOOL absolute mode. Save CSV to keep this offset mode, or switch the Fanuc export options to absolute.",
                MenuLabel,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return null;
        }

        if (csv)
        {
            FanucExporter.ExportCsv(points, save.FileName);
            return save.FileName;
        }

        FanucExporter.ExportLs(
            points,
            Path.GetFileNameWithoutExtension(save.FileName),
            save.FileName,
            applyFrameYReflection: !session.IsTeachWorkspace,
            userFrame: dialog.Settings.UserFrame ?? session.Calibration.DefaultUserFrame,
            userTool: dialog.Settings.UserTool ?? session.Calibration.DefaultUserTool);
        return save.FileName;
    }
}
