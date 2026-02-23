using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Threading;

namespace ModalCalendarNotification;

public static class Program
{
    private static Mutex? _singleInstanceMutex;

    public static IServiceProvider CreateServiceProvider()
    {
        EnsureSingleInstance();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/modal-calendar-notification-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("Application bootstrap starting");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        ServiceConfiguration.Configure(services, configuration);
        services.AddSingleton(Log.Logger);
        return services.BuildServiceProvider();
    }

    public static void ReleaseSingleInstanceLock()
    {
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        _singleInstanceMutex = null;
        Log.CloseAndFlush();
    }

    private static void EnsureSingleInstance()
    {
        if (_singleInstanceMutex is not null)
        {
            return;
        }

        _singleInstanceMutex = new Mutex(true, "ModalCalendarNotification.SingleInstance", out var createdNew);

        if (!createdNew)
        {
            throw new InvalidOperationException("Another instance of ModalCalendarNotification is already running.");
        }
    }
}
