using System.Text.Json;
using System.Text.Json.Serialization;
using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class ExportSettingsPersistence
{
    private const int CurrentSchemaVersion = 1;

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

    public static bool TryLoadInto(ProcessDefaults defaults)
    {
        try
        {
            string path = SettingsFilePath;
            if (!File.Exists(path))
            {
                return false;
            }

            string json = File.ReadAllText(path);
            ExportSettingsSnapshot? snapshot =
                JsonSerializer.Deserialize<ExportSettingsSnapshot>(json, JsonOptions);
            if (snapshot == null)
            {
                return false;
            }

            snapshot.ApplyTo(defaults);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void Save(ProcessDefaults defaults)
    {
        try
        {
            ExportSettingsSnapshot snapshot = ExportSettingsSnapshot.FromProcessDefaults(defaults);
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
