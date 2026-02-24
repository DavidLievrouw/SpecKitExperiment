using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Core.Features.CalendarIntegration;

public interface ICalendarProviderFactory
{
    Task<IReadOnlyList<Calendar>> GetAvailableCalendarsAsync(
        string providerName,
        CancellationToken cancellationToken = default
    );
}

