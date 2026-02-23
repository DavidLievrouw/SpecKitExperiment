using System.Windows;

namespace ModalCalendarNotification.UI.Features.ConfigurationManagement;

public partial class ProviderSelectionDialog : Window
{
    private readonly ProviderSelectionViewModel _viewModel;

    public ProviderSelectionDialog(ProviderSelectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Subscribe to IsAccepted changes
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (
                e.PropertyName == nameof(ProviderSelectionViewModel.IsAccepted)
                && _viewModel.IsAccepted
            )
            {
                this.DialogResult = true;
                this.Close();
            }
        };
    }
}
