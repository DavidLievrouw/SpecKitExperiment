using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;
using ModalCalendarNotification.Core.Shared.Utilities;
using ModalCalendarNotification.UI.Features.ConfigurationManagement;
using ModalCalendarNotification.UI.Features.SystemTrayManagement;
using AppTimeProvider = ModalCalendarNotification.Core.Shared.Utilities.TimeProvider;

namespace ModalCalendarNotification;

public static class ServiceConfiguration
{
    public static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton(configuration);
        services.AddSingleton<AppTimeProvider>();

        // Configuration Management
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<ConfigurationDialogViewModel>();
        services.AddSingleton<ConfigurationDialog>();

        // System Tray
        services.AddSingleton<SystemTrayViewModel>();
        services.AddSingleton<SystemTrayIcon>();
        services.AddSingleton<SystemTrayManager>();
        services.AddSingleton<ApplicationLifecycleManager>();
    }
}
