using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

/// <summary>
/// An open project. The type is fixed when the session is created.
/// </summary>
public sealed class ProjectSession
{
    private ProjectSession(PathDocument document)
    {
        Document = document;
        Kind = document.Workspace;
    }

    public WorkspaceKind Kind { get; }

    public PathDocument Document { get; }

    public string? FilePath { get; set; }

    public RobotCalibrationFile? Calibration { get; set; }

    public string? CalibrationPath { get; set; }

    public static ProjectSession Start(PathDocument document, WorkspaceKind kind)
    {
        if (kind is not (WorkspaceKind.Cv or WorkspaceKind.Teach))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), "Choose CV or Teach.");
        }

        if (document.Workspace == WorkspaceKind.Unset)
        {
            Initialize(document, kind);
        }
        else if (document.Workspace != kind)
        {
            throw new InvalidOperationException(WorkspaceGuard.RefusalMessage(document.Workspace));
        }

        return new ProjectSession(document);
    }

    public static ProjectSession Bind(PathDocument document)
    {
        if (document.Workspace is not (WorkspaceKind.Cv or WorkspaceKind.Teach))
        {
            throw new InvalidOperationException("This file has no project type.");
        }

        return new ProjectSession(document);
    }

    public void Require(WorkspaceKind kind)
    {
        if (Kind != kind)
        {
            throw new InvalidOperationException(WorkspaceGuard.RefusalMessage(Kind));
        }
    }

    private static void Initialize(PathDocument document, WorkspaceKind kind)
    {
        document.Workspace = kind;
        document.SchemaVersion = Math.Max(document.SchemaVersion, FrameMillimeters.SchemaVersion);
        StrokeWidths.EnsureStored(document.Defaults);
        if (document.Grid.Step_mm <= 0)
        {
            document.Grid.Step_mm = 0.2;
        }

        if (document.Grid.Steps_mm.Count == 0)
        {
            document.Grid.Steps_mm.AddRange(TeachGridSettings.DefaultStepsMm);
        }

        document.Grid.Enabled = true;
        if (kind != WorkspaceKind.Teach)
        {
            return;
        }

        document.Frame = new FrameDefinition();
        document.FrameIsDefined = true;
    }
}
