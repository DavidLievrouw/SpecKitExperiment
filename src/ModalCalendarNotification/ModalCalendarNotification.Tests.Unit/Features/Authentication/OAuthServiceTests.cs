using ModalCalendarNotification.Core.Features.Authentication;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.Authentication;

public sealed class OAuthServiceTests
{
    [Fact]
    public async Task AcquireAccessTokenAsync_ReturnsTokenFromMsalClient()
    {
        var tokenClient = new FakeMsalTokenClient("token-123");
        var sut = new OAuthService(tokenClient);

        string token = await sut.AcquireAccessTokenAsync("Outlook365", ["Calendars.Read"]);

        token.ShouldBe("token-123");
    }

    private sealed class FakeMsalTokenClient : IMsalTokenClient
    {
        private readonly string _token;

        public FakeMsalTokenClient(string token)
        {
            _token = token;
        }

        public Task<string> AcquireTokenAsync(
            string provider,
            IReadOnlyList<string> scopes,
            CancellationToken cancellationToken
        )
        {
            return Task.FromResult(_token);
        }
    }
}
