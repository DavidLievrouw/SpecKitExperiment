using ModalCalendarNotification.Core.Features.NotificationManagement;
using Shouldly;
using Xunit;
using AppTimeProvider = ModalCalendarNotification.Core.Shared.Utilities.TimeProvider;

namespace ModalCalendarNotification.Tests.EndToEnd.Workflows;

public sealed class SnoozeWorkflowTests
{
    [Fact]
    public void SnoozeWorkflow_RecalculatesTriggerTime()
    {
        var scheduler = new SnoozeScheduler(new AppTimeProvider());
        var now = DateTimeOffset.UtcNow;

        var next = scheduler.CalculateNextTrigger(now, 5);

        next.ShouldBe(now.AddMinutes(5));
    }
}
