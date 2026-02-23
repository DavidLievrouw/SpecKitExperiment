namespace ModalCalendarNotification.Core.Features.Authentication;

public interface IAuthenticationService
{
    Task<string> AcquireAccessTokenAsync(
        string provider,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default
    );
}
