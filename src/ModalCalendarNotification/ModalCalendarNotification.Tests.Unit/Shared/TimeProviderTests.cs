using System.Diagnostics;
using Shouldly;
using Xunit;
using AppTimeProvider = ModalCalendarNotification.Core.Shared.Utilities.TimeProvider;

namespace ModalCalendarNotification.Tests.Unit.Shared;

public sealed class TimeProviderTests
{
    [Fact]
    public void UtcNow_ReturnsRecentTimestamp()
    {
        var sut = new AppTimeProvider();

        var before = DateTimeOffset.UtcNow;
        var value = sut.UtcNow;
        var after = DateTimeOffset.UtcNow;

        value.ShouldBeInRange(before, after);
    }

    [Fact]
    public async Task DelayAsync_WaitsAtLeastRequestedTime()
    {
        var sut = new AppTimeProvider();
        var delay = TimeSpan.FromMilliseconds(25);

        var stopwatch = Stopwatch.StartNew();
        await sut.DelayAsync(delay, TestContext.Current.CancellationToken);
        stopwatch.Stop();

        (stopwatch.Elapsed >= delay - TimeSpan.FromMilliseconds(10)).ShouldBeTrue();
    }
}
