namespace ModalCalendarNotification.Tests.EndToEnd.TestHarness;

public sealed class TestApplicationHost
{
    public bool Started { get; private set; }

    public void Start()
    {
        Started = true;
    }

    public void Stop()
    {
        Started = false;
    }
}
