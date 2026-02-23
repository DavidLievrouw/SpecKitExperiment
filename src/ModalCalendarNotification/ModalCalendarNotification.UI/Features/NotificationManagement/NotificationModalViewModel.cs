using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModalCalendarNotification.Core.Features.DismissedEventsManagement;
using ModalCalendarNotification.Core.Features.NotificationManagement;

namespace ModalCalendarNotification.UI.Features.NotificationManagement;

public partial class NotificationModalViewModel : ObservableObject
{
    private readonly IDismissedEventTitleRepository? _dismissedEventTitleRepository;

    public NotificationModalViewModel()
    {
    }

    public NotificationModalViewModel(IDismissedEventTitleRepository dismissedEventTitleRepository)
    {
        _dismissedEventTitleRepository = dismissedEventTitleRepository;
    }

    [ObservableProperty]
    private string _title = "Upcoming Events";

    [ObservableProperty]
    private IReadOnlyList<NotificationEventItem> _events = [];

    partial void OnEventsChanged(IReadOnlyList<NotificationEventItem> value)
    {
        SelectedEventTitle = value.FirstOrDefault()?.Title;
    }

    [ObservableProperty]
    private bool _isClosed;

    [ObservableProperty]
    private int _snoozeMinutes = 5;

    [ObservableProperty]
    private string? _selectedEventTitle;

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
        if (!string.IsNullOrWhiteSpace(SelectedEventTitle) && _dismissedEventTitleRepository is not null)
        {
            await _dismissedEventTitleRepository.AddAsync(SelectedEventTitle);
        }

        IsClosed = true;
    }
}
