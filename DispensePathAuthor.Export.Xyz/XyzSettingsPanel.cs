using System.Text.Json;
using System.Text.Json.Nodes;
using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Export.Xyz;

public partial class XyzSettingsPanel : UserControl, IExporterSettingsPage
{
    public XyzSettingsPanel()
    {
        InitializeComponent();
        _zHeight.Value = 0;
        _valveOutput.Value = 1;
        _chordTolerance.Value = 0.2m;
    }

    public void LoadSection(JsonElement section)
    {
        XyzSettings settings = XyzSettings.Read(section);
        _zHeight.Value = Clamp(_zHeight, (decimal)settings.Z_mm);
        _valveOutput.Value = Clamp(_valveOutput, settings.ValveOutput);
        _chordTolerance.Value = Clamp(_chordTolerance, (decimal)settings.ChordTolerance_mm);
    }

    public JsonElement SaveSection()
    {
        var section = new JsonObject
        {
            ["z_mm"] = (double)_zHeight.Value,
            ["valveOutput"] = (int)_valveOutput.Value,
            ["chordTolerance_mm"] = (double)_chordTolerance.Value
        };
        return JsonSerializer.Deserialize<JsonElement>(section.ToJsonString());
    }

    private static decimal Clamp(NumericUpDown control, decimal value) =>
        Math.Max(control.Minimum, Math.Min(control.Maximum, value));
}
