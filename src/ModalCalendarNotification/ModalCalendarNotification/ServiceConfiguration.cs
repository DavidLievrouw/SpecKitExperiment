using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModalCalendarNotification.Core.Shared.Utilities;
using AppTimeProvider = ModalCalendarNotification.Core.Shared.Utilities.TimeProvider;

namespace ModalCalendarNotification;

public static class ServiceConfiguration
{
    public static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton(configuration);
        services.AddSingleton<AppTimeProvider>();
        services.AddSingleton<ApplicationLifecycleManager>();
    }
}
