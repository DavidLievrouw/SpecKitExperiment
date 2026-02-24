using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Core.Features.CalendarIntegration;

/// <summary>
/// Manages cached calendar events for offline support.
/// Events are cached when sync succeeds and used when connection fails.
/// </summary>
public interface ICachedEventsRepository
{
    /// <summary>
    /// Save events to cache.
    /// </summary>
    Task CacheEventsAsync(IReadOnlyList<CalendarEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Load cached events within the next N days.
    /// </summary>
    Task<IReadOnlyList<CalendarEvent>> GetCachedEventsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Clear old cached events (older than 14 days).
    /// </summary>
    Task ClearOldEventsAsync(CancellationToken cancellationToken = default);
}

