using System.Text.Json;
using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class ExportSettingsLegacyMigration
{
    public static void ApplyLocalRotation(JsonElement settingsElement, ProcessDefaults defaults)
    {
        if (settingsElement.TryGetProperty("localRotation", out _))
        {
            return;
        }

        if (!settingsElement.TryGetProperty("useFixedCameraInversion", out JsonElement legacy) ||
            legacy.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return;
        }

        defaults.LocalRotation = legacy.GetBoolean()
            ? ExportLocalRotation.Degree180
            : ExportLocalRotation.None;
    }
}
