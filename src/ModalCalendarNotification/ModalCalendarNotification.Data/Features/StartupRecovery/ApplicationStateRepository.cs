using Microsoft.EntityFrameworkCore;
using ModalCalendarNotification.Core.Features.StartupRecovery;

namespace ModalCalendarNotification.Data.Features.StartupRecovery;

public sealed class ApplicationStateRepository : IApplicationStateRepository
{
    private const string LastRunKey = "StartupRecovery.LastRunUtc";
    private readonly AppDbContext _dbContext;

    public ApplicationStateRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DateTimeOffset?> GetLastRunUtcAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ApplicationStates
            .FirstOrDefaultAsync(x => x.Key == LastRunKey, cancellationToken);

        if (entity?.Value is null)
        {
            return null;
        }

        return DateTimeOffset.TryParse(entity.Value, out var parsed) ? parsed : null;
    }

    public async Task SetLastRunUtcAsync(DateTimeOffset timestampUtc, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ApplicationStates
            .FirstOrDefaultAsync(x => x.Key == LastRunKey, cancellationToken);

        if (entity is null)
        {
            _dbContext.ApplicationStates.Add(new ApplicationStateEntity
            {
                Key = LastRunKey,
                Value = timestampUtc.ToString("O"),
                UpdatedAtUtc = DateTimeOffset.UtcNow
            });
        }
        else
        {
            entity.Value = timestampUtc.ToString("O");
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}