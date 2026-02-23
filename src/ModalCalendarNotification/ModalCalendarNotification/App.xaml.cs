using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ModalCalendarNotification;

public partial class App : Application
{
    private ApplicationLifecycleManager? _lifecycleManager;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var serviceProvider = Program.CreateServiceProvider();
            _lifecycleManager = serviceProvider.GetRequiredService<ApplicationLifecycleManager>();
            _lifecycleManager.Start();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Application startup failed");
            Current.Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _lifecycleManager?.Stop();
        Program.ReleaseSingleInstanceLock();
        base.OnExit(e);
    }
}
