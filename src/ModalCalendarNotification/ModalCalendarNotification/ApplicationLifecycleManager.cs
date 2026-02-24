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
    private readonly NotificationSchedulerService _notificationScheduler;
    private readonly MissedEventRecoveryService _missedEventRecoveryService;
    private readonly IConfigurationService _configurationService;
    private readonly TimeProvider _timeProvider;
    private readonly OutlookCalendarProvider _outlookProvider;
    private readonly GoogleCalendarProvider _googleProvider;
    private readonly Func<StartupMissedEventsViewModel> _createStartupMissedEventsViewModel;
    private bool _disposed;

    public ApplicationLifecycleManager(
        SystemTrayManager systemTrayManager,
        NotificationSchedulerService notificationScheduler,
        MissedEventRecoveryService missedEventRecoveryService,
        IConfigurationService configurationService,
        TimeProvider timeProvider,
        OutlookCalendarProvider outlookProvider,
        GoogleCalendarProvider googleProvider,
        Func<StartupMissedEventsViewModel> createStartupMissedEventsViewModel
    )
    {
        _systemTrayManager =
            systemTrayManager ?? throw new ArgumentNullException(nameof(systemTrayManager));
        _notificationScheduler =
            notificationScheduler ?? throw new ArgumentNullException(nameof(notificationScheduler));
        _missedEventRecoveryService =
            missedEventRecoveryService ?? throw new ArgumentNullException(nameof(missedEventRecoveryService));
        _configurationService =
            configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _outlookProvider = outlookProvider ?? throw new ArgumentNullException(nameof(outlookProvider));
        _googleProvider = googleProvider ?? throw new ArgumentNullException(nameof(googleProvider));
        _createStartupMissedEventsViewModel = createStartupMissedEventsViewModel ??
            throw new ArgumentNullException(nameof(createStartupMissedEventsViewModel));
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

    public void Start()
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

        // Start notification scheduler to monitor events (non-blocking)
        _ = _notificationScheduler.StartSchedulerAsync();

        // Check for missed events on startup (non-blocking)
        _ = CheckForMissedEventsAsync();
    }

    private async Task CheckForMissedEventsAsync()
    {
        try
        {
            // Get configuration to check if we have any configured providers
            var config = await _configurationService.LoadAsync();
            if (config.ProviderAccounts.Count == 0)
            {
                // No providers configured, skip missed event detection
                return;
            }

            // Collect events from all configured providers
            var allEvents = new List<Core.Shared.Models.CalendarEvent>();
            var now = _timeProvider.UtcNow;
            var lookback = now.AddHours(-24);

            // Fetch from Outlook365 if configured
            if (config.ProviderAccounts.Any(p => p.ProviderName == "Outlook365"))
            {
                try
                {
                    var outlookEvents = await _outlookProvider.GetEventsAsync(lookback, now.AddHours(1));
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
            if (config.ProviderAccounts.Any(p => p.ProviderName == "GoogleCalendar"))
            {
                try
                {
                    var googleEvents = await _googleProvider.GetEventsAsync(lookback, now.AddHours(1));
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
            var missedEvents = await _missedEventRecoveryService.RecoverMissedEventsAsync(allEvents, now);

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
            var viewModel = _createStartupMissedEventsViewModel();
            if (viewModel == null)
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
