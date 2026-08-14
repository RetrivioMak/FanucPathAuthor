using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class PathDocumentMigration
{
    public static void Migrate(PathDocument document)
    {
        if (document.SchemaVersion >= 3)
        {
            return;
        }

        document.SchemaVersion = 3;
        document.Defaults.UseFixedCameraInversion = true;
    }
}
