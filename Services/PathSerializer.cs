using System.Text.Json;
using System.Text.Json.Serialization;
using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class PathSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new ExportCoordinateModeJsonConverter(),
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static void Save(PathDocument document, string filePath)
    {
        string json = JsonSerializer.Serialize(document, Options);
        File.WriteAllText(filePath, json);
    }

    public static PathDocument Load(string filePath)
    {
        string json = File.ReadAllText(filePath);
        using JsonDocument jsonDocument = JsonDocument.Parse(json);
        PathDocument? document = JsonSerializer.Deserialize<PathDocument>(json, Options);
        document ??= new PathDocument();
        if (jsonDocument.RootElement.TryGetProperty("defaults", out JsonElement defaultsElement))
        {
            ExportSettingsLegacyMigration.ApplyLocalRotation(defaultsElement, document.Defaults);
        }

        PathDocumentMigration.Migrate(document);
        return document;
    }
}
