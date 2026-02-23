using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.CalendarProviders;

public static class GoogleEventMapper
{
    public static CalendarEvent Map(GoogleEventModel source)
    {
        return new CalendarEvent
        {
            Id = source.Id,
            Title = source.Summary,
            Description = source.Description,
            StartUtc = source.StartUtc,
            EndUtc = source.EndUtc,
            Location = source.Location,
            Provider = "GoogleCalendar",
            CalendarId = source.CalendarId,
            IsAllDay = source.IsAllDay,
        };
    }
}

public sealed record GoogleEventModel(
    string Id,
    string Summary,
    string? Description,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    string? Location,
    string CalendarId,
    bool IsAllDay
);
