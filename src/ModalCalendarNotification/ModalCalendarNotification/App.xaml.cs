using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ModalCalendarNotification.UI.Features.ConfigurationManagement;
using ModalCalendarNotification.UI.Features.SystemTrayManagement;
using Serilog;

namespace ModalCalendarNotification;

public partial class App : Application
{
    private ApplicationLifecycleManager? _lifecycleManager;
    private IServiceProvider? _serviceProvider;
    private SystemTrayViewModel? _systemTrayViewModel;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _serviceProvider = Program.CreateServiceProvider();
            _lifecycleManager = _serviceProvider.GetRequiredService<ApplicationLifecycleManager>();
            _systemTrayViewModel = _serviceProvider.GetRequiredService<SystemTrayViewModel>();

            // Subscribe to ViewModel state changes
            _systemTrayViewModel.PropertyChanged += SystemTrayViewModel_PropertyChanged;

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
        if (_systemTrayViewModel != null)
        {
            _systemTrayViewModel.PropertyChanged -= SystemTrayViewModel_PropertyChanged;
        }

        _lifecycleManager?.Stop();
        _lifecycleManager?.Dispose();
        Program.ReleaseSingleInstanceLock();
        base.OnExit(e);
    }

    private void SystemTrayViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (
            e.PropertyName == nameof(SystemTrayViewModel.IsExitRequested)
            && _systemTrayViewModel?.IsExitRequested == true
        )
        {
            Log.Information("Exit requested from system tray");
            Current.Shutdown();
        }
        else if (
            e.PropertyName == nameof(SystemTrayViewModel.IsSettingsRequested)
            && _systemTrayViewModel?.IsSettingsRequested == true
        )
        {
            Log.Information("Settings requested from system tray");
            OpenSettingsDialog();
            _systemTrayViewModel.IsSettingsRequested = false; // Reset the flag
        }
    }

    private void OpenSettingsDialog()
    {
        try
        {
            if (_serviceProvider == null)
            {
                Log.Warning("Service provider not available for opening settings dialog");
                return;
            }

            var configurationDialog = _serviceProvider.GetRequiredService<ConfigurationDialog>();
            configurationDialog.Owner = null; // No owner since we're in system tray
            configurationDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open settings dialog");
        }
    }
}
