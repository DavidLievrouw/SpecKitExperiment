using System.Windows;
using System.Windows.Media.Imaging;
using ModalCalendarNotification.UI.Features.SystemTrayManagement;

namespace ModalCalendarNotification.UI.Features.ConfigurationManagement;

public partial class ConfigurationDialog : Window
{
    private readonly ConfigurationDialogViewModel _viewModel;

    public ConfigurationDialog(
        ConfigurationDialogViewModel viewModel,
        SystemTrayManager systemTrayManager
    )
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Set the window icon from the SystemTrayManager
        try
        {
            var icon = systemTrayManager.GetIcon();
            if (icon is BitmapImage bitmapImage && bitmapImage.UriSource != null)
            {
                this.Icon = bitmapImage;
            }
        }
        catch
        {
            // If icon loading fails, just continue without it
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
