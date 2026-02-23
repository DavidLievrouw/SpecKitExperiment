namespace ModalCalendarNotification.Core.Features.CalendarSelection;

public sealed record SelectedCalendar
{
    public required string ProviderName { get; init; }

    public required string CalendarId { get; init; }

    public required string DisplayName { get; init; }

    public bool IsSelected { get; init; }
}
