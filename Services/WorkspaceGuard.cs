using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class WorkspaceGuard
{
    public static bool CanAdopt(WorkspaceKind current, WorkspaceKind next) =>
        current == next && current != WorkspaceKind.Unset;

    public static string RefusalMessage(WorkspaceKind current) =>
        current switch
        {
            WorkspaceKind.Teach => "This project is Teach. Start a new project to add an FOV image.",
            WorkspaceKind.Cv => "This project is CV. Start a new project to add teach points.",
            _ => "Choose a CV or Teach project first."
        };

    public static void Require(WorkspaceKind current, WorkspaceKind next)
    {
        if (!CanAdopt(current, next))
        {
            throw new InvalidOperationException(RefusalMessage(current));
        }
    }
}
