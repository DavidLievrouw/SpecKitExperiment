namespace ModalCalendarNotification.Core.Features.NotificationManagement;

public sealed class AutoDismissHandler
{
    public bool ShouldAutoDismiss(DateTimeOffset shownAtUtc, DateTimeOffset nowUtc, int timeoutSeconds)
    {
        return nowUtc >= shownAtUtc.AddSeconds(timeoutSeconds);
    }
}