using DispensePathAuthor.Controls;
using DispensePathAuthor.Models;
using DispensePathAuthor.Services;

namespace DispensePathAuthor;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        ProjectStart start;
        using (StartupDialog startup = new StartupDialog())
        {
            if (startup.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            start = new ProjectStart(startup.Action, startup.FilePath);
        }

        Application.Run(new MainForm(start));
    }
}
