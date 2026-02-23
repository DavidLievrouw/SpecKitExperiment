using ModalCalendarNotification.Core.Features.StartupRecovery;
using ModalCalendarNotification.Core.Shared.Models;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.StartupRecovery;

public sealed class MissedEventDetectorTests
{
    [Fact]
    public void DetectMissedEvents_Respects24HourLookback()
    {
        var now = DateTimeOffset.UtcNow;
        var detector = new MissedEventDetector();

        var events = new List<CalendarEvent>
        {
            new()
            {
                Id = "old",
                Title = "Old",
                StartUtc = now.AddHours(-26),
                EndUtc = now.AddHours(-25),
                Provider = "P",
                CalendarId = "C",
            },
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

        var missed = detector.DetectMissedEvents(events, null, now);

        missed.Count.ShouldBe(1);
        missed[0].Id.ShouldBe("missed");
    }
}
