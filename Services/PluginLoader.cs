using System.Reflection;
using System.Runtime.Loader;
using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Services;

public static class PluginLoader
{
    private static readonly object Gate = new();
    private static readonly List<string> ProbeDirectories = new();
    private static bool _resolverReady;

    public static IReadOnlyList<string> Load(string folder, IExporterCatalog catalog) =>
        LoadFolder(folder, type =>
        {
            if (type.IsAbstract || !typeof(IPluginEntry).IsAssignableFrom(type))
            {
                return;
            }

            if (Activator.CreateInstance(type) is IPluginEntry entry)
            {
                entry.Register(catalog);
            }
        });

    public static IReadOnlyList<string> LoadImporters(string folder, IImporterCatalog catalog) =>
        LoadFolder(folder, type =>
        {
            if (type.IsAbstract || !typeof(IImportPluginEntry).IsAssignableFrom(type))
            {
                return;
            }

            if (Activator.CreateInstance(type) is IImportPluginEntry entry)
            {
                entry.Register(catalog);
            }
        });

    private static IReadOnlyList<string> LoadFolder(string folder, Action<Type> visit)
    {
        if (!Directory.Exists(folder))
        {
            return [];
        }

        EnsureResolver(folder);
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
                Assembly assembly = LoadAssembly(file);
                foreach (Type type in assembly.GetExportedTypes())
                {
                    visit(type);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                string detail = ex.LoaderExceptions.FirstOrDefault(static error => error != null)?.Message ?? ex.Message;
                notes.Add($"{name}: {detail}");
            }
            catch (Exception ex)
            {
                notes.Add($"{name}: {ex.Message}");
            }
        }

        return notes;
    }

    private static Assembly LoadAssembly(string file)
    {
        AssemblyName assemblyName = AssemblyName.GetAssemblyName(file);
        Assembly? loaded = FindLoaded(assemblyName);
        if (loaded != null)
        {
            return loaded;
        }

        return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(file));
    }

    private static Assembly? FindLoaded(AssemblyName name)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (string.Equals(assembly.GetName().Name, name.Name, StringComparison.OrdinalIgnoreCase))
            {
                return assembly;
            }
        }

        return null;
    }

    private static void EnsureResolver(string folder)
    {
        string full = Path.GetFullPath(folder);
        lock (Gate)
        {
            if (!ProbeDirectories.Contains(full, StringComparer.OrdinalIgnoreCase))
            {
                ProbeDirectories.Add(full);
            }

            if (_resolverReady)
            {
                return;
            }

            AssemblyLoadContext.Default.Resolving += ResolveFromProbe;
            _resolverReady = true;
        }
    }

    private static Assembly? ResolveFromProbe(AssemblyLoadContext context, AssemblyName name)
    {
        if (string.IsNullOrEmpty(name.Name))
        {
            return null;
        }

        string[] directories;
        lock (Gate)
        {
            directories = ProbeDirectories.ToArray();
        }

        foreach (string directory in directories)
        {
            string candidate = Path.Combine(directory, name.Name + ".dll");
            if (!File.Exists(candidate))
            {
                continue;
            }

            return context.LoadFromAssemblyPath(Path.GetFullPath(candidate));
        }

        return null;
    }
}
