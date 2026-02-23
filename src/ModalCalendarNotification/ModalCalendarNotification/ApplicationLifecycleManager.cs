namespace ModalCalendarNotification;

public sealed class ApplicationLifecycleManager
{
    private readonly object _sync = new();

    public bool IsRunning { get; private set; }

    public void Start()
    {
        lock (_sync)
        {
            IsRunning = true;
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            IsRunning = false;
        }
    }
}
