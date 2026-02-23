using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using ModalCalendarNotification.UI.Features.SystemTrayManagement;

namespace ModalCalendarNotification.UI.Features.ConfigurationManagement;

public partial class ConfigurationDialog : Window
{
    private readonly ConfigurationDialogViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public ConfigurationDialog(
        ConfigurationDialogViewModel viewModel,
        SystemTrayManager systemTrayManager,
        IServiceProvider serviceProvider
    )
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
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
        // Create a new instance of the dialog each time (WPF windows can only be shown once)
        var providerSelectionDialog =
            _serviceProvider.GetRequiredService<ProviderSelectionDialog>();

        // Show the provider selection dialog as a modal child window
        providerSelectionDialog.Owner = this;
        var result = providerSelectionDialog.ShowDialog();

        if (result == true)
        {
            // Get the selected provider account from the ProviderSelectionViewModel
            var viewModel = providerSelectionDialog.DataContext as ProviderSelectionViewModel;
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
