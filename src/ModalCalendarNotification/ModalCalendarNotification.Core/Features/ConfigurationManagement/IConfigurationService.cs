using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Core.Features.ConfigurationManagement;

public interface IConfigurationService
{
    Task<ApplicationConfiguration> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(
        ApplicationConfiguration configuration,
        CancellationToken cancellationToken = default
    );
}
