using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ModalCalendarNotification.UI.Features.SystemTrayManagement;

public partial class SystemTrayViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSettingsRequested;

    [ObservableProperty]
    private bool _isExitRequested;

    [RelayCommand]
    private void OpenSettings()
    {
        IsSettingsRequested = true;
    }

    [RelayCommand]
    private void ExitApplication()
    {
        IsExitRequested = true;
    }
}
