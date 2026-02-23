using ModalCalendarNotification.Core.Features.CalendarSelection;
using ModalCalendarNotification.Core.Features.DismissedEventsManagement;
using ModalCalendarNotification.Core.Features.NotificationManagement;
using ModalCalendarNotification.Core.Shared.Models;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.NotificationManagement;

public sealed class NotificationEngineTests
{
    [Fact]
    public void BuildNotifications_FiltersByLeadTimeWindow()
    {
        var now = DateTimeOffset.UtcNow;
        var events = new List<CalendarEvent>
        {
            new()
            {
                Id = "1",
                Title = "In Window",
                StartUtc = now.AddMinutes(2),
                EndUtc = now.AddMinutes(3),
                Provider = "P",
                CalendarId = "C",
            },
            new()
            {
                Id = "2",
                Title = "Out of Window",
                StartUtc = now.AddMinutes(10),
                EndUtc = now.AddMinutes(11),
                Provider = "P",
                CalendarId = "C",
            },
        };

        var sut = new NotificationEngine();
        var notifications = sut.BuildNotifications(events, now, 3);

        notifications.Count.ShouldBe(1);
        notifications[0].EventId.ShouldBe("1");
    }

    [Fact]
    public void BuildNotifications_FiltersDismissedTitles()
    {
        var now = DateTimeOffset.UtcNow;
        var events = new List<CalendarEvent>
        {
            new()
            {
                Id = "1",
                Title = "Daily Standup",
                StartUtc = now.AddMinutes(2),
                EndUtc = now.AddMinutes(3),
                Provider = "P",
                CalendarId = "C",
            },
            new()
            {
                Id = "2",
                Title = "Product Review",
                StartUtc = now.AddMinutes(2),
                EndUtc = now.AddMinutes(3),
                Provider = "P",
                CalendarId = "C",
            },
        };

        var repo = new FakeDismissedTitleRepository(["daily standup"]);
        var sut = new NotificationEngine(repo);

        var notifications = sut.BuildNotifications(events, now, 3);

        notifications.Count.ShouldBe(1);
        notifications[0].Title.ShouldBe("Product Review");
    }

    [Fact]
    public void BuildNotifications_RespectsSelectedCalendars()
    {
        var now = DateTimeOffset.UtcNow;
        var events = new List<CalendarEvent>
        {
            new()
            {
                Id = "1",
                Title = "Allowed",
                StartUtc = now.AddMinutes(2),
                EndUtc = now.AddMinutes(3),
                Provider = "Outlook365",
                CalendarId = "primary",
            },
            new()
            {
                Id = "2",
                Title = "Blocked",
                StartUtc = now.AddMinutes(2),
                EndUtc = now.AddMinutes(3),
                Provider = "Outlook365",
                CalendarId = "other",
            },
        };

        var dismissed = new FakeDismissedTitleRepository([]);
        var selectedCalendars = new FakeCalendarSelectionRepository([
            new SelectedCalendar
            {
                ProviderName = "Outlook365",
                CalendarId = "primary",
                DisplayName = "Primary",
                IsSelected = true,
            },
        ]);

        var sut = new NotificationEngine(dismissed, selectedCalendars);

        var notifications = sut.BuildNotifications(events, now, 3);

        notifications.Count.ShouldBe(1);
        notifications[0].EventId.ShouldBe("1");
    }

    private sealed class FakeDismissedTitleRepository : IDismissedEventTitleRepository
    {
        private readonly List<DismissedEventTitle> _items;

        public FakeDismissedTitleRepository(IEnumerable<string> titles)
        {
            _items = titles
                .Select(x => new DismissedEventTitle
                {
                    Title = x,
                    DismissedAtUtc = DateTimeOffset.UtcNow,
                })
                .ToList();
        }

        public Task AddAsync(string title, CancellationToken cancellationToken = default)
        {
            _items.Add(
                new DismissedEventTitle { Title = title, DismissedAtUtc = DateTimeOffset.UtcNow }
            );
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string title, CancellationToken cancellationToken = default)
        {
            var exists = _items.Any(x =>
                string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase)
            );
            return Task.FromResult(exists);
        }

        public Task<IReadOnlyList<DismissedEventTitle>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult<IReadOnlyList<DismissedEventTitle>>(_items);
        }

        public Task RemoveAsync(string title, CancellationToken cancellationToken = default)
        {
            _items.RemoveAll(x =>
                string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase)
            );
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCalendarSelectionRepository : ICalendarSelectionRepository
    {
        private readonly IReadOnlyList<SelectedCalendar> _selected;

        public FakeCalendarSelectionRepository(IReadOnlyList<SelectedCalendar> selected)
        {
            _selected = selected;
        }

        public Task<IReadOnlyList<SelectedCalendar>> GetSelectedAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(_selected);
        }

        public Task SaveSelectedAsync(
            IReadOnlyList<SelectedCalendar> selectedCalendars,
            CancellationToken cancellationToken = default
        )
        {
            return Task.CompletedTask;
        }
    }
}
