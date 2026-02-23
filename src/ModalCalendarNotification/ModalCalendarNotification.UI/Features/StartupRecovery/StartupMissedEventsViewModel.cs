using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.UI.Features.StartupRecovery;

public partial class StartupMissedEventsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isClosed;

    [ObservableProperty]
    private IReadOnlyList<CalendarEvent> _missedEvents = [];

    [RelayCommand]
    private void Close()
    {
        IsClosed = true;
    }
}
