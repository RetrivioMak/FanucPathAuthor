using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class WorkspaceGuard
{
    public static bool CanAdopt(WorkspaceKind current, WorkspaceKind next) =>
        current == WorkspaceKind.Unset || current == next;

    public static string RefusalMessage(WorkspaceKind current) =>
        current == WorkspaceKind.Teach
            ? "This project is Teach. Start a new project to add an FOV image."
            : "This project is CV. Start a new project to add teach points.";

    public static void Adopt(PathDocument document, WorkspaceKind kind)
    {
        if (kind == WorkspaceKind.Unset)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), "Choose CV or Teach.");
        }

        if (!CanAdopt(document.Workspace, kind))
        {
            throw new InvalidOperationException(RefusalMessage(document.Workspace));
        }

        if (document.Workspace == kind)
        {
            return;
        }

        document.Workspace = kind;
        document.SchemaVersion = Math.Max(document.SchemaVersion, 4);
        if (kind != WorkspaceKind.Teach)
        {
            return;
        }

        document.Frame = new FrameDefinition();
        document.FrameIsDefined = true;
        if (document.Grid.Step_mm <= 0)
        {
            document.Grid.Step_mm = 0.2;
        }

        document.Grid.Enabled = true;
    }
}
