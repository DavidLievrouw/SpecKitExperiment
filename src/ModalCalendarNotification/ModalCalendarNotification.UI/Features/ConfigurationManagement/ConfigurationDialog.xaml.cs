using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;
using ModalCalendarNotification.UI.Features.CalendarSelection;
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
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (
                e.PropertyName == nameof(ConfigurationDialogViewModel.IsCalendarSelectionRequested)
                && _viewModel.IsCalendarSelectionRequested
            )
            {
                // Check if we're adding a new provider or selecting calendars for existing one
                if (_viewModel.SelectedProviderAccount == null)
                {
                    OpenProviderSelectionDialog();
                }
                else
                {
                    OpenCalendarSelectionDialog(_viewModel.SelectedProviderAccount);
                }
            }
        };
    }

    private async void OpenProviderSelectionDialog()
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
                // Add the new provider to the list if not already present
                var existing = _viewModel.ConfiguredProviders.FirstOrDefault(p =>
                    p.ProviderName == viewModel.SelectedAccount.ProviderName
                    && p.AccountLabel == viewModel.SelectedAccount.AccountLabel
                );

                if (existing == null)
                {
                    _viewModel.ConfiguredProviders.Add(viewModel.SelectedAccount);
                }

                // Update the active provider
                _viewModel.SelectedProvider = viewModel.SelectedAccount.ProviderName;

                // Now show calendar selection dialog for the newly authenticated provider
                await OpenCalendarSelectionDialogForNewProviderAsync(viewModel.SelectedAccount);
            }
        }

        // Reset the flag
        _viewModel.IsCalendarSelectionRequested = false;
        _viewModel.SelectedProviderAccount = null;
    }

    private async Task OpenCalendarSelectionDialogForNewProviderAsync(ProviderAccountItem provider)
    {
        // Get calendar list view model and load available calendars from the provider
        var calendarListViewModel = _serviceProvider.GetRequiredService<CalendarListViewModel>();

        // Load available calendars from the newly authenticated provider
        await calendarListViewModel.LoadAvailableCalendarsAsync(provider.ProviderName);

        // Show the calendar selection dialog
        var dialog = new CalendarListDialog(calendarListViewModel)
        {
            Owner = this,
            Title = $"Select Calendars - {provider.AccountLabel}"
        };

        var result = dialog.ShowDialog();

        if (result == true)
        {
            // Calendar selections have been saved by the dialog
        }
    }

    private void OpenCalendarSelectionDialog(ProviderAccountItem provider)
    {
        // Create a new instance of the dialog
        var calendarListViewModel = _serviceProvider.GetRequiredService<CalendarListViewModel>();

        // Show the calendar selection dialog
        var dialog = new CalendarListDialog(calendarListViewModel)
        {
            Owner = this,
            Title = $"Select Calendars - {provider.AccountLabel}"
        };

        var result = dialog.ShowDialog();

        if (result == true)
        {
            // Calendar selections have been saved by the dialog
        }

        // Reset the flag
        _viewModel.IsCalendarSelectionRequested = false;
        _viewModel.SelectedProviderAccount = null;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
