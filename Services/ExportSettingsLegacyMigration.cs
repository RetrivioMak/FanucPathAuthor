using System.Text.Json;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class ExportSettingsLegacyMigration
{
    public static void ApplyLocalRotation(JsonElement settingsElement, FanucLsSettings fanuc)
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

        fanuc.LocalRotation = legacy.GetBoolean()
            ? ExportLocalRotation.Degree180
            : ExportLocalRotation.None;
    }
}
