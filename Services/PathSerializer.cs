using System.Text.Json;
using System.Text.Json.Serialization;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

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

    public static string ToJson(PathDocument document) =>
        JsonSerializer.Serialize(document, Options);

    public static void Save(PathDocument document, string filePath) =>
        File.WriteAllText(filePath, ToJson(document));

    public static PathDocument Load(string filePath) =>
        LoadFromJson(File.ReadAllText(filePath));

    public static PathDocument LoadFromJson(string json)
    {
        using JsonDocument jsonDocument = JsonDocument.Parse(json);
        PathDocument? document = JsonSerializer.Deserialize<PathDocument>(json, Options);
        document ??= new PathDocument();
        bool workspaceSpecified = jsonDocument.RootElement.TryGetProperty("workspace", out _);
        LiftLegacyFanucSettings(jsonDocument.RootElement, document);
        if (jsonDocument.RootElement.TryGetProperty("defaults", out JsonElement defaultsElement))
        {
            ExportSettingsLegacyMigration.ApplyLocalRotation(defaultsElement, document.Exporters.FanucLs);
        }

        PathDocumentMigration.Migrate(document);
        if (!workspaceSpecified)
        {
            document.Workspace = WorkspaceKind.Cv;
            document.SchemaVersion = Math.Max(document.SchemaVersion, 4);
        }

        return document;
    }

    private static void LiftLegacyFanucSettings(JsonElement root, PathDocument document)
    {
        if (root.TryGetProperty("exporters", out JsonElement exporters) &&
            exporters.ValueKind == JsonValueKind.Object &&
            exporters.TryGetProperty("fanuc-ls", out _))
        {
            return;
        }

        if (!root.TryGetProperty("defaults", out JsonElement defaults) ||
            defaults.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        FanucLsSettings? lifted = JsonSerializer.Deserialize<FanucLsSettings>(defaults.GetRawText(), Options);
        if (lifted != null)
        {
            document.Exporters.FanucLs = lifted;
        }
    }
}
