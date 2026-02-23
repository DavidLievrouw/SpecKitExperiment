using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModalCalendarNotification.Core.Features.DismissedEventsManagement;
using ModalCalendarNotification.Core.Features.NotificationManagement;

namespace ModalCalendarNotification.UI.Features.NotificationManagement;

public partial class NotificationModalViewModel : ObservableObject
{
    private readonly IDismissedEventTitleRepository? _dismissedEventTitleRepository;

    [ObservableProperty]
    private IReadOnlyList<NotificationEventItem> _events = [];

    [ObservableProperty]
    private bool _isClosed;

    [ObservableProperty]
    private string? _selectedEventTitle;

    [ObservableProperty]
    private int _snoozeMinutes = 5;

    [ObservableProperty]
    private string _title = "Upcoming Events";

    public NotificationModalViewModel() { }

    public NotificationModalViewModel(IDismissedEventTitleRepository dismissedEventTitleRepository)
    {
        _dismissedEventTitleRepository = dismissedEventTitleRepository;
    }

    partial void OnEventsChanged(IReadOnlyList<NotificationEventItem> value)
    {
        SelectedEventTitle = value.FirstOrDefault()?.Title;
    }

    [RelayCommand]
    private void Dismiss()
    {
        IsClosed = true;
    }

    [RelayCommand]
    private void Snooze()
    {
        IsClosed = true;
    }

    [RelayCommand]
    private async Task DismissAllFutureAsync()
    {
        if (
            !string.IsNullOrWhiteSpace(SelectedEventTitle)
            && _dismissedEventTitleRepository is not null
        )
        {
            await _dismissedEventTitleRepository.AddAsync(SelectedEventTitle);
        }

        IsClosed = true;
    }
}
