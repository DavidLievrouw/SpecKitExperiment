using System.Windows;

namespace ModalCalendarNotification.UI.Features.StartupRecovery;

public partial class StartupMissedEventsModal : Window
{
    public StartupMissedEventsModal(StartupMissedEventsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
