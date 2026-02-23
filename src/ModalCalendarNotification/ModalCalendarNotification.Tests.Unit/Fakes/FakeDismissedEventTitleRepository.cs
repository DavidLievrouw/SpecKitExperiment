using ModalCalendarNotification.Core.Features.DismissedEventsManagement;

namespace ModalCalendarNotification.Tests.Unit.Fakes;

public sealed class FakeDismissedEventTitleRepository : IDismissedEventTitleRepository
{
    private readonly List<DismissedEventTitle> _items = [];

    public Task AddAsync(string title, CancellationToken cancellationToken = default)
    {
        _items.Add(new DismissedEventTitle
        {
            Title = title,
            DismissedAtUtc = DateTimeOffset.UtcNow
        });

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string title, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.Any(x => string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<IReadOnlyList<DismissedEventTitle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<DismissedEventTitle>>(_items);
    }

    public Task RemoveAsync(string title, CancellationToken cancellationToken = default)
    {
        _items.RemoveAll(x => string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase));
        return Task.CompletedTask;
    }
}
