using DispensePathAuthor.Services;

namespace DispensePathAuthor;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        IReadOnlyList<string> pluginNotes = ExporterRegistry.LoadPlugins();
        Application.Run(new MainForm(pluginNotes));
    }
}
