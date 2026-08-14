using System.Text.Json;
using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class CalibrationLoader
{
    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static RobotCalibrationFile Load(string filePath)
    {
        string json = File.ReadAllText(filePath);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        if (IsCompactMarkerProfilesFile(root))
        {
            throw new InvalidDataException(
                "CompactMarkerProfiles.json is a profile index, not a calibration transform. " +
                "Load CompactMarker/<CalibrationPoseId>/transform.json instead.");
        }

        RobotCalibrationFile calibration = JsonSerializer.Deserialize<RobotCalibrationFile>(json, DeserializeOptions)
            ?? throw new InvalidDataException("Calibration file is empty or invalid.");

        calibration.Kind = DetectKind(root, filePath);
        NormalizeCalibration(calibration);

        if (!calibration.HasPixelToRobot)
        {
            throw new InvalidDataException("Calibration file has no pixelToRobot coefficients.");
        }

        return calibration;
    }

    public static void ApplyCompactMarkerProcessDefaults(
        RobotCalibrationFile calibration,
        ProcessDefaults defaults)
    {
        if (!calibration.IsCompactMarker)
        {
            return;
        }

        if (calibration.FixedRobotZ_mm is double z)
        {
            defaults.Z_mm = z;
        }

        if (calibration.FixedRobotW_deg is double w)
        {
            defaults.W_deg = w;
        }

        if (calibration.FixedRobotP_deg is double p)
        {
            defaults.P_deg = p;
        }

        if (calibration.FixedRobotR_deg is double r)
        {
            defaults.R_deg = r;
        }
    }

    public static string FormatSummary(RobotCalibrationFile calibration, string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        string frameTool = $"{calibration.RobotFrame}/{calibration.RobotTool}";

        if (calibration.IsCompactMarker)
        {
            string poseId = string.IsNullOrWhiteSpace(calibration.CalibrationPoseId)
                ? fileName
                : calibration.CalibrationPoseId.Trim();
            return $"Calibration: Compact Marker [{poseId}] {frameTool} ({fileName})";
        }

        return $"Calibration: Robot-CV {frameTool} ({fileName})";
    }

    private static bool IsCompactMarkerProfilesFile(JsonElement root) =>
        root.TryGetProperty("profiles", out JsonElement profiles) &&
        profiles.ValueKind == JsonValueKind.Array &&
        !root.TryGetProperty("pixelToRobot", out _);

    private static CalibrationFileKind DetectKind(JsonElement root, string filePath)
    {
        if (HasNonEmptyString(root, "calibrationPoseId"))
        {
            return CalibrationFileKind.CompactMarker;
        }

        if (root.TryGetProperty("rotation", out _) ||
            HasNonEmptyString(root, "crossModelFilePath"))
        {
            return CalibrationFileKind.RobotCvTransform;
        }

        if (root.TryGetProperty("fixedRobotZ_mm", out _) ||
            root.TryGetProperty("fixedRobotW_deg", out _) ||
            root.TryGetProperty("fixedRobotP_deg", out _) ||
            root.TryGetProperty("fixedRobotR_deg", out _))
        {
            return CalibrationFileKind.CompactMarker;
        }

        if (filePath.Contains("CompactMarker", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(Path.GetFileName(filePath), "transform.json", StringComparison.OrdinalIgnoreCase))
        {
            return CalibrationFileKind.CompactMarker;
        }

        return CalibrationFileKind.RobotCvTransform;
    }

    private static void NormalizeCalibration(RobotCalibrationFile calibration)
    {
        if (calibration.IsCompactMarker)
        {
            calibration.ReferenceRobotR_deg ??= calibration.FixedRobotR_deg;
        }
    }

    private static bool HasNonEmptyString(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out JsonElement value) &&
        value.ValueKind == JsonValueKind.String &&
        !string.IsNullOrWhiteSpace(value.GetString());
}
