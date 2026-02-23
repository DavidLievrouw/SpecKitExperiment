using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Tests.EndToEnd.TestHarness;

public sealed class ModalWindowSimulator
{
    public bool IsVisible { get; private set; }

    public IReadOnlyList<Notification> ActiveNotifications { get; private set; } = [];

    public void Show(IReadOnlyList<Notification> notifications)
    {
        ActiveNotifications = notifications;
        IsVisible = notifications.Count > 0;
    }

    public void Dismiss()
    {
        IsVisible = false;
        ActiveNotifications = [];
    }
}
