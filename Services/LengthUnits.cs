using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public static class LengthUnits
{
    public static double ToMillimeters(double value, LengthUnit unit) =>
        unit switch
        {
            LengthUnit.Micrometer => value / 1000.0,
            LengthUnit.Centimeter => value * 10.0,
            LengthUnit.Meter => value * 1000.0,
            _ => value
        };

    public static string Label(LengthUnit unit) =>
        unit switch
        {
            LengthUnit.Micrometer => "µm",
            LengthUnit.Centimeter => "cm",
            LengthUnit.Meter => "m",
            _ => "mm"
        };
}
