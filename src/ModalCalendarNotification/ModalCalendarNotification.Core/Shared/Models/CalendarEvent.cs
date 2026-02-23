namespace ModalCalendarNotification.Core.Shared.Models;

public sealed record CalendarEvent
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset StartUtc { get; init; }

    public DateTimeOffset EndUtc { get; init; }

    public string? Location { get; init; }

    public string Provider { get; init; } = string.Empty;

    public string CalendarId { get; init; } = string.Empty;

    public bool IsAllDay { get; init; }
}