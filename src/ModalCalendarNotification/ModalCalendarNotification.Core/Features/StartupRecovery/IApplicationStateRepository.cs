namespace ModalCalendarNotification.Core.Features.StartupRecovery;

public interface IApplicationStateRepository
{
    Task<DateTimeOffset?> GetLastRunUtcAsync(CancellationToken cancellationToken = default);

    Task SetLastRunUtcAsync(
        DateTimeOffset timestampUtc,
        CancellationToken cancellationToken = default
    );
}
