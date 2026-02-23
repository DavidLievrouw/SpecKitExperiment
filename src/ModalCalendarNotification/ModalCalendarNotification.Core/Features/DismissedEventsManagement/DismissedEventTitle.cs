namespace ModalCalendarNotification.Core.Features.DismissedEventsManagement;

public sealed record DismissedEventTitle
{
    public required string Title { get; init; }

    public DateTimeOffset DismissedAtUtc { get; init; }
}
