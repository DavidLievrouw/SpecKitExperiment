namespace ModalCalendarNotification.Core.Shared.Models;

public sealed record Calendar
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }
}

