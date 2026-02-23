using ModalCalendarNotification.Core.Features.NotificationManagement;
using ModalCalendarNotification.Tests.EndToEnd.TestHarness;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.EndToEnd.Workflows;

public sealed class ReceiveNotificationWorkflowTests
{
    [Fact]
    public void ReceiveNotificationWorkflow_ShowsModalForUpcomingEvent()
    {
        var host = new TestApplicationHost();
        host.Start();

        var modal = new ModalWindowSimulator();
        var now = DateTimeOffset.UtcNow;
        var events = new[] { MockCalendarEventGenerator.CreateUpcoming("1", "Standup", 2) };

        var engine = new NotificationEngine();
        var notifications = engine.BuildNotifications(events, now, 3);

        modal.Show(notifications);

        host.Started.ShouldBeTrue();
        modal.IsVisible.ShouldBeTrue();
        modal.ActiveNotifications.Count.ShouldBe(1);
    }
}
