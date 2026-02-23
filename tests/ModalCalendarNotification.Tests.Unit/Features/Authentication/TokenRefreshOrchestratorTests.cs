using ModalCalendarNotification.Core.Features.Authentication;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.Authentication;

public sealed class TokenRefreshOrchestratorTests
{
    [Fact]
    public async Task RefreshTokenAsync_DelegatesToAuthenticationService()
    {
        var authService = new StubAuthenticationService("refreshed-token");
        var sut = new TokenRefreshOrchestrator(authService);

        var token = await sut.RefreshTokenAsync("GoogleCalendar", ["scope"]);

        token.ShouldBe("refreshed-token");
    }

    private sealed class StubAuthenticationService : IAuthenticationService
    {
        private readonly string _token;

        public StubAuthenticationService(string token)
        {
            _token = token;
        }

        public Task<string> AcquireAccessTokenAsync(string provider, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_token);
        }
    }
}
