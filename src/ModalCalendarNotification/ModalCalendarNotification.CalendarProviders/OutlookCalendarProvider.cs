using ModalCalendarNotification.Core.Features.Authentication;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.CalendarProviders;

public sealed class OutlookCalendarProvider : ICalendarProvider
{
    private static readonly IReadOnlyList<string> Scopes = ["Calendars.Read"];
    private readonly IAuthenticationService _authenticationService;

    public OutlookCalendarProvider(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public string ProviderName => "Outlook365";

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

        var sourceEvents = new List<OutlookEventModel>();
        return sourceEvents.Select(OutlookEventMapper.Map).ToList();
    }
}
