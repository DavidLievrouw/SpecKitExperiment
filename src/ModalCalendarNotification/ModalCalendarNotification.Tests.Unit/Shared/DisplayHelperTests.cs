using ModalCalendarNotification.Core.Shared.Utilities;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Shared;

public sealed class DisplayHelperTests
{
    [Fact]
    public void GetPrimaryDisplay_ReturnsPrimaryDisplayMetadata()
    {
        DisplayInfo display = DisplayHelper.GetPrimaryDisplay();

        display.IsPrimary.ShouldBeTrue();
        display.Width.ShouldBeGreaterThan(0);
        display.Height.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GetPrimaryDisplayCenter_ReturnsPointInsideDisplay()
    {
        DisplayInfo display = DisplayHelper.GetPrimaryDisplay();
        DisplayPoint center = DisplayHelper.GetPrimaryDisplayCenter();

        center.X.ShouldBeInRange(0, display.Width);
        center.Y.ShouldBeInRange(0, display.Height);
    }
}
