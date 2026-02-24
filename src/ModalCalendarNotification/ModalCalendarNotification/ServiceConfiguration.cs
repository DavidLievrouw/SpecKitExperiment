using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModalCalendarNotification.CalendarProviders;
using ModalCalendarNotification.Core.Features.Authentication;
using ModalCalendarNotification.Core.Features.CalendarIntegration;
using ModalCalendarNotification.Core.Features.CalendarSelection;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;
using ModalCalendarNotification.Core.Features.DismissedEventsManagement;
using ModalCalendarNotification.Core.Features.NotificationManagement;
using ModalCalendarNotification.Core.Features.StartupRecovery;
using ModalCalendarNotification.Data.Features.CalendarIntegration;
using ModalCalendarNotification.Data.Features.CalendarSelection;
using ModalCalendarNotification.Data.Features.DismissedEventsManagement;
using ModalCalendarNotification.Data.Features.StartupRecovery;
using ModalCalendarNotification.Features.CalendarIntegration;
using ModalCalendarNotification.Features.NotificationManagement;
using ModalCalendarNotification.UI.Features.CalendarSelection;
using ModalCalendarNotification.UI.Features.ConfigurationManagement;
using ModalCalendarNotification.UI.Features.NotificationManagement;
using ModalCalendarNotification.UI.Features.StartupRecovery;
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
        services.AddSingleton(sp =>
            new ConfigurationDialogViewModel(
                sp.GetRequiredService<IConfigurationService>(),
                sp.GetRequiredService<IDismissedEventTitleRepository>(),
                sp.GetRequiredService<ISyncStatusService>()
            )
        );
        services.AddSingleton<ConfigurationDialog>();
        services.AddTransient<ProviderSelectionViewModel>();
        services.AddTransient<ProviderSelectionDialog>();

        // Calendar Integration
        services.AddSingleton<ICalendarProviderFactory, CalendarProviderFactory>();
        services.AddSingleton<ISyncStatusService, SyncStatusService>();

        // Calendar Selection
        services.AddTransient<CalendarListViewModel>();
        services.AddTransient<CalendarListDialog>();
        services.AddScoped<ICalendarSelectionRepository, CalendarSelectionRepository>();
        services.AddScoped<ICachedEventsRepository, CachedEventsRepository>();

        // Dismissed Events Management
        services.AddScoped<IDismissedEventTitleRepository, DismissedEventTitleRepository>();

        // Startup Recovery
        services.AddScoped<IApplicationStateRepository, ApplicationStateRepository>();
        services.AddScoped<MissedEventDetector>();
        services.AddScoped<MissedEventRecoveryService>();
        services.AddTransient<StartupMissedEventsViewModel>();
        services.AddTransient<StartupMissedEventsModal>();

        // Notification Management
        services.AddSingleton<INotificationEngine, NotificationEngine>(sp =>
            new NotificationEngine(
                sp.GetRequiredService<IDismissedEventTitleRepository>(),
                sp.GetRequiredService<ICalendarSelectionRepository>()
            )
        );

        services.AddSingleton<NotificationSchedulerService>(sp =>
            new NotificationSchedulerService(
                new ICalendarProvider[]
                {
                    sp.GetRequiredService<OutlookCalendarProvider>(),
                    sp.GetRequiredService<GoogleCalendarProvider>(),
                },
                sp.GetRequiredService<IConfigurationService>(),
                sp.GetRequiredService<INotificationEngine>(),
                sp.GetRequiredService<AppTimeProvider>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger>(),
                sp.GetRequiredService<ICachedEventsRepository>(),
                sp.GetRequiredService<ISyncStatusService>()
            )
        );

        services.AddTransient(sp =>
            new NotificationModalViewModel(
                sp.GetRequiredService<IDismissedEventTitleRepository>(),
                sp.GetRequiredService<IConfigurationService>(),
                sp.GetRequiredService<AppTimeProvider>()
            )
        );

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
