using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Core.Features.NotificationManagement;

public interface INotificationEngine
{
    IReadOnlyList<Notification> BuildNotifications(
        IReadOnlyList<CalendarEvent> events,
        DateTimeOffset nowUtc,
        int leadTimeMinutes);
}
