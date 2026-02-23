using ModalCalendarNotification.Core.Shared.Utilities;

namespace ModalCalendarNotification.Core.Features.Authentication;

public sealed class OAuthService : IAuthenticationService
{
    private readonly IMsalTokenClient _tokenClient;

    public OAuthService(IMsalTokenClient tokenClient)
    {
        _tokenClient = tokenClient;
    }

    public Task<string> AcquireAccessTokenAsync(string provider, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentNullException.ThrowIfNull(scopes);

        return PollyPolicies.ExecuteWithRetryAsync(
            async token => await _tokenClient.AcquireTokenAsync(provider, scopes, token),
            maxRetryAttempts: 3,
            retryDelay: TimeSpan.FromMilliseconds(50),
            cancellationToken: cancellationToken);
    }
}

public interface IMsalTokenClient
{
    Task<string> AcquireTokenAsync(string provider, IReadOnlyList<string> scopes, CancellationToken cancellationToken);
}
