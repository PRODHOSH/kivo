using System.Windows;
using System.IO;

namespace Kivo;

public partial class App : Application
{
    public App()
    {
        this.DispatcherUnhandledException += (s, e) =>
        {
            File.WriteAllText("error.log", e.Exception.ToString());
            e.Handled = true;
        };
    }
}
