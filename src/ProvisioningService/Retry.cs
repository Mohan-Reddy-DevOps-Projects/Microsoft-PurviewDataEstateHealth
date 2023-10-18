// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

internal static class Retry
{
    /// <summary>
    /// Does the asynchronous.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="action">The action.</param>
    /// <param name="retryInterval">The retry interval in milliseconds.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="logException">The log function.</param>
    /// <param name="shouldRetry">The should retry.</param>
    /// <returns>Return value and list of exceptions.</returns>
    public static async Task<T> DoAsync<T>(
        Func<Task<T>> action,
        int retryInterval = 500,
        int retryCount = 2,
        Action<string, Exception> logException = null,
        Func<Exception, bool> shouldRetry = null)
    {
        int retry = 0;
        while (true)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ShouldThrow(ex, retry, retryCount, logException, shouldRetry))
                {
                    throw;
                }

                await Task.Delay(retryInterval * retry);
                retry++;
            }
        }
    }

    /// <summary>
    /// Does the asynchronous retries for actions - no return value.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="retryInterval">The retry interval in milliseconds.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="logException">The log function.</param>
    /// <param name="shouldRetry">The should retry.</param>
    /// <returns>Return value and list of exceptions.</returns>
    public static async Task DoAsync(
        Func<Task> action,
        int retryInterval = 500,
        int retryCount = 2,
        Action<string, Exception> logException = null,
        Func<Exception, bool> shouldRetry = null)
    {
        int retry = 0;
        while (true)
        {
            try
            {
                await action().ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                if (ShouldThrow(ex, retry, retryCount, logException, shouldRetry))
                {
                    throw;
                }

                await Task.Delay(retryInterval * retry);
                retry++;
            }
        }
    }

    private static bool ShouldThrow(
            Exception ex,
            int retry,
            int retryCount,
            Action<string, Exception> logException,
            Func<Exception, bool> shouldRetry)
    {
        logException?.Invoke($"Retry attempt {retry} failed. {(retry == retryCount ? "no more retries" : "retrying...")}", ex);

        if ((retry == retryCount) || (shouldRetry != null && !shouldRetry(ex)))
        {
            return true;
        }

        return false;
    }
}
