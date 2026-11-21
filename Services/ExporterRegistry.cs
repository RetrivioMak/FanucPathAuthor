using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Services;

public static class ExporterRegistry
{
    private static readonly List<IProgramExporter> Registered = [new FanucLsExporter()];
    private static bool _pluginsLoaded;

    public static IReadOnlyList<IProgramExporter> Exporters => Registered;

    public static IReadOnlyList<string> LoadPlugins()
    {
        if (_pluginsLoaded)
        {
            return [];
        }

        _pluginsLoaded = true;
        string folder = Path.Combine(AppContext.BaseDirectory, "plugins");
        return PluginLoader.Load(folder, new Catalog(Registered));
    }

    private sealed class Catalog(List<IProgramExporter> exporters) : IExporterCatalog
    {
        public void Add(IProgramExporter exporter)
        {
            if (exporters.Any(existing => existing.Id == exporter.Id))
            {
                return;
            }

            exporters.Add(exporter);
        }
    }
}
