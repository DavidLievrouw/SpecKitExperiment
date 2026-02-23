namespace ModalCalendarNotification.Core.Features.CalendarSelection;

public interface ICalendarSelectionRepository
{
    Task<IReadOnlyList<SelectedCalendar>> GetSelectedAsync(
        CancellationToken cancellationToken = default
    );

    Task SaveSelectedAsync(
        IReadOnlyList<SelectedCalendar> selectedCalendars,
        CancellationToken cancellationToken = default
    );
}
