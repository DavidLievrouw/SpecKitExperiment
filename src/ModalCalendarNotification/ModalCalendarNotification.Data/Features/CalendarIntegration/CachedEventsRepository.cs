using Microsoft.EntityFrameworkCore;
using ModalCalendarNotification.Core.Features.CalendarIntegration;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Data.Features.CalendarIntegration;

public sealed class CachedEventsRepository : ICachedEventsRepository
{
    private readonly AppDbContext _dbContext;

    public CachedEventsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CacheEventsAsync(
        IReadOnlyList<CalendarEvent> events,
        CancellationToken cancellationToken = default
    )
    {
        // Clear old events first
        await ClearOldEventsAsync(cancellationToken);

        // Add new events
        foreach (var evt in events)
        {
            var existingEvent = await _dbContext.CachedCalendarEvents
                .FirstOrDefaultAsync(e => e.EventId == evt.Id && e.Provider == evt.Provider, cancellationToken);

            if (existingEvent == null)
            {
                _dbContext.CachedCalendarEvents.Add(new CachedCalendarEventEntity
                {
                    EventId = evt.Id,
                    Title = evt.Title,
                    Description = evt.Description,
                    StartUtc = evt.StartUtc,
                    EndUtc = evt.EndUtc,
                    Location = evt.Location,
                    Provider = evt.Provider,
                    CalendarId = evt.CalendarId,
                    IsAllDay = evt.IsAllDay,
                    CachedAtUtc = DateTime.UtcNow,
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarEvent>> GetCachedEventsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default
    )
    {
        var cachedEntities = await _dbContext.CachedCalendarEvents
            .Where(e => e.StartUtc >= fromUtc && e.StartUtc <= toUtc)
            .OrderBy(e => e.StartUtc)
            .ToListAsync(cancellationToken);

        return cachedEntities
            .ConvertAll(e => new CalendarEvent
            {
                Id = e.EventId,
                Title = e.Title,
                Description = e.Description,
                StartUtc = e.StartUtc,
                EndUtc = e.EndUtc,
                Location = e.Location,
                Provider = e.Provider,
                CalendarId = e.CalendarId,
                IsAllDay = e.IsAllDay,
            })
;
    }

    public async Task ClearOldEventsAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-14);
        await _dbContext.CachedCalendarEvents
            .Where(e => e.CachedAtUtc < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }
}

/// <summary>
/// Database entity for cached calendar events.
/// </summary>
public sealed class CachedCalendarEventEntity
{
    public int Id { get; set; }
    public required string EventId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
    public string? Location { get; set; }
    public required string Provider { get; set; }
    public required string CalendarId { get; set; }
    public bool IsAllDay { get; set; }
    public DateTime CachedAtUtc { get; set; }
}

