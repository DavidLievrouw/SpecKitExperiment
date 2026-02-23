using System.Text.Json;
using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Core.Features.ConfigurationManagement;

public sealed class ConfigurationService : IConfigurationService
{
    private readonly string _configurationPath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public ConfigurationService(string? configurationPath = null)
    {
        _configurationPath =
            configurationPath
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ModalCalendarNotification",
                "configuration.json"
            );
    }

    public async Task<ApplicationConfiguration> LoadAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (!File.Exists(_configurationPath))
        {
            return new ApplicationConfiguration();
        }

        await using FileStream stream = File.OpenRead(_configurationPath);
        var configuration = await JsonSerializer.DeserializeAsync<ApplicationConfiguration>(
            stream,
            _jsonOptions,
            cancellationToken
        );
        return configuration ?? new ApplicationConfiguration();
    }

    public async Task SaveAsync(
        ApplicationConfiguration configuration,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string? directory = Path.GetDirectoryName(_configurationPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream stream = File.Create(_configurationPath);
        await JsonSerializer.SerializeAsync(stream, configuration, _jsonOptions, cancellationToken);
    }
}
