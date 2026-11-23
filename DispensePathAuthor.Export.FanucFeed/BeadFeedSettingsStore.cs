using System.Text.Json;

namespace DispensePathAuthor.Export.FanucFeed;

internal static class BeadFeedSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string FilePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FanucPathAuthor",
            "fanuc-feed.json");

    public static BeadFeedSettings Load()
    {
        try
        {
            string path = FilePath;
            if (!File.Exists(path))
            {
                return BeadFeedSettings.CreateDefault();
            }

            BeadFeedSettings? settings = JsonSerializer.Deserialize<BeadFeedSettings>(File.ReadAllText(path), JsonOptions);
            return settings ?? BeadFeedSettings.CreateDefault();
        }
        catch
        {
            return BeadFeedSettings.CreateDefault();
        }
    }

    public static void Save(BeadFeedSettings settings)
    {
        try
        {
            string path = FilePath;
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, JsonSerializer.Serialize(settings, JsonOptions));
        }
        catch
        {
            // The export still uses the values from this dialog.
        }
    }
}
