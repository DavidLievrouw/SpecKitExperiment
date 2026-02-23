using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModalCalendarNotification.Core.Features.CalendarSelection;

namespace ModalCalendarNotification.UI.Features.CalendarSelection;

public partial class CalendarListViewModel : ObservableObject
{
    private readonly ICalendarSelectionRepository _calendarSelectionRepository;

    public CalendarListViewModel(ICalendarSelectionRepository calendarSelectionRepository)
    {
        _calendarSelectionRepository = calendarSelectionRepository;
    }

    [ObservableProperty]
    private IReadOnlyList<CalendarSelectionItem> _items = [];

    [ObservableProperty]
    private bool _isSaved;

    [RelayCommand]
    private async Task LoadAsync()
    {
        var selections = await _calendarSelectionRepository.GetSelectedAsync();
        Items = selections
            .Select(x => new CalendarSelectionItem(x.ProviderName, x.CalendarId, x.DisplayName, x.IsSelected))
            .ToList();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var selected = Items
            .Select(x => new SelectedCalendar
            {
                ProviderName = x.ProviderName,
                CalendarId = x.CalendarId,
                DisplayName = x.DisplayName,
                IsSelected = x.IsSelected
            })
            .ToList();

        await _calendarSelectionRepository.SaveSelectedAsync(selected);
        IsSaved = true;
    }
}

public sealed partial class CalendarSelectionItem : ObservableObject
{
    public CalendarSelectionItem(string providerName, string calendarId, string displayName, bool isSelected)
    {
        ProviderName = providerName;
        CalendarId = calendarId;
        DisplayName = displayName;
        _isSelected = isSelected;
    }

    public string ProviderName { get; }

    public string CalendarId { get; }

    public string DisplayName { get; }

    [ObservableProperty]
    private bool _isSelected;
}
