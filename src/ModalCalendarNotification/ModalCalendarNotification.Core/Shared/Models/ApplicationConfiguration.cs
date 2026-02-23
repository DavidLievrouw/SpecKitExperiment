using ModalCalendarNotification.Core.Features.ConfigurationManagement;

namespace ModalCalendarNotification.Core.Shared.Models;

public sealed record ApplicationConfiguration
{
    public int NotificationLeadTimeMinutes { get; init; } = 3;

    public int SnoozeDurationMinutes { get; init; } = 5;

    public int AutoDismissTimeoutSeconds { get; init; } = 60;

    public string ActiveProvider { get; init; } = "Outlook365";

    public bool IsStartupRecoveryEnabled { get; init; } = true;

    public IReadOnlyList<ProviderAccountItem> ProviderAccounts { get; init; } = [];
}
