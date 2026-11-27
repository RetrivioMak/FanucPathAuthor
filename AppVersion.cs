using System.Reflection;

namespace DispensePathAuthor;

public static class AppVersion
{
    public const string ProductName = "Dispense Path Author";

    /// <summary>Semantic version, e.g. 1.0.7.</summary>
    public static string Version =>
        typeof(AppVersion).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

    /// <summary>Display version including pre-release label, e.g. 1.0.7 Alpha.</summary>
    public static string Informational =>
        typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? Version;

    public static string WindowTitle => $"{ProductName} {Informational}";
}
