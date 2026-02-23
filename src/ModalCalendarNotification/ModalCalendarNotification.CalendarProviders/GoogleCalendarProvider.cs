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

    public GoogleCalendarProvider(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public string ProviderName => "GoogleCalendar";

    public async Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default
    )
    {
        _ = await _authenticationService.AcquireAccessTokenAsync(
            ProviderName,
            Scopes,
            cancellationToken
        );

        var sourceEvents = new List<GoogleEventModel>();
        return sourceEvents.ConvertAll(GoogleEventMapper.Map);
    }
}
