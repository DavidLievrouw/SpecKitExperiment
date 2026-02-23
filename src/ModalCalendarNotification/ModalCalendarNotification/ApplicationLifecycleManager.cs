using ModalCalendarNotification.UI.Features.SystemTrayManagement;

namespace ModalCalendarNotification;

public sealed class ApplicationLifecycleManager : IDisposable
{
    private readonly object _sync = new();
    private readonly SystemTrayManager _systemTrayManager;
    private bool _disposed;

    public ApplicationLifecycleManager(SystemTrayManager systemTrayManager)
    {
        _systemTrayManager =
            systemTrayManager ?? throw new ArgumentNullException(nameof(systemTrayManager));
    }

    public bool IsRunning { get; private set; }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _systemTrayManager?.Dispose();
            _disposed = true;
        }
    }

    public void Start()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ApplicationLifecycleManager));
            }

            IsRunning = true;
            _systemTrayManager.Initialize();
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
