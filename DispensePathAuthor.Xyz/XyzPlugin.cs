using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Xyz;

public sealed class XyzPlugin : IPluginEntry
{
    public void Register(IExporterCatalog catalog) => catalog.Add(new XyzExporter());
}
