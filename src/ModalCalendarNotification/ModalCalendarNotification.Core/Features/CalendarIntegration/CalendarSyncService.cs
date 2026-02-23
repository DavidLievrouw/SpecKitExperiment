using System.Diagnostics;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Core.Features.CalendarIntegration;

public sealed class CalendarSyncService
{
    private readonly IReadOnlyList<ICalendarProviderAdapter> _providers;

    public CalendarSyncService(IEnumerable<ICalendarProviderAdapter> providers)
    {
        _providers = providers.ToList();
    }

    public async Task<IReadOnlyList<CalendarEvent>> SyncUpcomingEventsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default
    )
    {
        var allEvents = new List<CalendarEvent>();

        foreach (var provider in _providers)
        {
            try
            {
                var events = await provider.GetEventsAsync(fromUtc, toUtc, cancellationToken);
                allEvents.AddRange(events);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CalendarSyncService provider error: {ex.Message}");
            }
        }

        return allEvents.OrderBy(x => x.StartUtc).ToList();
    }
}

public interface ICalendarProviderAdapter
{
    Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default
    );
}
