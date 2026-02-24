using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;
using ModalCalendarNotification.Core.Features.DismissedEventsManagement;
using ModalCalendarNotification.Core.Features.NotificationManagement;
using TimeProvider = ModalCalendarNotification.Core.Shared.Utilities.TimeProvider;

namespace ModalCalendarNotification.UI.Features.NotificationManagement;

public partial class NotificationModalViewModel : ObservableObject
{
    private readonly IDismissedEventTitleRepository? _dismissedEventTitleRepository;
    private readonly IConfigurationService? _configurationService;
    private readonly TimeProvider? _timeProvider;
    private Timer? _autoDismissTimer;
    private DateTimeOffset? _earliestEventStartTime;

    [ObservableProperty]
    private ObservableCollection<NotificationEventItem> _events = [];

    [ObservableProperty]
    private bool _isClosed;

    [ObservableProperty]
    private string? _selectedEventTitle;

    [ObservableProperty]
    private int _snoozeMinutes = 5;

    [ObservableProperty]
    private string _title = "Upcoming Events";

    public NotificationModalViewModel() { }

    public NotificationModalViewModel(IDismissedEventTitleRepository dismissedEventTitleRepository)
    {
        _dismissedEventTitleRepository = dismissedEventTitleRepository;
    }

    public NotificationModalViewModel(
        IDismissedEventTitleRepository dismissedEventTitleRepository,
        IConfigurationService configurationService,
        TimeProvider timeProvider
    )
    {
        _dismissedEventTitleRepository = dismissedEventTitleRepository;
        _configurationService = configurationService;
        _timeProvider = timeProvider;
    }

    partial void OnEventsChanged(ObservableCollection<NotificationEventItem> value)
    {
        SelectedEventTitle = value.FirstOrDefault()?.Title;

        // Track earliest event start time for auto-dismiss calculation
        if (value.Count > 0)
        {
            _earliestEventStartTime = value.Min(e => e.StartUtc);
            ScheduleAutoDismissAsync();
        }
        else
        {
            StopAutoDismissTimer();
        }
    }

    /// <summary>
    /// Groups events that start within 5 minutes of each other
    /// </summary>
    public void GroupEventsByTime(IReadOnlyList<NotificationEventItem> allEvents)
    {
        Events.Clear();

        if (allEvents.Count == 0)
        {
            return;
        }

        // Group events within 5-minute window
        var orderedEvents = allEvents.OrderBy(e => e.StartUtc).ToList();
        var groupedEvents = new List<List<NotificationEventItem>>();
        var currentGroup = new List<NotificationEventItem> { orderedEvents[0] };

        for (int i = 1; i < orderedEvents.Count; i++)
        {
            var timeDiff = orderedEvents[i].StartUtc - orderedEvents[i - 1].StartUtc;
            if (timeDiff.TotalMinutes <= 5)
            {
                currentGroup.Add(orderedEvents[i]);
            }
            else
            {
                groupedEvents.Add(currentGroup);
                currentGroup = new List<NotificationEventItem> { orderedEvents[i] };
            }
        }

        groupedEvents.Add(currentGroup);

        // For now, display first group. In multi-modal system, would show all groups
        foreach (var evt in groupedEvents[0])
        {
            Events.Add(evt);
        }

        // Update title to show event count
        Title = Events.Count > 1 ? $"Upcoming Events ({Events.Count})" : "Upcoming Events";
    }

    /// <summary>
    /// Schedule auto-dismiss based on configuration and earliest event start time
    /// </summary>
    private async void ScheduleAutoDismissAsync()
    {
        StopAutoDismissTimer();

        if (_configurationService == null || _timeProvider == null || _earliestEventStartTime == null)
        {
            return;
        }

        try
        {
            // Get auto-dismiss timeout in seconds from configuration
            var config = await _configurationService.LoadAsync();
            var autoDismissSeconds = config.AutoDismissTimeoutSeconds;

            // Calculate when to auto-dismiss: event start time + timeout
            var autoDismissTime = _earliestEventStartTime.Value.AddSeconds(autoDismissSeconds);
            var now = _timeProvider.UtcNow;
            var delayMilliseconds = (long)(autoDismissTime - now).TotalMilliseconds;

            // Only schedule if there's still time remaining
            if (delayMilliseconds > 0)
            {
                _autoDismissTimer = new Timer(
                    _ => AutoDismissNotifications(),
                    state: null,
                    dueTime: delayMilliseconds,
                    period: Timeout.Infinite
                );
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash - auto-dismiss is non-critical
            Debug.WriteLine($"Failed to schedule auto-dismiss: {ex.Message}");
        }
    }

    /// <summary>
    /// Called when auto-dismiss timer fires - closes modal without persisting dismissal
    /// </summary>
    private void AutoDismissNotifications()
    {
        StopAutoDismissTimer();
        IsClosed = true;
    }

    [RelayCommand]
    private void Dismiss()
    {
        StopAutoDismissTimer();
        IsClosed = true;
    }

    [RelayCommand]
    private void Snooze()
    {
        StopAutoDismissTimer();
        IsClosed = true;
    }

    [RelayCommand]
    private async Task DismissSingleEventAsync(NotificationEventItem? eventItem)
    {
        if (eventItem != null)
        {
            Events.Remove(eventItem);

            // If all events dismissed, close modal
            if (Events.Count == 0)
            {
                StopAutoDismissTimer();
                IsClosed = true;
            }
        }
    }

    [RelayCommand]
    private async Task SnoozeSingleEventAsync(NotificationEventItem? eventItem)
    {
        if (eventItem != null)
        {
            Events.Remove(eventItem);

            // If all events snoozed, close modal
            if (Events.Count == 0)
            {
                StopAutoDismissTimer();
                IsClosed = true;
            }
        }
    }

    [RelayCommand]
    private async Task DismissAllFutureAsync(NotificationEventItem? eventItem = null)
    {
        StopAutoDismissTimer();

        // Get the title from the parameter event item or selected event
        var titleToDismiss = eventItem?.Title ?? SelectedEventTitle;

        if (
            !string.IsNullOrWhiteSpace(titleToDismiss)
            && _dismissedEventTitleRepository is not null
        )
        {
            await _dismissedEventTitleRepository.AddAsync(titleToDismiss);
        }

        // Remove all events with this title from the modal
        foreach (var evt in Events.Where(e => e.Title == titleToDismiss).ToList())
        {
            Events.Remove(evt);
        }

        // Close if no events remain
        if (Events.Count == 0)
        {
            IsClosed = true;
        }
    }

    private void StopAutoDismissTimer()
    {
        _autoDismissTimer?.Dispose();
        _autoDismissTimer = null;
    }

    public void Cleanup()
    {
        StopAutoDismissTimer();
    }
}





