using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ModalCalendarNotification.Core.Features.Authentication;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;

namespace ModalCalendarNotification.UI.Features.ConfigurationManagement;

public partial class ProviderSelectionViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<ProviderSelectionViewModel> _logger;

    [ObservableProperty]
    private string _accountLabel = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<string> _availableProviders = ["Outlook365", "GoogleCalendar"];

    [ObservableProperty]
    private bool _isAccepted;

    [ObservableProperty]
    private bool _isAuthenticating;

    [ObservableProperty]
    private string _selectedProvider = "Outlook365";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ProviderSelectionViewModel(
        IAuthenticationService authenticationService,
        ILogger<ProviderSelectionViewModel> logger
    )
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public ProviderAccountItem? SelectedAccount { get; private set; }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        try
        {
            IsAuthenticating = true;
            StatusMessage = "Authenticating...";

            // Define scopes based on provider
            var scopes = SelectedProvider.ToLowerInvariant() switch
            {
                "outlook365" => new[] { "Calendars.Read" },
                "googlecalendar" => new[] { "https://www.googleapis.com/auth/calendar.readonly" },
                _ => Array.Empty<string>(),
            };

            // Trigger authentication flow
            var accessToken = await _authenticationService.AcquireAccessTokenAsync(
                SelectedProvider,
                scopes,
                CancellationToken.None
            );

            if (!string.IsNullOrEmpty(accessToken))
            {
                SelectedAccount = new ProviderAccountItem
                {
                    ProviderName = SelectedProvider,
                    AccountLabel = string.IsNullOrWhiteSpace(AccountLabel)
                        ? SelectedProvider
                        : AccountLabel,
                    IsConnected = true,
                };

                StatusMessage = "Authentication successful!";
                IsAccepted = true;
            }
            else
            {
                StatusMessage = "Authentication failed. Please try again.";
                _logger.LogWarning(
                    "Authentication failed for provider {Provider}",
                    SelectedProvider
                );
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            _logger.LogError(
                ex,
                "Error during authentication for provider {Provider}",
                SelectedProvider
            );
        }
        finally
        {
            IsAuthenticating = false;
        }
    }
}
