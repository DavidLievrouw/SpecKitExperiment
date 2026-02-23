using ModalCalendarNotification.Core.Features.NotificationManagement;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.NotificationManagement;

public sealed class AutoDismissHandlerTests
{
    [Fact]
    public void ShouldAutoDismiss_ReturnsTrueWhenTimeoutReached()
    {
        DateTimeOffset shown = DateTimeOffset.UtcNow;
        DateTimeOffset now = shown.AddSeconds(61);
        var sut = new AutoDismissHandler();

        sut.ShouldAutoDismiss(shown, now, 60).ShouldBeTrue();
    }
}
