namespace ModalCalendarNotification.Core.Features.CalendarIntegration;

/// <summary>
/// Tracks the health and status of calendar provider connections.
/// </summary>
public interface ISyncStatusService
{
    /// <summary>
    /// Gets the current sync status.
    /// </summary>
    SyncStatus GetStatus();

    /// <summary>
    /// Marks sync as successful.
    /// </summary>
    void MarkSyncSuccess();

    /// <summary>
    /// Marks sync as failed with a specific error.
    /// </summary>
    void MarkSyncFailure(string errorMessage);

    /// <summary>
    /// Returns true if the connection is currently healthy.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Time of last successful sync, or null if never synced.
    /// </summary>
    DateTime? LastSuccessfulSyncUtc { get; }

    /// <summary>
    /// Error message from last failed sync.
    /// </summary>
    string? LastErrorMessage { get; }

    /// <summary>
    /// Event raised when sync status changes.
    /// </summary>
    event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;
}

/// <summary>
/// Sync status information.
/// </summary>
public sealed record SyncStatus
{
    public bool IsConnected { get; init; }
    public DateTime? LastSuccessfulSyncUtc { get; init; }
    public string? ErrorMessage { get; init; }
    public int FailureCount { get; init; }
}

/// <summary>
/// Event args for sync status changes.
/// </summary>
public sealed class SyncStatusChangedEventArgs : EventArgs
{
    public required SyncStatus Status { get; init; }
    public required string Provider { get; init; }
}

