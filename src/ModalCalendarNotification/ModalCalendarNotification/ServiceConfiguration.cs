using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModalCalendarNotification.CalendarProviders;
using ModalCalendarNotification.Core.Features.Authentication;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;
using ModalCalendarNotification.UI.Features.ConfigurationManagement;
using ModalCalendarNotification.UI.Features.SystemTrayManagement;
using Serilog;
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

        // Logging
        services.AddSingleton(Log.Logger);
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(Log.Logger, dispose: false);
        });

        // Authentication
        services.AddSingleton<IMsalTokenClient, MsalTokenClient>();
        services.AddSingleton<IAuthenticationService, OAuthService>();

        // Configuration Management
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<ConfigurationDialogViewModel>();
        services.AddSingleton<ConfigurationDialog>();
        services.AddTransient<ProviderSelectionViewModel>();
        services.AddTransient<ProviderSelectionDialog>();

        // Calendar Providers
        services.AddSingleton<OutlookCalendarProvider>();
        services.AddSingleton<GoogleCalendarProvider>();

        // System Tray
        services.AddSingleton<SystemTrayViewModel>();
        services.AddSingleton<SystemTrayIcon>();
        services.AddSingleton<SystemTrayManager>();
        services.AddSingleton<ApplicationLifecycleManager>();
    }
}
