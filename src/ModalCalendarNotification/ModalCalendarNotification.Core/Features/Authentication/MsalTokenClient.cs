using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace ModalCalendarNotification.Core.Features.Authentication;

public sealed class MsalTokenClient : IMsalTokenClient
{
    private readonly ILogger<MsalTokenClient> _logger;
    private readonly Dictionary<string, IPublicClientApplication> _clients = new();

    /// <summary>
    /// Azure AD configuration for Office 365
    /// </summary>
    private const string OutlookClientId = "YOUR_OUTLOOK_CLIENT_ID"; // TODO: Configure in appsettings.json
    private const string OutlookTenantId = "common";

    public MsalTokenClient(ILogger<MsalTokenClient> logger)
    {
        _logger = logger;
    }

    public async Task<string> AcquireTokenAsync(
        string provider,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var app = GetOrCreateClientApp(provider);

            // Try to acquire token silently first
            var accounts = await app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            AuthenticationResult? result = null;

            if (firstAccount != null)
            {
                try
                {
                    result = await app.AcquireTokenSilent(scopes, firstAccount)
                        .ExecuteAsync(cancellationToken);
                }
                catch (MsalUiRequiredException)
                {
                    _logger.LogInformation(
                        "Silent token acquisition failed, requiring interactive login"
                    );
                }
            }

            // If silent acquisition failed, acquire token interactively
            if (result == null)
            {
                result = await app.AcquireTokenInteractive(scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync(cancellationToken);
            }

            return result.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire token for provider {Provider}", provider);
            return string.Empty;
        }
    }

    private IPublicClientApplication GetOrCreateClientApp(string provider)
    {
        if (_clients.TryGetValue(provider, out var existingClient))
        {
            return existingClient;
        }

        IPublicClientApplication app = provider.ToLowerInvariant() switch
        {
            "outlook365" => PublicClientApplicationBuilder
                .Create(OutlookClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, OutlookTenantId)
                .WithRedirectUri("http://localhost")
                .Build(),

            "googlecalendar" => throw new NotImplementedException(
                "Google Calendar authentication not yet implemented. Use MSAL for Azure AD only."
            ),

            _ => throw new ArgumentException($"Unknown provider: {provider}", nameof(provider)),
        };

        _clients[provider] = app;
        return app;
    }
}
