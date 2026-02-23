using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.UI.Features.StartupRecovery;

public partial class StartupMissedEventsViewModel : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyList<CalendarEvent> _missedEvents = [];

    [ObservableProperty]
    private bool _isClosed;

    [RelayCommand]
    private void Close()
    {
        IsClosed = true;
    }
}
