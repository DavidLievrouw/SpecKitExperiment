using Microsoft.EntityFrameworkCore;
using ModalCalendarNotification.Core.Features.CalendarSelection;

namespace ModalCalendarNotification.Data.Features.CalendarSelection;

public sealed class CalendarSelectionRepository : ICalendarSelectionRepository
{
    private readonly AppDbContext _dbContext;

    public CalendarSelectionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SelectedCalendar>> GetSelectedAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .SelectedCalendars.OrderBy(x => x.ProviderName)
            .ThenBy(x => x.DisplayName)
            .Select(x => new SelectedCalendar
            {
                ProviderName = x.ProviderName,
                CalendarId = x.CalendarId,
                DisplayName = x.DisplayName,
                IsSelected = x.IsSelected,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task SaveSelectedAsync(
        IReadOnlyList<SelectedCalendar> selectedCalendars,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(selectedCalendars);

        var existing = await _dbContext.SelectedCalendars.ToListAsync(cancellationToken);
        _dbContext.SelectedCalendars.RemoveRange(existing);

        foreach (var selectedCalendar in selectedCalendars)
        {
            _dbContext.SelectedCalendars.Add(
                new SelectedCalendarEntity
                {
                    ProviderName = selectedCalendar.ProviderName,
                    CalendarId = selectedCalendar.CalendarId,
                    DisplayName = selectedCalendar.DisplayName,
                    IsSelected = selectedCalendar.IsSelected,
                }
            );
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
