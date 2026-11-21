using System.Text.Json;
using System.Text.Json.Serialization;

namespace DispensePathAuthor.Models;

public sealed class FanucLsSettings
{
    public ExportCoordinateMode CoordinateMode { get; set; } = ExportCoordinateMode.OffsetFromCalibrationRef;

    public bool UseFovCenterNeedleOffset { get; set; }

    public ExportLocalRotation LocalRotation { get; set; } = ExportLocalRotation.Degree180;

    public bool UseNeedleOffset { get; set; }

    public double NeedleOffsetX_mm { get; set; }

    public double NeedleOffsetY_mm { get; set; }

    public double NeedleOffsetR_deg { get; set; }

    public int? UserFrame { get; set; }

    public int? UserTool { get; set; }

    public FanucLsSettings Clone() =>
        new()
        {
            CoordinateMode = CoordinateMode,
            UseFovCenterNeedleOffset = UseFovCenterNeedleOffset,
            LocalRotation = LocalRotation,
            UseNeedleOffset = UseNeedleOffset,
            NeedleOffsetX_mm = NeedleOffsetX_mm,
            NeedleOffsetY_mm = NeedleOffsetY_mm,
            NeedleOffsetR_deg = NeedleOffsetR_deg,
            UserFrame = UserFrame,
            UserTool = UserTool
        };
}

public sealed class ExporterSettings
{
    [JsonPropertyName("fanuc-ls")]
    public FanucLsSettings FanucLs { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }

    public JsonElement Section(string id) =>
        Extra != null && Extra.TryGetValue(id, out JsonElement section) ? section.Clone() : default;

    public void SetSection(string id, JsonElement section)
    {
        if (id == "fanuc-ls")
        {
            return;
        }

        Extra ??= new Dictionary<string, JsonElement>();
        Extra[id] = section.Clone();
    }

    public ExporterSettings Clone() =>
        new()
        {
            FanucLs = FanucLs.Clone(),
            Extra = CloneExtra(Extra)
        };

    public static Dictionary<string, JsonElement>? CloneExtra(Dictionary<string, JsonElement>? extra) =>
        extra?.ToDictionary(pair => pair.Key, pair => pair.Value.Clone());
}
