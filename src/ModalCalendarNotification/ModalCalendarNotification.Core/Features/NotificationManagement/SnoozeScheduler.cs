using ModalCalendarNotification.Core.Shared.Utilities;
using AppTimeProvider = ModalCalendarNotification.Core.Shared.Utilities.TimeProvider;

namespace ModalCalendarNotification.Core.Features.NotificationManagement;

public sealed class SnoozeScheduler
{
    private readonly AppTimeProvider _timeProvider;

    public SnoozeScheduler(AppTimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DateTimeOffset CalculateNextTrigger(DateTimeOffset fromUtc, int snoozeMinutes)
    {
        return fromUtc.AddMinutes(snoozeMinutes);
    }

    public Task DelayUntilAsync(DateTimeOffset triggerUtc, CancellationToken cancellationToken = default)
    {
        var delay = triggerUtc - _timeProvider.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        return _timeProvider.DelayAsync(delay, cancellationToken);
    }
}
