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
        Application.Run(new MainForm());
    }
}
