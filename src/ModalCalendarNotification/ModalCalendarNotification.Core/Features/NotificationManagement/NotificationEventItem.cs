namespace ModalCalendarNotification.Core.Features.NotificationManagement;

public sealed record NotificationEventItem
{
    public required string EventId { get; init; }

    public required string Title { get; init; }

    public DateTimeOffset StartUtc { get; init; }

    public string Provider { get; init; } = string.Empty;

    public string ProviderAccountLabel { get; init; } = string.Empty;
}
