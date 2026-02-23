using ModalCalendarNotification.Core.Shared.Utilities;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Shared;

public sealed class PollyPoliciesTests
{
    [Fact]
    public async Task ExecuteWithRetryAsync_RetriesAndEventuallySucceeds()
    {
        int attempts = 0;

        await PollyPolicies.ExecuteWithRetryAsync(
            async _ =>
            {
                await Task.Yield();
                attempts++;
                if (attempts < 3)
                {
                    throw new InvalidOperationException("Transient error");
                }
            },
            3,
            TimeSpan.FromMilliseconds(1)
        );

        attempts.ShouldBe(3);
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_ThrowsWhenOperationExceedsTimeout()
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(10);

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await PollyPolicies.ExecuteWithTimeoutAsync(
                async cancellationToken =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                    return 42;
                },
                timeout
            )
        );
    }
}
