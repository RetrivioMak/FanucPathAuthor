using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Export.Fanuc;

public sealed class FanucPlugin : IPluginEntry
{
    public void Register(IExporterCatalog catalog) => catalog.Add(new FanucLsExporter());
}
