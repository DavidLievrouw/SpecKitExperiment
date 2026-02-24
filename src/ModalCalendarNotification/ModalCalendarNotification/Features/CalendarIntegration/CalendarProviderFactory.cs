using ModalCalendarNotification.CalendarProviders;
using ModalCalendarNotification.Core.Features.CalendarIntegration;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Features.CalendarIntegration;

public sealed class CalendarProviderFactory : ICalendarProviderFactory
{
    private readonly OutlookCalendarProvider _outlookProvider;
    private readonly GoogleCalendarProvider _googleProvider;

    public CalendarProviderFactory(
        OutlookCalendarProvider outlookProvider,
        GoogleCalendarProvider googleProvider
    )
    {
        _outlookProvider = outlookProvider;
        _googleProvider = googleProvider;
    }

    public async Task<IReadOnlyList<Calendar>> GetAvailableCalendarsAsync(
        string providerName,
        CancellationToken cancellationToken = default
    )
    {
        var provider = providerName switch
        {
            "Outlook365" => (ICalendarProvider)_outlookProvider,
            "GoogleCalendar" => (ICalendarProvider)_googleProvider,
            _ => null,
        };

        if (provider == null)
        {
            return [];
        }

        return await provider.GetAvailableCalendarsAsync(cancellationToken);
    }
}


