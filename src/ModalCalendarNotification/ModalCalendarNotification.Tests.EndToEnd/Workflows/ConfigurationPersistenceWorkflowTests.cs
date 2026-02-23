using System.IO;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;
using ModalCalendarNotification.Core.Shared.Models;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.EndToEnd.Workflows;

public sealed class ConfigurationPersistenceWorkflowTests
{
    [Fact]
    public async Task ConfigurationPersistenceWorkflow_SettingsSurviveReload()
    {
        var path = Path.Combine(Path.GetTempPath(), $"modal-config-e2e-{Guid.NewGuid():N}.json");
        var serviceA = new ConfigurationService(path);

        await serviceA.SaveAsync(
            new ApplicationConfiguration
            {
                NotificationLeadTimeMinutes = 9,
                AutoDismissTimeoutSeconds = 75,
                ActiveProvider = "Outlook365",
            },
            TestContext.Current.CancellationToken
        );

        var serviceB = new ConfigurationService(path);
        var loaded = await serviceB.LoadAsync(TestContext.Current.CancellationToken);

        loaded.NotificationLeadTimeMinutes.ShouldBe(9);
        loaded.AutoDismissTimeoutSeconds.ShouldBe(75);
        loaded.ActiveProvider.ShouldBe("Outlook365");

        File.Delete(path);
    }
}
