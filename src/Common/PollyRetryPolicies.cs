// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.DGP.ServiceBasics.Errors;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Timeout;

/// <summary>
/// Polly retry policies
/// </summary>
public static class PollyRetryPolicies
{
    private static readonly HttpStatusCode[] httpStatusCodesWorthRetrying = {
        HttpStatusCode.RequestTimeout, // 408
        HttpStatusCode.InternalServerError, // 500
        HttpStatusCode.BadGateway, // 502
        HttpStatusCode.ServiceUnavailable, // 503
        HttpStatusCode.GatewayTimeout // 504
    };

    /// <summary>
    /// Returns a transient errors retry policy for HTTP requests.
    /// </summary>
    /// <param name="onRetry"></param>
    /// <param name="retryCount"></param>
    /// <returns>The retry policy</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetHttpClientTransientRetryPolicy(
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context> onRetry,
        int retryCount = 5)
    {
        IEnumerable<TimeSpan> delay = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(1),
            retryCount: retryCount,
            fastFirst: true);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .OrInner<TaskCanceledException>()
            .OrInner<TimeoutException>()
            .OrInner<SocketException>()
            .OrInner<IOException>()
            .WaitAndRetryAsync(delay, onRetry);
    }

    /// <summary>
    /// Returns a transient error retry policy for generic actions.
    /// </summary>
    /// <returns>The retry policy</returns>
    public static IAsyncPolicy GetNonHttpClientTransientRetryPolicy(Action<Exception, TimeSpan, int, Context> onRetry, int retryCount = 5)
    {
        IEnumerable<TimeSpan> delay = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(1),
            retryCount: retryCount,
            fastFirst: true);

        return Policy.HandleInner<HttpRequestException>((e) =>
            !e.StatusCode.HasValue || httpStatusCodesWorthRetrying.Contains(e.StatusCode.Value))
            .OrInner<TaskCanceledException>()
            .OrInner<TimeoutException>()
            .OrInner<IOException>()
            .OrInner<SocketException>()
            .OrInner<HttpRequestException>()
            .Or<ServiceException>((e) =>
                e.ServiceError.Code == ErrorCode.ArtifactStoreServiceException.Code)
            .WaitAndRetryAsync(delay, onRetry);
    }
}

