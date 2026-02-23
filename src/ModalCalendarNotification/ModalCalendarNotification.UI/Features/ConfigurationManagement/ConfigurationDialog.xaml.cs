using System.Windows;

namespace ModalCalendarNotification.UI.Features.ConfigurationManagement;

public partial class ConfigurationDialog : Window
{
    public ConfigurationDialog(ConfigurationDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}