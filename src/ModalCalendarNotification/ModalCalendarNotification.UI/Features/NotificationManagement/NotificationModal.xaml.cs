using System.Windows;

namespace ModalCalendarNotification.UI.Features.NotificationManagement;

public partial class NotificationModal : Window
{
    public NotificationModal(NotificationModalViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Topmost = true;
    }
}