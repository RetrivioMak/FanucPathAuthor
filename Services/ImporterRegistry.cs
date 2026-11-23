using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Services;

public static class ImporterRegistry
{
    private static readonly List<IGeometryImporter> Registered = new();
    private static bool _pluginsLoaded;

    public static IReadOnlyList<IGeometryImporter> Importers => Registered;

    public static IReadOnlyList<string> LoadPlugins()
    {
        if (_pluginsLoaded)
        {
            return [];
        }

        _pluginsLoaded = true;
        string folder = Path.Combine(AppContext.BaseDirectory, "plugins", "import");
        return PluginLoader.LoadImporters(folder, new Catalog(Registered));
    }

    private sealed class Catalog(List<IGeometryImporter> importers) : IImporterCatalog
    {
        public void Add(IGeometryImporter importer)
        {
            if (importers.Any(existing => existing.Id == importer.Id))
            {
                return;
            }

            importers.Add(importer);
        }
    }
}
