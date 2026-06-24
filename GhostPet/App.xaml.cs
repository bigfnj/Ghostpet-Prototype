using System.Threading;
using System.Windows;

namespace GhostPet;

public partial class App : Application
{
    private Mutex? _mutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, @"Global\GhostPet-SingleInstance-A1B2C3D4", out bool isNew);
        if (!isNew)
        {
            // Another instance is already running — exit silently
            _mutex.Dispose();
            _mutex = null;
            Shutdown();
            return;
        }
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
