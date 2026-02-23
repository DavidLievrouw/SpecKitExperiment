using ModalCalendarNotification.CalendarProviders;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Tests.Unit.Fakes;

public sealed class FakeCalendarProvider : ICalendarProvider
{
    private readonly IReadOnlyList<CalendarEvent> _events;

    public FakeCalendarProvider(string providerName, IReadOnlyList<CalendarEvent> events)
    {
        ProviderName = providerName;
        _events = events;
    }

    public string ProviderName { get; }

    public Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default
    )
    {
        var filtered = _events.Where(x => x.StartUtc >= fromUtc && x.StartUtc <= toUtc).ToList();

        return Task.FromResult<IReadOnlyList<CalendarEvent>>(filtered);
    }
}
