using ModalCalendarNotification.Core.Features.CalendarIntegration;

namespace ModalCalendarNotification.Features.CalendarIntegration;

/// <summary>
/// Tracks sync status for each provider and overall connection health.
/// </summary>
public sealed class SyncStatusService : ISyncStatusService
{
    private DateTime? _lastSuccessfulSyncUtc;
    private string? _lastErrorMessage;
    private int _failureCount;
    private bool _isConnected = true;

    public event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;

    public SyncStatus GetStatus()
    {
        return new SyncStatus
        {
            IsConnected = IsConnected,
            LastSuccessfulSyncUtc = LastSuccessfulSyncUtc,
            ErrorMessage = LastErrorMessage,
            FailureCount = _failureCount,
        };
    }

    public void MarkSyncSuccess()
    {
        _lastSuccessfulSyncUtc = DateTime.UtcNow;
        var wasDisconnected = !_isConnected;
        _isConnected = true;
        _lastErrorMessage = null;
        _failureCount = 0;

        if (wasDisconnected)
        {
            OnSyncStatusChanged(new SyncStatusChangedEventArgs
            {
                Status = GetStatus(),
                Provider = "All",
            });
        }
    }

    public void MarkSyncFailure(string errorMessage)
    {
        _lastErrorMessage = errorMessage;
        _failureCount++;
        var previousState = _isConnected;
        _isConnected = false;

        if (previousState != _isConnected)
        {
            OnSyncStatusChanged(new SyncStatusChangedEventArgs
            {
                Status = GetStatus(),
                Provider = "All",
            });
        }
    }

    public bool IsConnected => _isConnected;

    public DateTime? LastSuccessfulSyncUtc => _lastSuccessfulSyncUtc;

    public string? LastErrorMessage => _lastErrorMessage;

    private void OnSyncStatusChanged(SyncStatusChangedEventArgs args)
    {
        SyncStatusChanged?.Invoke(this, args);
    }
}

