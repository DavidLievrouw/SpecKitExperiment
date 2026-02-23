using ModalCalendarNotification.CalendarProviders;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.CalendarIntegration;

public sealed class GoogleCalendarProviderTests
{
    [Fact]
    public async Task GetEventsAsync_AcquiresAccessToken()
    {
        var auth = new CapturingAuthenticationService();
        var sut = new GoogleCalendarProvider(auth);

        var events = await sut.GetEventsAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        auth.CallCount.ShouldBe(1);
        events.ShouldNotBeNull();
    }
}
