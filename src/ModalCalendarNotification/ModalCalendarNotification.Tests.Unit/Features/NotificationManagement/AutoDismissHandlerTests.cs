using ModalCalendarNotification.Core.Features.NotificationManagement;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.NotificationManagement;

public sealed class AutoDismissHandlerTests
{
    [Fact]
    public void ShouldAutoDismiss_ReturnsTrueWhenTimeoutReached()
    {
        var shown = DateTimeOffset.UtcNow;
        var now = shown.AddSeconds(61);
        var sut = new AutoDismissHandler();

        sut.ShouldAutoDismiss(shown, now, 60).ShouldBeTrue();
    }
}
