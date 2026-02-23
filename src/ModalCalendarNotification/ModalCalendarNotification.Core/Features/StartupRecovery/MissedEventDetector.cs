using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Core.Features.StartupRecovery;

public sealed class MissedEventDetector
{
    public IReadOnlyList<CalendarEvent> DetectMissedEvents(
        IReadOnlyList<CalendarEvent> events,
        DateTimeOffset? lastRunUtc,
        DateTimeOffset nowUtc
    )
    {
        DateTimeOffset lookbackFloor = nowUtc.AddHours(-24);
        DateTimeOffset effectiveStart =
            lastRunUtc > lookbackFloor ? lastRunUtc.Value : lookbackFloor;

        return events
            .Where(x => x.StartUtc > effectiveStart && x.StartUtc <= nowUtc)
            .OrderBy(x => x.StartUtc)
            .ToList();
    }
}
