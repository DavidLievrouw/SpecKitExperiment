using ModalCalendarNotification.Core.Features.Authentication;

namespace ModalCalendarNotification.Tests.Unit.Features.CalendarIntegration;

internal sealed class CapturingAuthenticationService : IAuthenticationService
{
    public int CallCount { get; private set; }

    public Task<string> AcquireAccessTokenAsync(string provider, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult("access-token");
    }
}