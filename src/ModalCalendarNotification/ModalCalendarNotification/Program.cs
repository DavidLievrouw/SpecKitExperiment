using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ModalCalendarNotification;

public static class Program
{
    private const string SingleInstanceMutexName = "ModalCalendarNotification.SingleInstance";
    private static Mutex? _singleInstanceMutex;

    public static IServiceProvider CreateServiceProvider()
    {
        EnsureSingleInstance();
        ConfigureLogging();
        Log.Information("Application bootstrap starting");

        IConfiguration configuration = BuildConfiguration();
        var services = new ServiceCollection();

        ServiceConfiguration.Configure(services, configuration);
        services.AddSingleton(Log.Logger);

        return services.BuildServiceProvider();
    }

    public static void ReleaseSingleInstanceLock()
    {
        if (Interlocked.Exchange(ref _singleInstanceMutex, null) is { } mutex)
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
        }

        Log.CloseAndFlush();
    }

    private static void EnsureSingleInstance()
    {
        if (_singleInstanceMutex is not null)
        {
            return;
        }

        var mutex = new Mutex(true, SingleInstanceMutexName, out bool createdNew);

        if (!createdNew)
        {
            mutex.Dispose();
            throw new InvalidOperationException(
                "Another instance of ModalCalendarNotification is already running."
            );
        }

        _singleInstanceMutex = mutex;
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .Build();
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                "logs/modal-calendar-notification-.log",
                rollingInterval: RollingInterval.Day
            )
            .CreateLogger();
    }
}
