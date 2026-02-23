using ModalCalendarNotification.Core.Features.ConfigurationManagement;
using ModalCalendarNotification.Core.Shared.Models;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.ConfigurationManagement;

public sealed class ConfigurationServiceTests
{
    [Fact]
    public async Task SaveAndLoad_PersistsConfigurationValues()
    {
        var path = Path.Combine(Path.GetTempPath(), $"modal-config-{Guid.NewGuid():N}.json");
        var sut = new ConfigurationService(path);

        var expected = new ApplicationConfiguration
        {
            NotificationLeadTimeMinutes = 7,
            AutoDismissTimeoutSeconds = 45,
            ActiveProvider = "GoogleCalendar",
        };

        await sut.SaveAsync(expected, TestContext.Current.CancellationToken);
        var actual = await sut.LoadAsync(TestContext.Current.CancellationToken);

        actual.NotificationLeadTimeMinutes.ShouldBe(7);
        actual.AutoDismissTimeoutSeconds.ShouldBe(45);
        actual.ActiveProvider.ShouldBe("GoogleCalendar");

        File.Delete(path);
    }
}
