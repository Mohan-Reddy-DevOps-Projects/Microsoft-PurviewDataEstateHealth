// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Retry utility
/// </summary>
public static class RetryUtil
{
    /// <summary>
    /// Delegate type for the error handler
    /// </summary>
    /// <param name="ex">Exception to handle</param>
    /// <param name="overrideException">Exception to throw if handler returns false</param>
    /// <returns>True if exception is handled and loop continues, false when exception should be rethrown</returns>
    public delegate bool ExceptionHandler(Exception ex, out Exception overrideException);

    /// <summary>
    /// Asynchronously retries the action when exception is:
    /// HttpRequestException, TaskCanceledException, TimeoutException, or matches exceptionPredicate for custom errors
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TException"></typeparam>
    /// <param name="action">Action</param>
    /// <param name="exceptionPredicate">function to retry based on given condition of an exception</param>
    /// <param name="retryIntervalInMs">Retry interval</param>
    /// <param name="maxRetries">Max number of retries</param>
    /// <param name="exceptionHandler"></param>
    /// <returns></returns>
    public static Task<TResult> ExecuteWithRetryAsync<TResult, TException>(
        Func<int, Task<TResult>> action,
        Func<TException, bool> exceptionPredicate = null,
        int retryIntervalInMs = 500,
        int maxRetries = 3,
        ExceptionHandler exceptionHandler = null)
        where TException : Exception, new()
    {
        var delay = Enumerable.Repeat(TimeSpan.FromMilliseconds(retryIntervalInMs), maxRetries);
        var policyBuilder = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>();

        if (exceptionPredicate != null)
        {
            policyBuilder.Or(exceptionPredicate);
        }

        return policyBuilder
            .WaitAndRetryAsync(
            delay,
            onRetry: (exception, timespan, attempt, context) =>
            {
                context["attempt"] = attempt;
                Exception handledException = null;
                if (exceptionHandler != null && !exceptionHandler(exception, out handledException))
                {
                    // if we force override then throw immediately
                    if (handledException != null)
                    {
                        throw handledException;
                    }
                }
                if (attempt == maxRetries)
                {
                    // if we exhausted retry and we handled the exception then throw that instead of propagating the original
                    if (handledException != null)
                    {
                        throw handledException;
                    }
                }
            })
            .ExecuteAsync(action: c =>
            {
                c.TryGetValue("attempt", out object value);
                int attempt = (value is int i ? i : 0);
                return action(attempt);
            },
            context: new Context("operationKey", new Dictionary<string, object> { { "attempt", 0 } }));
    }
}
