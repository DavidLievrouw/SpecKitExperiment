using ModalCalendarNotification.Core.Shared.Utilities;

namespace ModalCalendarNotification.UI.Features.NotificationManagement;

public sealed class DisplayMonitor
{
    public event EventHandler<DisplayInfo>? PrimaryDisplayChanged;

    public DisplayInfo CurrentPrimaryDisplay { get; private set; } = DisplayHelper.GetPrimaryDisplay();

    public void Refresh()
    {
        var latest = DisplayHelper.GetPrimaryDisplay();
        if (latest != CurrentPrimaryDisplay)
        {
            CurrentPrimaryDisplay = latest;
            PrimaryDisplayChanged?.Invoke(this, latest);
        }
    }
}
