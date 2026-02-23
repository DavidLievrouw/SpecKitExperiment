using FakeItEasy;
using ModalCalendarNotification.CalendarProviders;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.CalendarIntegration;

public sealed class OutlookCalendarProviderTests
{
    [Fact]
    public async Task GetEventsAsync_AcquiresAccessToken()
    {
        var auth = new CapturingAuthenticationService();
        var logger = A.Fake<Microsoft.Extensions.Logging.ILogger<OutlookCalendarProvider>>();
        var sut = new OutlookCalendarProvider(auth, logger);

        var events = await sut.GetEventsAsync(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(1),
            TestContext.Current.CancellationToken
        );

        auth.CallCount.ShouldBe(1);
        events.ShouldNotBeNull();
    }
}
