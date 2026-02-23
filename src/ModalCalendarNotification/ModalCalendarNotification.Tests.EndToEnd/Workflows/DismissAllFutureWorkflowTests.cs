using ModalCalendarNotification.Core.Features.DismissedEventsManagement;
using ModalCalendarNotification.Core.Features.NotificationManagement;
using ModalCalendarNotification.Core.Shared.Models;
using ModalCalendarNotification.UI.Features.NotificationManagement;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.EndToEnd.Workflows;

public sealed class DismissAllFutureWorkflowTests
{
    [Fact]
    public async Task DismissAllFutureWorkflow_FiltersFutureNotificationsByTitle()
    {
        var now = DateTimeOffset.UtcNow;
        var repository = new InMemoryDismissedTitleRepository();
        var viewModel = new NotificationModalViewModel(repository)
        {
            Events =
            [
                new NotificationEventItem
                {
                    EventId = "1",
                    Title = "Daily Standup",
                    StartUtc = now.AddMinutes(2)
                }
            ]
        };

        await viewModel.DismissAllFutureCommand.ExecuteAsync(null);

        var engine = new NotificationEngine(repository);
        var upcoming = new List<CalendarEvent>
        {
            new() { Id = "1", Title = "Daily Standup", StartUtc = now.AddMinutes(2), EndUtc = now.AddMinutes(3), Provider = "P", CalendarId = "C" },
            new() { Id = "2", Title = "Architecture Review", StartUtc = now.AddMinutes(2), EndUtc = now.AddMinutes(3), Provider = "P", CalendarId = "C" }
        };

        var notifications = engine.BuildNotifications(upcoming, now, 3);

        notifications.Count.ShouldBe(1);
        notifications[0].Title.ShouldBe("Architecture Review");
    }

    private sealed class InMemoryDismissedTitleRepository : IDismissedEventTitleRepository
    {
        private readonly List<DismissedEventTitle> _items = [];

        public Task AddAsync(string title, CancellationToken cancellationToken = default)
        {
            _items.Add(new DismissedEventTitle { Title = title, DismissedAtUtc = DateTimeOffset.UtcNow });
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
}