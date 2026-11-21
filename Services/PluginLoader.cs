using System.Reflection;
using System.Runtime.Loader;
using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Services;

public static class PluginLoader
{
    public static IReadOnlyList<string> Load(string folder, IExporterCatalog catalog)
    {
        if (!Directory.Exists(folder))
        {
            return [];
        }

        var notes = new List<string>();
        foreach (string file in Directory.GetFiles(folder, "*.dll").OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            string name = Path.GetFileName(file);
            if (name.Equals("DispensePathAuthor.dll", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("DispensePathAuthor.Contracts.dll", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(file));
                foreach (Type type in assembly.GetExportedTypes())
                {
                    if (type.IsAbstract || !typeof(IPluginEntry).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    if (Activator.CreateInstance(type) is not IPluginEntry entry)
                    {
                        continue;
                    }

                    entry.Register(catalog);
                }
            }
            catch (Exception ex)
            {
                notes.Add($"{name}: {ex.Message}");
            }
        }

        return notes;
    }
}
