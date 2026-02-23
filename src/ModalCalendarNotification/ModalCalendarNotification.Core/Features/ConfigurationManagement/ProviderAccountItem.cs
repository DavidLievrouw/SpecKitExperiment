namespace ModalCalendarNotification.Core.Features.ConfigurationManagement;

public sealed record ProviderAccountItem
{
    public required string ProviderName { get; init; }

    public required string AccountLabel { get; init; }

    public bool IsConnected { get; init; }
}
