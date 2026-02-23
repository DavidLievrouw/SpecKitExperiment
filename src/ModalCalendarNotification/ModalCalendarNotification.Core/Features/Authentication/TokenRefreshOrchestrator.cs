namespace ModalCalendarNotification.Core.Features.Authentication;

public sealed class TokenRefreshOrchestrator
{
    private readonly IAuthenticationService _authenticationService;

    public TokenRefreshOrchestrator(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<string> RefreshTokenAsync(
        string provider,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default
    )
    {
        return await _authenticationService.AcquireAccessTokenAsync(
            provider,
            scopes,
            cancellationToken
        );
    }
}
