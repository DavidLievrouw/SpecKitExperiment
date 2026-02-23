using ModalCalendarNotification.CalendarProviders;
using ModalCalendarNotification.Core.Features.Authentication;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.CalendarIntegration;

public sealed class OutlookCalendarProviderTests
{
    [Fact]
    public async Task GetEventsAsync_AcquiresAccessToken()
    {
        var auth = new CapturingAuthenticationService();
        var sut = new OutlookCalendarProvider(auth);

        var events = await sut.GetEventsAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        auth.CallCount.ShouldBe(1);
        events.ShouldNotBeNull();
    }
}
