using ModalCalendarNotification.Core.Features.DismissedEventsManagement;
using ModalCalendarNotification.Core.Features.CalendarSelection;
using ModalCalendarNotification.Core.Shared.Models;
using System.Diagnostics;

namespace ModalCalendarNotification.Core.Features.NotificationManagement;

public sealed class NotificationEngine : INotificationEngine
{
    private readonly IDismissedEventTitleRepository? _dismissedEventTitleRepository;
    private readonly ICalendarSelectionRepository? _calendarSelectionRepository;

    public NotificationEngine()
    {
    }

    public NotificationEngine(IDismissedEventTitleRepository dismissedEventTitleRepository)
    {
        _dismissedEventTitleRepository = dismissedEventTitleRepository;
    }

    public NotificationEngine(
        IDismissedEventTitleRepository dismissedEventTitleRepository,
        ICalendarSelectionRepository calendarSelectionRepository)
    {
        _dismissedEventTitleRepository = dismissedEventTitleRepository;
        _calendarSelectionRepository = calendarSelectionRepository;
    }

    public IReadOnlyList<Notification> BuildNotifications(
        IReadOnlyList<CalendarEvent> events,
        DateTimeOffset nowUtc,
        int leadTimeMinutes)
    {
        var stopwatch = Stopwatch.StartNew();

        var dismissedTitles = _dismissedEventTitleRepository is null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : _dismissedEventTitleRepository
                .GetAllAsync()
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Title)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var selectedCalendarKeys = _calendarSelectionRepository is null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : _calendarSelectionRepository
                .GetSelectedAsync()
                .GetAwaiter()
                .GetResult()
                .Where(x => x.IsSelected)
                .Select(x => BuildCalendarKey(x.ProviderName, x.CalendarId))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var enforceCalendarSelection = selectedCalendarKeys.Count > 0;

        var cutoff = nowUtc.AddMinutes(leadTimeMinutes);

        var notifications = events
            .Where(x => !dismissedTitles.Contains(x.Title))
            .Where(x => !enforceCalendarSelection || selectedCalendarKeys.Contains(BuildCalendarKey(x.Provider, x.CalendarId)))
            .Where(x => x.StartUtc >= nowUtc && x.StartUtc <= cutoff)
            .Select(x => new Notification
            {
                Id = Guid.NewGuid().ToString("N"),
                EventId = x.Id,
                Title = x.Title,
                TriggerAtUtc = x.StartUtc.AddMinutes(-leadTimeMinutes),
                SnoozeCount = 0,
                IsDismissed = false
            })
            .OrderBy(x => x.TriggerAtUtc)
            .ToList();

            stopwatch.Stop();
            Debug.WriteLine($"NotificationEngine.BuildNotifications latency: {stopwatch.ElapsedMilliseconds}ms ({notifications.Count} notifications)");

            return notifications;
    }

    private static string BuildCalendarKey(string providerName, string calendarId)
    {
        return $"{providerName}:{calendarId}";
    }
}

