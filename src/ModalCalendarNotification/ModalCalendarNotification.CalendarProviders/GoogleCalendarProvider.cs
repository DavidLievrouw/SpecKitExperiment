using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ModalCalendarNotification.Core.Features.Authentication;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.CalendarProviders;

public sealed class GoogleCalendarProvider : ICalendarProvider
{
    private static readonly IReadOnlyList<string> Scopes =
    [
        "https://www.googleapis.com/auth/calendar.readonly",
    ];
    private readonly IAuthenticationService _authenticationService;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public GoogleCalendarProvider(IAuthenticationService authenticationService, ILogger logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public string ProviderName => "GoogleCalendar";

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
                _logger.LogWarning("Failed to acquire access token for Google Calendar provider");
                return [];
            }

            // Prepare the request to Google Calendar API
            var timeMin = Uri.EscapeDataString(fromUtc.ToString("O"));
            var timeMax = Uri.EscapeDataString(toUtc.ToString("O"));
            var requestUrl =
                $"https://www.googleapis.com/calendar/v3/calendars/primary/events?timeMin={timeMin}&timeMax={timeMax}&singleEvents=true&maxResults=250";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Google Calendar API returned {StatusCode}",
                    response.StatusCode
                );
                return [];
            }

            var jsonContent = await response.Content.ReadFromJsonAsync<GoogleCalendarApiResponse>(
                cancellationToken: cancellationToken
            );

            if (jsonContent?.Items == null)
            {
                return [];
            }

            return jsonContent.Items.ConvertAll(GoogleEventMapper.Map);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch events from Google Calendar provider");
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
                _logger.LogWarning("Failed to acquire access token for Google Calendar provider");
                return [];
            }

            // Fetch calendar list from Google Calendar API
            const string requestUrl = "https://www.googleapis.com/calendar/v3/users/me/calendarList";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Google Calendar API returned {StatusCode} when fetching calendars",
                    response.StatusCode
                );
                return [];
            }

            var jsonContent = await response.Content.ReadFromJsonAsync<GoogleCalendarsResponse>(
                cancellationToken: cancellationToken
            );

            if (jsonContent?.Items == null)
            {
                return [];
            }

            return jsonContent.Items
                .Select(c => new Calendar { Id = c.Id, DisplayName = c.Summary })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch calendars from Google Calendar provider");
            return [];
        }
    }

    private class GoogleCalendarApiResponse
    {
        public List<GoogleEventModel>? Items { get; set; }
    }

    private class GoogleCalendarsResponse
    {
        public List<GoogleCalendarModel>? Items { get; set; }
    }

    private class GoogleCalendarModel
    {
        public string Id { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }
}
