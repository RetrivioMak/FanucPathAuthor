using System.Text.Json;
using System.Text.Json.Nodes;
using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Export.Xyz;

public sealed partial class XyzSettingsPanel : Form, IExporterSettingsPage
{
    public XyzSettingsPanel()
    {
        InitializeComponent();
    }

    public void LoadSection(JsonElement section)
    {
        XyzSettings settings = XyzSettings.Read(section);
        _zHeight.Value = Clamp(_zHeight, (decimal)settings.Z_mm);
        _valveOutput.Value = Clamp(_valveOutput, (decimal)settings.ValveOutput);
        _pointSpacing.Value = Clamp(_pointSpacing, (decimal)settings.PointSpacing_mm);
        _chordTolerance.Value = Clamp(_chordTolerance, (decimal)settings.ChordTolerance_mm);
    }

    public JsonElement SaveSection()
    {
        var section = new JsonObject
        {
            ["z_mm"] = (double)_zHeight.Value,
            ["valveOutput"] = (int)_valveOutput.Value,
            ["pointSpacing_mm"] = (double)_pointSpacing.Value,
            ["chordTolerance_mm"] = (double)_chordTolerance.Value
        };
        return JsonSerializer.Deserialize<JsonElement>(section.ToJsonString());
    }

    private static decimal Clamp(NumericUpDown control, decimal value) =>
        Math.Max(control.Minimum, Math.Min(control.Maximum, value));
}
