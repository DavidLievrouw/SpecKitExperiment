using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.CalendarProviders;

public interface ICalendarProvider
{
    string ProviderName { get; }

    Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default);
}
