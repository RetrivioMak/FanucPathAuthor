using System.Text.Json;
using System.Text.Json.Serialization;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class ExportSettingsPersistence
{
    private const int CurrentSchemaVersion = 2;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new ExportCoordinateModeJsonConverter(),
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static string SettingsFilePath
    {
        get
        {
            string? overridePath = Environment.GetEnvironmentVariable("FANUC_PATH_AUTHOR_EXPORT_SETTINGS");
            if (!string.IsNullOrWhiteSpace(overridePath))
            {
                return overridePath;
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FanucPathAuthor",
                "export-settings.json");
        }
    }

    public static bool TryLoadInto(FanucLsSettings fanuc)
    {
        try
        {
            string path = SettingsFilePath;
            if (!File.Exists(path))
            {
                return false;
            }

            string json = File.ReadAllText(path);
            using JsonDocument document = JsonDocument.Parse(json);
            ExportSettingsSnapshot? snapshot =
                JsonSerializer.Deserialize<ExportSettingsSnapshot>(json, JsonOptions);
            if (snapshot == null)
            {
                return false;
            }

            snapshot.ApplyTo(fanuc);
            ExportSettingsLegacyMigration.ApplyLocalRotation(document.RootElement, fanuc);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void Save(FanucLsSettings fanuc)
    {
        try
        {
            ExportSettingsSnapshot snapshot = ExportSettingsSnapshot.FromFanuc(fanuc);
            snapshot.SchemaVersion = CurrentSchemaVersion;

            string path = SettingsFilePath;
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(snapshot, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch
        {
            // Ignore persistence failures; export still works for the current session.
        }
    }
}
