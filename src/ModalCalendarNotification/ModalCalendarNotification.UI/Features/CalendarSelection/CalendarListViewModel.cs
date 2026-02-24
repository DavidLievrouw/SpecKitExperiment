using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModalCalendarNotification.Core.Features.CalendarIntegration;
using ModalCalendarNotification.Core.Features.CalendarSelection;

namespace ModalCalendarNotification.UI.Features.CalendarSelection;

public partial class CalendarListViewModel : ObservableObject
{
    private readonly ICalendarSelectionRepository _calendarSelectionRepository;
    private readonly ICalendarProviderFactory? _providerFactory;

    [ObservableProperty]
    private bool _isSaved;

    [ObservableProperty]
    private ObservableCollection<CalendarSelectionItem> _items = [];

    [ObservableProperty]
    private string? _currentProvider;

    public CalendarListViewModel(ICalendarSelectionRepository calendarSelectionRepository)
    {
        _calendarSelectionRepository = calendarSelectionRepository;
        _providerFactory = null;
    }

    public CalendarListViewModel(
        ICalendarSelectionRepository calendarSelectionRepository,
        ICalendarProviderFactory providerFactory
    )
    {
        _calendarSelectionRepository = calendarSelectionRepository;
        _providerFactory = providerFactory;
    }

    /// <summary>
    /// Load available calendars from a specific provider (for new provider setup)
    /// </summary>
    public async Task LoadAvailableCalendarsAsync(
        string providerName,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            CurrentProvider = providerName;

            if (_providerFactory == null)
            {
                Items.Clear();
                return;
            }

            var availableCalendars = await _providerFactory.GetAvailableCalendarsAsync(
                providerName,
                cancellationToken
            );

            // Default all calendars to selected for new providers
            Items.Clear();
            foreach (var cal in availableCalendars)
            {
                Items.Add(new CalendarSelectionItem(
                    providerName,
                    cal.Id,
                    cal.DisplayName,
                    isSelected: true // Default to selected
                ));
            }
        }
        catch (Exception ex)
        {
            // Log or handle error appropriately
            Items.Clear();
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var selections = await _calendarSelectionRepository.GetSelectedAsync();
        Items.Clear();
        foreach (var x in selections)
        {
            Items.Add(new CalendarSelectionItem(
                x.ProviderName,
                x.CalendarId,
                x.DisplayName,
                x.IsSelected
            ));
        }
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
                IsSelected = x.IsSelected,
            })
            .ToList();

        await _calendarSelectionRepository.SaveSelectedAsync(selected);
        IsSaved = true;
    }
}

public sealed partial class CalendarSelectionItem : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public CalendarSelectionItem(
        string providerName,
        string calendarId,
        string displayName,
        bool isSelected
    )
    {
        ProviderName = providerName;
        CalendarId = calendarId;
        DisplayName = displayName;
        _isSelected = isSelected;
    }

    public string ProviderName { get; }

    public string CalendarId { get; }

    public string DisplayName { get; }
}
