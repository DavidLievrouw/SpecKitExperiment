namespace ModalCalendarNotification.Core.Features.DismissedEventsManagement;

public interface IDismissedEventTitleRepository
{
    Task AddAsync(string title, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string title, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DismissedEventTitle>> GetAllAsync(CancellationToken cancellationToken = default);

    Task RemoveAsync(string title, CancellationToken cancellationToken = default);
}
