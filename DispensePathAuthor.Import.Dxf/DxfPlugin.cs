using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Import.Dxf;

public sealed class DxfPlugin : IImportPluginEntry
{
    public void Register(IImporterCatalog catalog) => catalog.Add(new DxfImporter());
}
