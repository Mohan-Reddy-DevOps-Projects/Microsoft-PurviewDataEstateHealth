// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.WindowsAzure.ResourceStack.Common.Instrumentation;
using Newtonsoft.Json;
using ErrorModel = Common.ErrorModel;

/// <summary>
/// The Partner Service implementation.
/// </summary>
internal abstract class PartnerServiceBase
{
    // default values and boundaries for polling
    private const int DefaultPollingTimeoutSeconds = 1800;   // 30 min
    private const int DefaultPollingIntervalSeconds = 30;   // 30 sec
    private const int MinPollingIntervalSeconds = 1;
    private const int MaxPollingIntervalSeconds = DefaultPollingTimeoutSeconds;

    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// Name of the partner http client
    /// </summary>
    public const string PartnerClient = "PartnerClient";

    /// <summary>
    /// The request context access to get it from DI.
    /// </summary>
    protected IRequestHeaderContext RequestHeaderContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartnerServiceBase" /> class.
    /// </summary>
    /// <param name="requestHeaderContext">The request header context.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClientFactory">The http client factory.</param>
    public PartnerServiceBase(
        IRequestHeaderContext requestHeaderContext,
        IDataEstateHealthRequestLogger logger,
        IHttpClientFactory httpClientFactory)
    {
        this.RequestHeaderContext = requestHeaderContext;
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Sends the HTTP request message.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="partnerName">Name of the partner.</param>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="validateResponse">if set to <c>true</c> [validate response].</param>
    /// <param name="operationTimeoutSeconds">The operation timeout seconds.</param>
    /// <param name="validHttpStatusResponses">The valid HTTP status responses.</param>
    /// <param name="pollingTimeoutSeconds">Timeout seconds config for polling async operation status.</param>
    /// <param name="pollingIntervalSeconds">Interval seconds config for polling async operation status.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The task.</returns>
    /// <exception cref="ServiceException">Server error when trying to delete partner resource</exception>
    protected internal async Task SendHttpRequestMessage(
        HttpRequestMessage request,
        string partnerName,
        string endpoint,
        bool validateResponse,
        int operationTimeoutSeconds,
        HttpStatusCode[] validHttpStatusResponses = null,
        int pollingTimeoutSeconds = DefaultPollingTimeoutSeconds,
        int pollingIntervalSeconds = DefaultPollingIntervalSeconds,
        [CallerMemberName] string operation = "")
    {
        var sw = Stopwatch.StartNew();
        this.logger.LogInformation($"Invoking operation:{operation} on partner:{partnerName}, endpoint:{endpoint}, timeout:{operationTimeoutSeconds}");

        // If no valid http response, default to accepting 200.
        validHttpStatusResponses ??= new[] { HttpStatusCode.OK };

        try
        {
            // Create timeout for HTTP call
            var timeout = TimeSpan.FromSeconds(operationTimeoutSeconds);
            using (var cts = new CancellationTokenSource(timeout))
            {
                // Invoke HTTP call
                using (HttpClient httpClient = this.httpClientFactory.CreateClient(PartnerClient))
                {
                    httpClient.Timeout = timeout;
                    using (HttpResponseMessage response = await httpClient.SendAsync(request, cts.Token))
                    {
                        // TODO: consider adding 202 Accepted to parameter validHttpStatusResponses, and handling it separately
                        if (response.StatusCode == HttpStatusCode.Accepted)
                        {
                            this.logger.LogInformation($"Handle async response for operation:{operation} on partner:{partnerName}, endpoint:{endpoint}.");
                            // handle async operation done by Partner
                            await this.HandleAsynchronousResponse(
                                partnerName,
                                operation,
                                httpClient,
                                response,
                                pollingTimeoutSeconds,
                                pollingIntervalSeconds).ConfigureAwait(false);
                        }
                        else if (!validHttpStatusResponses.Any(code => code == response.StatusCode))
                        {
                            await this.LogHttpErrorResponse(response, operation, partnerName, endpoint);

                            throw new ServiceError(
                                ErrorCategory.DownStreamError,
                                ErrorCode.PartnerException.Code,
                                $"Service {partnerName} returning invalid response on {operation}. Response code: {response.StatusCode}.").ToException();
                        }

                        this.logger.LogInformation($"Successfully completed operation:{operation} on partner:{partnerName}, endpoint:{endpoint}. Duration {sw.ElapsedMilliseconds} ms");
                    }
                }
            }
        }
        catch (Exception e)
        {
            string errorMessage = $"Error on operation:{operation}, partner:{partnerName}, endpoint:{endpoint}. Duration {sw.ElapsedMilliseconds} ms";
            if (validateResponse)
            {
                this.logger.LogInformation(errorMessage, e);

                // If already a babylon exception throw existing.
                // Else create new on service unavailable.
                if (e is ServiceException)
                {
                    throw;
                }

                throw new ServiceError(
                    ErrorCategory.DownStreamError,
                    ErrorCode.PartnerException.Code,
                    $"Service {partnerName} unavailable on {operation}.").ToException();
            }

            this.logger.LogWarning($"Partner failing silently due to validate response configuration: {partnerName}");
            this.logger.LogWarning(errorMessage, e);

        }
    }

    /// <summary>
    /// Asynchronous operation handler.
    /// </summary>
    /// <param name="partnerName">Name of the partner.</param>
    /// <param name="operation">The asynchronous operation done by Partner.</param>
    /// <param name="httpClient">HTTP Client of the Partner.</param>
    /// <param name="response">The received Http response message.</param>
    /// <param name="timeoutSeconds">The configured timeout for polling in seconds.</param>
    /// <param name="intervalSeconds">The configured polling interval in seconds.</param>
    /// <returns>The task.</returns>
    protected async Task HandleAsynchronousResponse(
        string partnerName,
        string operation,
        HttpClient httpClient,
        HttpResponseMessage response,
        int timeoutSeconds,
        int intervalSeconds)
    {
        // validate status endpoint returned as Location header response
        string requestUri = response?.Headers?.Location?.ToString();
        this.logger.LogInformation($"Async response status request uri for partner: {partnerName} is at endpoint: {requestUri}, " +
            $"asynchronous operation: {operation}.");
        if (string.IsNullOrWhiteSpace(requestUri))
        {
            // Location response header not found - throw due to missing required header
            throw new ServiceError(
                ErrorCategory.DownStreamError,
                ErrorCode.PartnerException.Code,
                $"Required Location response header not returned by Partner: {partnerName}, for asynchronous operation: {operation}.").ToException();
        }

        // validate and sanitize polling interval and timeout - note, sanitization is done in three steps
        // first, sanitize the configured polling interval if its value is outside the defined boundaries
        if (intervalSeconds < MinPollingIntervalSeconds || intervalSeconds > MaxPollingIntervalSeconds)
        {
            // overwrite invalid config value to default interval, which is correctly bounded
            intervalSeconds = DefaultPollingIntervalSeconds;
        }

        // second, further sanitize interval by overwriting it as specified by the partner in the response
        // note, previous step sanitized config value - current step sanitizes the response value, if any
        if (response.Headers.RetryAfter?.Delta != null)
        {
            int retryAfterSeconds = (int)response.Headers.RetryAfter.Delta?.TotalSeconds;
            if (retryAfterSeconds >= MinPollingIntervalSeconds && retryAfterSeconds <= MaxPollingIntervalSeconds)
            {
                // valid interval in partner response - overwrite current value to the specified interval
                intervalSeconds = retryAfterSeconds;
            }
        }

        // third, sanitize the configured polling timeout if it is less than the polling interval
        // note, timeout needs to be an upper bound of interval, so this has to be done after finalizing interval
        timeoutSeconds = (timeoutSeconds < intervalSeconds) ? intervalSeconds : timeoutSeconds;

        // all polling values sanitized - log details before proceeding
        this.logger.LogInformation($"Calling partner: {partnerName}, endpoint: {requestUri}, to poll the status of " +
            $"asynchronous operation: {operation}. Status will be polled in intervals of {intervalSeconds} sec, " +
            $"for a maximum of {timeoutSeconds} sec before timing out.");

        // account for Task.Delay inaccuracies - shave 200ms from timeout
        TimeSpan timeout = TimeSpan.FromMilliseconds((1000 * timeoutSeconds) - 200);
        TimeSpan interval = TimeSpan.FromSeconds(intervalSeconds);

        // track polling attempts and overall duration of polling
        int pollCount = 0;
        Stopwatch timer = Stopwatch.StartNew();
        try
        {
            // timeout governed by cancellation token in ms
            using (CancellationTokenSource cts = new(timeout))
            {
                while (!cts.IsCancellationRequested)
                {
                    pollCount++;

                    // call partner to get operation status, retry in case of API failures or seralization issue
                    AsyncOperationStatusResponse partnerStatusResponse = await Retry.DoAsync(
                        () => this.GetPartnerAsyncOperationStatus(
                            partnerName,
                            operation,
                            httpClient,
                            requestUri,
                            cts.Token),
                        shouldRetry: ex => ex is not OperationCanceledException)
                    .ConfigureAwait(false);

                    // check if request status is in completed state, i.e., Succeeded or Failed
                    if (partnerStatusResponse.Status == AsyncOperationStatus.Succeeded
                        || partnerStatusResponse.Status == AsyncOperationStatus.Failed)
                    {
                        // Partner async operation completed
                        timer.Stop();
                        this.logger.LogInformation("Asynchronous Partner operation status polling completed in " +
                            $"{timer.Elapsed.TotalSeconds} seconds with status {partnerStatusResponse.Status}. " +
                            $"Status was polled {pollCount} times before completion");

                        // throw if Partner operation failed
                        if (partnerStatusResponse.Status == AsyncOperationStatus.Failed)
                        {
                            throw new ServiceError(
                                ErrorCategory.DownStreamError,
                                ErrorCode.AsyncOperation_ExecutionFailed.Code,
                                $"Asynchronous operation: {operation} failed for partner: {partnerName}.").ToException();
                        }

                        // async operation successful - exit method
                        return;
                    }

                    // failed to get request status or request still in progress - wait before retrying
                    await Task.Delay(interval, cts.Token).ConfigureAwait(false);
                }

                // throw OperationCanceledException
                cts.Token.ThrowIfCancellationRequested();
            }
        }
        catch (ServiceException ex) when (ex.ServiceError.Code == ErrorCode.AsyncOperation_ExecutionFailed.Code)
        {
            // async operation failed - rethrow
            throw;
        }
        catch (Exception ex) when (ex is not ServiceException)
        {
            timer.Stop();

            // exception either due to timeout or retry limit for exceptions reached
            string reason = (ex is OperationCanceledException) ?
                "polling timed out before Partner operation completion" :
                "retry limit for exceptions reached";

            string errorMessage = $"Failed to poll asynchronous operation status from Partner after polling " +
                $"{pollCount} over {timer.Elapsed.TotalSeconds} seconds. Partner: {partnerName}, operation: " +
                $"{operation}, endpoint: {requestUri}, reason: {reason}, exception: {ex}.";

            throw new ServiceError(
                ErrorCategory.DownStreamError,
                ErrorCode.PartnerException.Code,
                errorMessage).ToException();
        }
    }

    private async Task<AsyncOperationStatusResponse> GetPartnerAsyncOperationStatus(
        string partnerName,
        string operation,
        HttpClient httpClient,
        string requestUri,
        CancellationToken token = default)
    {
        using (HttpRequestMessage statusRequest = this.CreateRequest(HttpMethod.Get, requestUri))
        {
            this.logger.LogInformation($"Polling status from Partner: {partnerName}, for asynchronous operation: " +
                $"{operation}. GET status request: {statusRequest}.");

            // send request to Partner to fetch status of async operation
            Stopwatch timer = Stopwatch.StartNew();
            using (HttpResponseMessage statusResponse = await httpClient.SendAsync(statusRequest, token)
                .ConfigureAwait(false))
            {
                timer.Stop();
                this.logger.LogInformation($"Polled status from Partner: {partnerName}, for asynchronous operation: " +
                    $"{operation}. Duration: {timer.Elapsed.TotalSeconds}, GET status response: {statusResponse}");

                // only consider statusResponse 200
                if (statusResponse.StatusCode == HttpStatusCode.OK)
                {
                    string responseStringContent = await statusResponse.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);

                    // deserialize response body as OperationResponse
                    AsyncOperationStatusResponse responsePayload = Rest.Serialization.SafeJsonConvert
                        .DeserializeObject<AsyncOperationStatusResponse>(responseStringContent);

                    return responsePayload;
                }

                // throw for unexepcted response code
                string errorMessage = "Unexpected Partner response for GetPartnerAsyncOperationStatus - expected" +
                    $" status code 200 OK, but Partner returned status code: {(int)statusResponse.StatusCode} " +
                    $"{statusResponse.StatusCode}. Response: {statusResponse}";

                throw new ServiceError(
                    ErrorCategory.DownStreamError,
                    ErrorCode.PartnerException.Code,
                    errorMessage).ToException();
            }
        }
    }

    /// <summary>
    /// Logs the HTTP Error Response.
    /// </summary>
    /// <param name="response">The received Http response message.</param>
    /// <param name="operation">The type of the operation.</param>
    /// <param name="partnerName">Name of the partner.</param>
    /// <param name="endpoint">The endpoint.</param>
    private async Task<string> LogHttpErrorResponse(HttpResponseMessage response, string operation, string partnerName, string endpoint)
    {
        // We are following same pattern for exception handling as generated by Swagger SDK.
        // Reference : ProjectBabylon\ResourceProvider\Tools\DataPlaneAccountClient\Generated\Accounts.cs
        string errorDescription = $"Error while performing operation:{operation} on partner:{partnerName}, endpoint:{endpoint}. StatusCode:{response.StatusCode}. Reason:{response.ReasonPhrase}.";
        string errorDetails = string.Empty;

        ErrorModel errorResponse = null;
        try
        {
            // Deserialize response
            string content = await response.Content.ReadAsStringAsync();
            errorResponse = Rest.Serialization.SafeJsonConvert.DeserializeObject<ErrorModel>(content);
        }
        catch (JsonException ex)
        {
            // Log the exception
            errorDetails += $"Unable to deserialize the response. {ex}";
        }

        errorDetails += errorResponse?.Details;

        this.logger.LogWarning(
                FormattableString.Invariant(
                    $"{errorDescription}. Details:{errorDetails}"));

        return errorDescription;
    }

    /// <summary>
    /// Add request correlation ID
    /// </summary>
    /// <param name="httpRequest">The http request.</param>
    protected void AddCorrelationId(HttpRequestMessage httpRequest)
    {
        // Add correlation ID
        httpRequest.Headers.TryAddWithoutValidation(
            RequestCorrelationContext.HeaderCorrelationRequestId,
            this.RequestHeaderContext?.CorrelationId ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Add request account context headers
    /// </summary>
    /// <param name="httpRequest">The http request.</param>
    /// <param name="accountName">The account name</param>
    /// <param name="accountId">The account id</param>
    /// <param name="tenantId">The account tenant id</param>
    /// <param name="catalogId">The account catalog id</param>
    protected void AddAccountContext(HttpRequestMessage httpRequest, string accountName, string accountId, string tenantId, string catalogId)
    {
        httpRequest.Headers.TryAddWithoutValidation(DataPlaneRequestHeaderConstants.HeaderAccountId, accountId);
        httpRequest.Headers.TryAddWithoutValidation(DataPlaneRequestHeaderConstants.HeaderAccountName, accountName);
        httpRequest.Headers.TryAddWithoutValidation(DataPlaneRequestHeaderConstants.HeaderCatalogId, catalogId);
        httpRequest.Headers.TryAddWithoutValidation(DataPlaneRequestHeaderConstants.HeaderClientTenantId, tenantId);
    }

    protected HttpRequestMessage CreateRequest(HttpMethod requestMethod, string requestUri, string requestContent = null)
    {
        // Create HTTP transport objects
        HttpRequestMessage httpRequest = new(requestMethod, requestUri);

        if (requestContent != null)
        {
            // add request body
            httpRequest.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
            httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue
                .Parse("application/json; charset=utf-8");
        }

        // add correlationId to request and return request
        this.AddCorrelationId(httpRequest);

        return httpRequest;
    }
}
