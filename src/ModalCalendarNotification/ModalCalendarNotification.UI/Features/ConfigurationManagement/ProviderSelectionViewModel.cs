using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;

namespace ModalCalendarNotification.UI.Features.ConfigurationManagement;

public partial class ProviderSelectionViewModel : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyList<string> _availableProviders = ["Outlook365", "GoogleCalendar"];

    [ObservableProperty]
    private string _selectedProvider = "Outlook365";

    [ObservableProperty]
    private string _accountLabel = string.Empty;

    [ObservableProperty]
    private bool _isAccepted;

    public ProviderAccountItem? SelectedAccount { get; private set; }

    [RelayCommand]
    private void Confirm()
    {
        SelectedAccount = new ProviderAccountItem
        {
            ProviderName = SelectedProvider,
            AccountLabel = string.IsNullOrWhiteSpace(AccountLabel) ? SelectedProvider : AccountLabel,
            IsConnected = false
        };

        IsAccepted = true;
    }
}
