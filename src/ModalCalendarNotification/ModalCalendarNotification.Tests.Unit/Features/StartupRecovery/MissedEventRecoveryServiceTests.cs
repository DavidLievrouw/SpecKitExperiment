using ModalCalendarNotification.Core.Features.StartupRecovery;
using ModalCalendarNotification.Core.Shared.Models;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.StartupRecovery;

public sealed class MissedEventRecoveryServiceTests
{
    [Fact]
    public async Task RecoverMissedEventsAsync_DetectsAndUpdatesLastRun()
    {
        var now = DateTimeOffset.UtcNow;
        var repo = new InMemoryApplicationStateRepository(now.AddHours(-3));
        var detector = new MissedEventDetector();
        var sut = new MissedEventRecoveryService(repo, detector);

        var events = new List<CalendarEvent>
        {
            new()
            {
                Id = "missed",
                Title = "Missed",
                StartUtc = now.AddHours(-2),
                EndUtc = now.AddHours(-1),
                Provider = "P",
                CalendarId = "C",
            },
            new()
            {
                Id = "future",
                Title = "Future",
                StartUtc = now.AddHours(1),
                EndUtc = now.AddHours(2),
                Provider = "P",
                CalendarId = "C",
            },
        };

        var missed = await sut.RecoverMissedEventsAsync(
            events,
            now,
            TestContext.Current.CancellationToken
        );

        missed.Count.ShouldBe(1);
        (await repo.GetLastRunUtcAsync(TestContext.Current.CancellationToken)).ShouldBe(now);
    }

    private sealed class InMemoryApplicationStateRepository : IApplicationStateRepository
    {
        private DateTimeOffset? _value;

        public InMemoryApplicationStateRepository(DateTimeOffset? initialValue)
        {
            _value = initialValue;
        }

        public Task<DateTimeOffset?> GetLastRunUtcAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(_value);
        }

        public Task SetLastRunUtcAsync(
            DateTimeOffset timestampUtc,
            CancellationToken cancellationToken = default
        )
        {
            _value = timestampUtc;
            return Task.CompletedTask;
        }
    }
}
