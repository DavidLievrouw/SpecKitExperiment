using System.Windows;
using System.Windows.Media.Imaging;
using ModalCalendarNotification.UI.Features.SystemTrayManagement;

namespace ModalCalendarNotification.UI.Features.ConfigurationManagement;

public partial class ConfigurationDialog : Window
{
    private readonly ConfigurationDialogViewModel _viewModel;
    private readonly ProviderSelectionDialog _providerSelectionDialog;

    public ConfigurationDialog(
        ConfigurationDialogViewModel viewModel,
        SystemTrayManager systemTrayManager,
        ProviderSelectionDialog providerSelectionDialog
    )
    {
        InitializeComponent();
        _viewModel = viewModel;
        _providerSelectionDialog = providerSelectionDialog;
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

        // Subscribe to property changes
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (
                e.PropertyName == nameof(ConfigurationDialogViewModel.IsCalendarSelectionRequested)
                && _viewModel.IsCalendarSelectionRequested
            )
            {
                OpenProviderSelectionDialog();
            }
        };
    }

    private void OpenProviderSelectionDialog()
    {
        // Show the provider selection dialog as a modal child window
        _providerSelectionDialog.Owner = this;
        var result = _providerSelectionDialog.ShowDialog();

        if (result == true)
        {
            // Get the selected provider account from the ProviderSelectionViewModel
            var viewModel = _providerSelectionDialog.DataContext as ProviderSelectionViewModel;
            if (viewModel?.SelectedAccount != null)
            {
                // Update the selected provider in the configuration dialog
                _viewModel.SelectedProvider = viewModel.SelectedAccount.ProviderName;
            }
        }

        // Reset the flag
        _viewModel.IsCalendarSelectionRequested = false;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
