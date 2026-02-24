using ModalCalendarNotification.CalendarProviders;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;
using ModalCalendarNotification.Core.Features.StartupRecovery;
using ModalCalendarNotification.Features.NotificationManagement;
using ModalCalendarNotification.UI.Features.SystemTrayManagement;
using ModalCalendarNotification.UI.Features.StartupRecovery;
using TimeProvider = ModalCalendarNotification.Core.Shared.Utilities.TimeProvider;

namespace ModalCalendarNotification;

public sealed class ApplicationLifecycleManager : IDisposable
{
    private readonly object _sync = new();
    private readonly SystemTrayManager _systemTrayManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly NotificationSchedulerService _notificationScheduler;
    private bool _disposed;

    public ApplicationLifecycleManager(
        SystemTrayManager systemTrayManager,
        IServiceProvider serviceProvider,
        NotificationSchedulerService notificationScheduler
    )
    {
        _systemTrayManager =
            systemTrayManager ?? throw new ArgumentNullException(nameof(systemTrayManager));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _notificationScheduler =
            notificationScheduler ?? throw new ArgumentNullException(nameof(notificationScheduler));
    }

    public bool IsRunning { get; private set; }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _systemTrayManager?.Dispose();
            _notificationScheduler?.Dispose();
            _disposed = true;
        }
    }

    public async void Start()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ApplicationLifecycleManager));
            }

            IsRunning = true;
            _systemTrayManager.Initialize();
        }

        // Start notification scheduler to monitor events
        await _notificationScheduler.StartSchedulerAsync();

        // Check for missed events on startup (non-blocking)
        _ = CheckForMissedEventsAsync();
    }

    private async Task CheckForMissedEventsAsync()
    {
        try
        {
            // Get required services
            var recoveryService = _serviceProvider.GetService(
                typeof(MissedEventRecoveryService)
            ) as MissedEventRecoveryService;

            var timeProvider = _serviceProvider.GetService(typeof(TimeProvider)) as TimeProvider;
            var configService = _serviceProvider.GetService(
                typeof(IConfigurationService)
            ) as IConfigurationService;

            var outlookProvider = _serviceProvider.GetService(
                typeof(OutlookCalendarProvider)
            ) as OutlookCalendarProvider;

            var googleProvider = _serviceProvider.GetService(
                typeof(GoogleCalendarProvider)
            ) as GoogleCalendarProvider;

            if (
                recoveryService == null
                || timeProvider == null
                || configService == null
                || (outlookProvider == null && googleProvider == null)
            )
            {
                return;
            }

            // Get configuration to check if we have any configured providers
            var config = await configService.LoadAsync();
            if (config.ProviderAccounts.Count == 0)
            {
                // No providers configured, skip missed event detection
                return;
            }

            // Collect events from all configured providers
            var allEvents = new List<Core.Shared.Models.CalendarEvent>();
            var now = timeProvider.UtcNow;
            var lookback = now.AddHours(-24);

            // Fetch from Outlook365 if configured
            if (
                config.ProviderAccounts.Any(p => p.ProviderName == "Outlook365")
                && outlookProvider != null
            )
            {
                try
                {
                    var outlookEvents = await outlookProvider.GetEventsAsync(lookback, now.AddHours(1));
                    allEvents.AddRange(outlookEvents);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to fetch Outlook events: {ex.Message}"
                    );
                }
            }

            // Fetch from Google Calendar if configured
            if (
                config.ProviderAccounts.Any(p => p.ProviderName == "GoogleCalendar")
                && googleProvider != null
            )
            {
                try
                {
                    var googleEvents = await googleProvider.GetEventsAsync(lookback, now.AddHours(1));
                    allEvents.AddRange(googleEvents);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to fetch Google Calendar events: {ex.Message}"
                    );
                }
            }

            if (allEvents.Count == 0)
            {
                return;
            }

            // Detect missed events using the recovery service
            var missedEvents = await recoveryService.RecoverMissedEventsAsync(allEvents, now);

            if (missedEvents.Count == 0)
            {
                return;
            }

            // Show missed events modal
            ShowMissedEventsModal(missedEvents);
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Failed to check for missed events: {ex.Message}");
        }
    }

    private void ShowMissedEventsModal(IReadOnlyList<Core.Shared.Models.CalendarEvent> missedEvents)
    {
        try
        {
            if (
                _serviceProvider.GetService(typeof(StartupMissedEventsViewModel))
                    is not StartupMissedEventsViewModel viewModel
            )
            {
                return;
            }

            // Set the missed events in the view model
            viewModel.MissedEvents = missedEvents;

            // Create and show the modal dialog
            var modal = new StartupMissedEventsModal(viewModel)
            {
                Owner = null, // No owner for startup modal
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
            };

            // Show as modal (blocking until closed)
            _ = modal.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to show missed events modal: {ex.Message}");
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            IsRunning = false;
            _notificationScheduler.StopScheduler();
        }
    }
}
