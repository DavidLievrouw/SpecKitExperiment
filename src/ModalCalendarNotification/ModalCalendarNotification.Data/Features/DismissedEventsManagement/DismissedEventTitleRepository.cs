using Microsoft.EntityFrameworkCore;
using ModalCalendarNotification.Core.Features.DismissedEventsManagement;

namespace ModalCalendarNotification.Data.Features.DismissedEventsManagement;

public sealed class DismissedEventTitleRepository : IDismissedEventTitleRepository
{
    private readonly AppDbContext _dbContext;

    public DismissedEventTitleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(string title, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        string normalized = title.Trim();
        bool exists = await ExistsAsync(normalized, cancellationToken);
        if (exists)
        {
            return;
        }

        _dbContext.DismissedEventTitles.Add(
            new DismissedEventTitleEntity
            {
                Title = normalized,
                DismissedAtUtc = DateTimeOffset.UtcNow,
            }
        );

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string title, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        string normalized = title.Trim().ToLowerInvariant();

        return await _dbContext.DismissedEventTitles.AnyAsync(
            x => x.Title.ToLower() == normalized,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<DismissedEventTitle>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .DismissedEventTitles.OrderBy(x => x.Title)
            .Select(x => new DismissedEventTitle
            {
                Title = x.Title,
                DismissedAtUtc = x.DismissedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveAsync(string title, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        string normalized = title.Trim().ToLowerInvariant();

        List<DismissedEventTitleEntity> entities = await _dbContext
            .DismissedEventTitles.Where(x => x.Title.ToLower() == normalized)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
        {
            return;
        }

        _dbContext.DismissedEventTitles.RemoveRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
