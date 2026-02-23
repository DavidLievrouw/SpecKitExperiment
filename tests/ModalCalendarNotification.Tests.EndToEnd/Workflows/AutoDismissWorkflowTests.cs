using ModalCalendarNotification.Core.Features.NotificationManagement;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.EndToEnd.Workflows;

public sealed class AutoDismissWorkflowTests
{
    [Fact]
    public void AutoDismissWorkflow_DismissesUntouchedModalAfterTimeout()
    {
        var shown = DateTimeOffset.UtcNow;
        var now = shown.AddSeconds(61);
        var handler = new AutoDismissHandler();

        handler.ShouldAutoDismiss(shown, now, 60).ShouldBeTrue();
    }
}