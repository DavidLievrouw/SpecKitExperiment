using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ModalCalendarNotification.Core.Features.Authentication;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.CalendarProviders;

public sealed class OutlookCalendarProvider : ICalendarProvider
{
    private static readonly IReadOnlyList<string> Scopes = ["Calendars.Read"];
    private readonly IAuthenticationService _authenticationService;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public OutlookCalendarProvider(IAuthenticationService authenticationService, ILogger logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public string ProviderName => "Outlook365";

    public async Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var accessToken = await _authenticationService.AcquireAccessTokenAsync(
                ProviderName,
                Scopes,
                cancellationToken
            );

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Failed to acquire access token for Outlook provider");
                return [];
            }

            // Prepare the request to Microsoft Graph API
            var requestUrl =
                $"https://graph.microsoft.com/v1.0/me/events?$filter=start/dateTime ge '{fromUtc:O}' and end/dateTime le '{toUtc:O}'";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Microsoft Graph API returned {StatusCode}",
                    response.StatusCode
                );
                return [];
            }

            var jsonContent = await response.Content.ReadFromJsonAsync<OutlookApiResponse>(
                cancellationToken: cancellationToken
            );

            if (jsonContent?.Value == null)
            {
                return [];
            }

            return jsonContent.Value.ConvertAll(OutlookEventMapper.Map);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch events from Outlook provider");
            return [];
        }
    }

    public async Task<IReadOnlyList<Calendar>> GetAvailableCalendarsAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var accessToken = await _authenticationService.AcquireAccessTokenAsync(
                ProviderName,
                Scopes,
                cancellationToken
            );

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Failed to acquire access token for Outlook provider");
                return [];
            }

            // Fetch calendars from Microsoft Graph API
            const string requestUrl = "https://graph.microsoft.com/v1.0/me/calendars";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Microsoft Graph API returned {StatusCode} when fetching calendars",
                    response.StatusCode
                );
                return [];
            }

            var jsonContent = await response.Content.ReadFromJsonAsync<OutlookCalendarsResponse>(
                cancellationToken: cancellationToken
            );

            if (jsonContent?.Value == null)
            {
                return [];
            }

            return jsonContent.Value
                .ConvertAll(c => new Calendar { Id = c.Id, DisplayName = c.Name })
;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch calendars from Outlook provider");
            return [];
        }
    }

    private class OutlookApiResponse
    {
        public List<OutlookEventModel>? Value { get; set; }
    }

    private class OutlookCalendarsResponse
    {
        public List<OutlookCalendarModel>? Value { get; set; }
    }

    private class OutlookCalendarModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
