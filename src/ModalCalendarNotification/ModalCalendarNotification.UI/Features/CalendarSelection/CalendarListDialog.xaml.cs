using System.Windows;

namespace ModalCalendarNotification.UI.Features.CalendarSelection;

public partial class CalendarListDialog : Window
{
    public CalendarListDialog(CalendarListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
