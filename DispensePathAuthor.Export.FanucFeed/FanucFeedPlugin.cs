using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Export.FanucFeed;

public sealed class FanucFeedPlugin : IPluginEntry
{
    public void Register(IExporterCatalog catalog) => catalog.Add(new FanucFeedExporter());
}
