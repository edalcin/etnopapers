using Serilog;

namespace EtnoPapers.Core.Utils;

/// <summary>
/// Implements exponential backoff retry logic for cloud API calls.
/// Used when handling transient failures (429, 5xx errors).
/// </summary>
public static class RetryHelper
{
    private static readonly ILogger Logger = Log.ForContext(typeof(RetryHelper));

    /// <summary>
    /// Default maximum number of retry attempts.
    /// </summary>
    public const int DefaultMaxAttempts = 3;

    /// <summary>
    /// Default initial delay in milliseconds before first retry.
    /// </summary>
    public const int DefaultInitialDelayMs = 2000;

    /// <summary>
    /// Executes an operation with exponential backoff retry logic.
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="maxAttempts">Maximum number of retry attempts (default: 3)</param>
    /// <param name="initialDelayMs">Initial delay in milliseconds (default: 2000)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    /// <throws>The last exception if all retries are exhausted</throws>
    /// <throws>OperationCanceledException if cancelled</throws>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        int maxAttempts = DefaultMaxAttempts,
        int initialDelayMs = DefaultInitialDelayMs,
        CancellationToken cancellationToken = default)
    {
        if (maxAttempts < 1)
            throw new ArgumentException("Max attempts must be at least 1", nameof(maxAttempts));

        if (initialDelayMs < 0)
            throw new ArgumentException("Initial delay must be non-negative", nameof(initialDelayMs));

        Exception? lastException = null;
        var currentDelayMs = initialDelayMs;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Logger.Debug("[{Operation}] Attempt {Attempt}/{MaxAttempts}", operationName, attempt, maxAttempts);
                return await operation(cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;

                // Don't retry on cancellation
                if (ex is OperationCanceledException)
                    throw;

                // Don't retry on last attempt
                if (attempt >= maxAttempts)
                {
                    Logger.Error(ex, "[{Operation}] Failed after {MaxAttempts} attempts", operationName, maxAttempts);
                    break;
                }

                // Log retry attempt
                Logger.Warning(
                    ex,
                    "[{Operation}] Attempt {Attempt} failed. Retrying in {DelayMs}ms",
                    operationName,
                    attempt,
                    currentDelayMs);

                // Wait before retrying
                await Task.Delay(currentDelayMs, cancellationToken);

                // Exponential backoff: double the delay for next attempt
                currentDelayMs *= 2;
            }
        }

        throw lastException ?? new InvalidOperationException("Operation failed with no exception recorded");
    }

    /// <summary>
    /// Checks if an exception represents a transient error that should be retried.
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <param name="statusCode">Optional HTTP status code</param>
    /// <returns>True if the exception is transient and should be retried</returns>
    public static bool IsTransientError(Exception exception, int? statusCode = null)
    {
        // Timeout and cancellation errors
        if (exception is TimeoutException or OperationCanceledException)
            return false; // Don't retry timeout/cancellation

        // HTTP-specific transient errors
        if (statusCode.HasValue)
        {
            return statusCode.Value switch
            {
                429 => true,  // Rate limited - retry with backoff
                500 => true,  // Internal server error - may be transient
                502 => true,  // Bad gateway - may be transient
                503 => true,  // Service unavailable - may be transient
                504 => true,  // Gateway timeout - may be transient
                _ => false
            };
        }

        // Network errors can be transient
        if (exception is HttpRequestException)
            return true;

        return false;
    }
}
