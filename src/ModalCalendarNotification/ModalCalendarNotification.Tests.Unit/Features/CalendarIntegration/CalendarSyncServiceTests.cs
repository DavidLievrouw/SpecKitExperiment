using ModalCalendarNotification.Core.Features.CalendarIntegration;
using ModalCalendarNotification.Core.Shared.Models;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.CalendarIntegration;

public sealed class CalendarSyncServiceTests
{
    [Fact]
    public async Task SyncUpcomingEventsAsync_CombinesAndSortsEvents()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var provider1 = new FakeProvider([
            new CalendarEvent
            {
                Id = "2",
                Title = "B",
                StartUtc = now.AddMinutes(2),
                EndUtc = now.AddMinutes(3),
                Provider = "P",
                CalendarId = "c",
            },
        ]);
        var provider2 = new FakeProvider([
            new CalendarEvent
            {
                Id = "1",
                Title = "A",
                StartUtc = now.AddMinutes(1),
                EndUtc = now.AddMinutes(2),
                Provider = "P",
                CalendarId = "c",
            },
        ]);
        var sut = new CalendarSyncService([provider1, provider2]);

        IReadOnlyList<CalendarEvent> events = await sut.SyncUpcomingEventsAsync(
            now,
            now.AddHours(1)
        );

        events.Count.ShouldBe(2);
        events[0].Id.ShouldBe("1");
    }

    private sealed class FakeProvider : ICalendarProviderAdapter
    {
        private readonly IReadOnlyList<CalendarEvent> _events;

        public FakeProvider(IReadOnlyList<CalendarEvent> events)
        {
            _events = events;
        }

        public Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
            DateTimeOffset fromUtc,
            DateTimeOffset toUtc,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(_events);
        }
    }
}
