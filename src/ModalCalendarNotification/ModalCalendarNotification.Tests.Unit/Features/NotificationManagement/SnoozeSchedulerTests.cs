using ModalCalendarNotification.Core.Features.NotificationManagement;
using Shouldly;
using Xunit;
using AppTimeProvider = ModalCalendarNotification.Core.Shared.Utilities.TimeProvider;

namespace ModalCalendarNotification.Tests.Unit.Features.NotificationManagement;

public sealed class SnoozeSchedulerTests
{
    [Fact]
    public void CalculateNextTrigger_AddsSnoozeDuration()
    {
        var now = DateTimeOffset.UtcNow;
        var sut = new SnoozeScheduler(new AppTimeProvider());

        var next = sut.CalculateNextTrigger(now, 5);

        next.ShouldBe(now.AddMinutes(5));
    }
}
