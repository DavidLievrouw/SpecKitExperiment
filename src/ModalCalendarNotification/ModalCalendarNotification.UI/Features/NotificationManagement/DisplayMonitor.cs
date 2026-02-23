using ModalCalendarNotification.Core.Shared.Utilities;

namespace ModalCalendarNotification.UI.Features.NotificationManagement;

public sealed class DisplayMonitor
{
    public DisplayInfo CurrentPrimaryDisplay { get; private set; } =
        DisplayHelper.GetPrimaryDisplay();
    public event EventHandler<DisplayInfo>? PrimaryDisplayChanged;

    public void Refresh()
    {
        DisplayInfo latest = DisplayHelper.GetPrimaryDisplay();
        if (latest != CurrentPrimaryDisplay)
        {
            CurrentPrimaryDisplay = latest;
            PrimaryDisplayChanged?.Invoke(this, latest);
        }
    }
}
