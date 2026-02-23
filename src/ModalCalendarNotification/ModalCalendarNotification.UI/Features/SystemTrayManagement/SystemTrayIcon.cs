namespace ModalCalendarNotification.UI.Features.SystemTrayManagement;

public sealed class SystemTrayIcon
{
    private readonly SystemTrayViewModel _viewModel;

    public SystemTrayIcon(SystemTrayViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public IReadOnlyList<string> MenuItems => ["Settings", "Exit"];

    public void TriggerSettings()
    {
        _viewModel.OpenSettingsCommand.Execute(null);
    }

    public void TriggerExit()
    {
        _viewModel.ExitApplicationCommand.Execute(null);
    }
}
