namespace ModalCalendarNotification.Core.Shared.Utilities;

public static class PollyPolicies
{
    public static async Task ExecuteWithRetryAsync(
        Func<CancellationToken, Task> operation,
        int maxRetryAttempts = 3,
        TimeSpan? retryDelay = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var delay = retryDelay ?? TimeSpan.FromMilliseconds(100);
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxRetryAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await operation(cancellationToken);
                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt == maxRetryAttempts)
                {
                    throw;
                }

                await Task.Delay(delay, cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("Retry policy failed unexpectedly.");
    }

    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        int maxRetryAttempts = 3,
        TimeSpan? retryDelay = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var delay = retryDelay ?? TimeSpan.FromMilliseconds(100);
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxRetryAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt == maxRetryAttempts)
                {
                    throw;
                }

                await Task.Delay(delay, cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("Retry policy failed unexpectedly.");
    }

    public static async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(timeout);

        return await operation(linkedCts.Token);
    }
}
