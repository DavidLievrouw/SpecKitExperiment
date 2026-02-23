namespace ModalCalendarNotification.Core.Shared.Models;

public sealed record Notification
{
    public required string Id { get; init; }

    public required string EventId { get; init; }

    public required string Title { get; init; }

    public required DateTimeOffset TriggerAtUtc { get; init; }

    public int SnoozeCount { get; init; }

    public bool IsDismissed { get; init; }
}
