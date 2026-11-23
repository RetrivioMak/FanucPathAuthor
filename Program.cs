using DispensePathAuthor.Services;

namespace DispensePathAuthor;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var pluginNotes = new List<string>();
        pluginNotes.AddRange(ExporterRegistry.LoadPlugins());
        pluginNotes.AddRange(ImporterRegistry.LoadPlugins());
        Application.Run(new MainForm(pluginNotes));
    }
}
