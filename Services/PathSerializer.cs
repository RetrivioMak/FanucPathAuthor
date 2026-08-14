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
        PathDocument? document = JsonSerializer.Deserialize<PathDocument>(json, Options);
        document ??= new PathDocument();
        PathDocumentMigration.Migrate(document);
        return document;
    }
}
