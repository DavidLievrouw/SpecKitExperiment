using System.Windows;

namespace ModalCalendarNotification.UI.Features.ConfigurationManagement;

public partial class ProviderSelectionDialog : Window
{
    public ProviderSelectionDialog(ProviderSelectionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
