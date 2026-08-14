using System.Text.Json;
using System.Text.Json.Serialization;
using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public sealed class ExportCoordinateModeJsonConverter : JsonConverter<ExportCoordinateMode>
{
    public override ExportCoordinateMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Export coordinate mode must be a string.");
        }

        string? value = reader.GetString();
        return value switch
        {
            "uframeAbsolute" => ExportCoordinateMode.UframeAbsolute,
            "offsetFromFrameOrigin" => ExportCoordinateMode.OffsetFromFrameOrigin,
            "offsetFromCalibrationRef" => ExportCoordinateMode.OffsetFromCalibrationRef,
            "affineAbsolute" => ExportCoordinateMode.UframeAbsolute,
            "referenceRelative" => ExportCoordinateMode.OffsetFromCalibrationRef,
            _ => throw new JsonException($"Unknown export coordinate mode: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ExportCoordinateMode value, JsonSerializerOptions options)
    {
        string text = value switch
        {
            ExportCoordinateMode.UframeAbsolute => "uframeAbsolute",
            ExportCoordinateMode.OffsetFromFrameOrigin => "offsetFromFrameOrigin",
            ExportCoordinateMode.OffsetFromCalibrationRef => "offsetFromCalibrationRef",
            _ => throw new JsonException($"Unknown export coordinate mode: {value}")
        };

        writer.WriteStringValue(text);
    }
}
