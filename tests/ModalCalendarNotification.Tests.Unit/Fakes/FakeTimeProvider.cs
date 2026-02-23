using AppTimeProvider = ModalCalendarNotification.Core.Shared.Utilities.TimeProvider;

namespace ModalCalendarNotification.Tests.Unit.Fakes;

public sealed class FakeTimeProvider : AppTimeProvider
{
    public FakeTimeProvider(DateTimeOffset initialUtc)
    {
        CurrentUtc = initialUtc;
    }

    public DateTimeOffset CurrentUtc { get; private set; }

    public override DateTimeOffset UtcNow => CurrentUtc;

    public void Advance(TimeSpan by)
    {
        CurrentUtc = CurrentUtc.Add(by);
    }

    public override Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        CurrentUtc = CurrentUtc.Add(delay);
        return Task.CompletedTask;
    }
}
