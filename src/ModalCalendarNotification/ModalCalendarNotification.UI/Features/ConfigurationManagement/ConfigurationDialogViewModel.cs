using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;
using ModalCalendarNotification.Core.Features.DismissedEventsManagement;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.UI.Features.ConfigurationManagement;

public partial class ConfigurationDialogViewModel : ObservableObject
{
    private readonly IConfigurationService _configurationService;
    private readonly IDismissedEventTitleRepository _dismissedEventTitleRepository;

    [ObservableProperty]
    private int _autoDismissTimeoutSeconds = 60;

    [ObservableProperty]
    private bool _isCalendarSelectionRequested;

    [ObservableProperty]
    private bool _isSaved;

    [ObservableProperty]
    private int _notificationLeadTimeMinutes = 3;

    [ObservableProperty]
    private string _selectedProvider = "Outlook365";

    [ObservableProperty]
    private ProviderAccountItem? _selectedProviderAccount;

    [ObservableProperty]
    private string? _selectedDismissedTitle;

    public ObservableCollection<ProviderAccountItem> ConfiguredProviders { get; } =
        new ObservableCollection<ProviderAccountItem>();

    public ObservableCollection<string> DismissedEventTitles { get; } =
        new ObservableCollection<string>();

    public ConfigurationDialogViewModel(
        IConfigurationService configurationService,
        IDismissedEventTitleRepository dismissedEventTitleRepository
    )
    {
        _configurationService = configurationService;
        _dismissedEventTitleRepository = dismissedEventTitleRepository;
        ApplicationVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    }

    public string ApplicationVersion { get; }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var config = await _configurationService.LoadAsync();
        NotificationLeadTimeMinutes = config.NotificationLeadTimeMinutes;
        AutoDismissTimeoutSeconds = config.AutoDismissTimeoutSeconds;
        SelectedProvider = config.ActiveProvider;

        // Load configured providers
        ConfiguredProviders.Clear();
        foreach (var account in config.ProviderAccounts)
        {
            ConfiguredProviders.Add(account);
        }

        // Load dismissed event titles
        DismissedEventTitles.Clear();
        var dismissedTitles = await _dismissedEventTitleRepository.GetAllAsync();
        foreach (var title in dismissedTitles.OrderBy(x => x.Title))
        {
            DismissedEventTitles.Add(title.Title);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var configuration = new ApplicationConfiguration
        {
            NotificationLeadTimeMinutes = NotificationLeadTimeMinutes,
            AutoDismissTimeoutSeconds = AutoDismissTimeoutSeconds,
            ActiveProvider = SelectedProvider,
            ProviderAccounts = ConfiguredProviders.ToList(),
        };

        await _configurationService.SaveAsync(configuration);
        IsSaved = true;
    }

    [RelayCommand]
    private async Task RestoreDismissedTitleAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedDismissedTitle))
        {
            return;
        }

        await _dismissedEventTitleRepository.RemoveAsync(SelectedDismissedTitle);
        DismissedEventTitles.Remove(SelectedDismissedTitle);
        SelectedDismissedTitle = null;
    }

    [RelayCommand]
    private void AddProvider()
    {
        // Trigger the provider selection dialog
        IsCalendarSelectionRequested = true;
    }

    [RelayCommand]
    private void RemoveProvider(ProviderAccountItem? provider)
    {
        if (provider != null)
        {
            ConfiguredProviders.Remove(provider);
        }
    }

    [RelayCommand]
    private void SelectCalendars(ProviderAccountItem? provider)
    {
        if (provider != null)
        {
            SelectedProviderAccount = provider;
            IsCalendarSelectionRequested = true;
        }
    }

    [RelayCommand]
    private void OpenCalendarSelection()
    {
        IsCalendarSelectionRequested = true;
    }
}
