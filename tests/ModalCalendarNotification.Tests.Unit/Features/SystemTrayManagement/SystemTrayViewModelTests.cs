using ModalCalendarNotification.UI.Features.SystemTrayManagement;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.SystemTrayManagement;

public sealed class SystemTrayViewModelTests
{
    [Fact]
    public void OpenSettingsCommand_SetsSettingsRequestedFlag()
    {
        var sut = new SystemTrayViewModel();

        sut.OpenSettingsCommand.Execute(null);

        sut.IsSettingsRequested.ShouldBeTrue();
    }

    [Fact]
    public void ExitApplicationCommand_SetsExitRequestedFlag()
    {
        var sut = new SystemTrayViewModel();

        sut.ExitApplicationCommand.Execute(null);

        sut.IsExitRequested.ShouldBeTrue();
    }
}
