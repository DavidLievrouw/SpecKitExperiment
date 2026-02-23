using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Core.Features.StartupRecovery;

public sealed class MissedEventRecoveryService
{
    private readonly IApplicationStateRepository _applicationStateRepository;
    private readonly MissedEventDetector _missedEventDetector;

    public MissedEventRecoveryService(
        IApplicationStateRepository applicationStateRepository,
        MissedEventDetector missedEventDetector
    )
    {
        _applicationStateRepository = applicationStateRepository;
        _missedEventDetector = missedEventDetector;
    }

    public async Task<IReadOnlyList<CalendarEvent>> RecoverMissedEventsAsync(
        IReadOnlyList<CalendarEvent> latestEvents,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default
    )
    {
        var lastRun = await _applicationStateRepository.GetLastRunUtcAsync(cancellationToken);
        var missed = _missedEventDetector.DetectMissedEvents(latestEvents, lastRun, nowUtc);

        await _applicationStateRepository.SetLastRunUtcAsync(nowUtc, cancellationToken);
        return missed;
    }
}
