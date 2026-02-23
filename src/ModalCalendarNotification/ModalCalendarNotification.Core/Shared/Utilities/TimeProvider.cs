namespace ModalCalendarNotification.Core.Shared.Utilities;

public class TimeProvider
{
    public virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public virtual DateTimeOffset LocalNow => DateTimeOffset.Now;

    public virtual Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return Task.Delay(delay, cancellationToken);
    }
}
