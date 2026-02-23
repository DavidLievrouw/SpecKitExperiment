using ModalCalendarNotification.Core.Features.StartupRecovery;
using ModalCalendarNotification.Core.Shared.Models;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.EndToEnd.Workflows;

public sealed class MissedEventDetectionWorkflowTests
{
    [Fact]
    public async Task MissedEventDetectionWorkflow_DetectsEventsSinceLastRun()
    {
        var now = DateTimeOffset.UtcNow;
        var repository = new InMemoryApplicationStateRepository(now.AddHours(-3));
        var service = new MissedEventRecoveryService(repository, new MissedEventDetector());

        var events = new List<CalendarEvent>
        {
            new()
            {
                Id = "1",
                Title = "Missed",
                StartUtc = now.AddHours(-2),
                EndUtc = now.AddHours(-1),
                Provider = "Outlook365",
                CalendarId = "primary",
            },
            new()
            {
                Id = "2",
                Title = "Future",
                StartUtc = now.AddHours(1),
                EndUtc = now.AddHours(2),
                Provider = "Outlook365",
                CalendarId = "primary",
            },
        };

        var missed = await service.RecoverMissedEventsAsync(
            events,
            now,
            TestContext.Current.CancellationToken
        );

        missed.Count.ShouldBe(1);
        missed[0].Title.ShouldBe("Missed");
        (await repository.GetLastRunUtcAsync(TestContext.Current.CancellationToken)).ShouldBe(now);
    }

    private sealed class InMemoryApplicationStateRepository : IApplicationStateRepository
    {
        private DateTimeOffset? _value;

        public InMemoryApplicationStateRepository(DateTimeOffset? initial)
        {
            _value = initial;
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
