using System.Windows.Forms;

namespace CariTakip;

internal static class ApplicationConfiguration
{
    internal static void Initialize()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
    }
}
