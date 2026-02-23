using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.CalendarProviders;

public static class OutlookEventMapper
{
    public static CalendarEvent Map(OutlookEventModel source)
    {
        return new CalendarEvent
        {
            Id = source.Id,
            Title = source.Subject,
            Description = source.Body,
            StartUtc = source.StartUtc,
            EndUtc = source.EndUtc,
            Location = source.Location,
            Provider = "Outlook365",
            CalendarId = source.CalendarId,
            IsAllDay = source.IsAllDay
        };
    }
}

public sealed record OutlookEventModel(
    string Id,
    string Subject,
    string? Body,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    string? Location,
    string CalendarId,
    bool IsAllDay);
