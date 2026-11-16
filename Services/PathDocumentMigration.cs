using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class PathDocumentMigration
{
    public static void Migrate(PathDocument document)
    {
        if (document.SchemaVersion < 3)
        {
            document.SchemaVersion = 3;
            document.Exporters.FanucLs.LocalRotation = ExportLocalRotation.Degree180;
        }

        if (document.SchemaVersion < 4)
        {
            document.SchemaVersion = 4;
            document.Workspace = WorkspaceKind.Cv;
        }
    }
}
