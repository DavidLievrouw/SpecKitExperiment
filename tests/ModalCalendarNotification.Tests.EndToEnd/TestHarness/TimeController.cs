namespace ModalCalendarNotification.Tests.EndToEnd.TestHarness;

public sealed class TimeController
{
    public DateTimeOffset UtcNow { get; private set; } = DateTimeOffset.UtcNow;

    public void Advance(TimeSpan by)
    {
        UtcNow = UtcNow.Add(by);
    }
}
