// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

internal class ApiClient : IApiClient
{
    private readonly HttpClient client;

    private readonly IDataEstateHealthLogger dataEstateHealthLogger;

    public ApiClient(HttpClient httpClient, IDataEstateHealthLogger dataEstateHealthLogger)
    {
        this.dataEstateHealthLogger = dataEstateHealthLogger;
        this.client = httpClient;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> GetAsync(
        HttpRequestMessage httpRequestMessage,
        EndPointType endPointType,
        CancellationToken cancellationToken = default)
    {
        return await this.executeOperationAsync(
            httpRequestMessage,
            endPointType,
            "GetAsync",
            () => this.client.SendAsync(httpRequestMessage, cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> PutAsync(
        HttpRequestMessage httpRequestMessage,
        EndPointType endPointType,
        CancellationToken cancellationToken = default)
    {
        return await this.executeOperationAsync(
            httpRequestMessage,
            endPointType,
            "PutAsync",
            () => this.client.SendAsync(httpRequestMessage, cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> PostAsync(
        HttpRequestMessage httpRequestMessage,
        EndPointType endPointType,
        CancellationToken cancellationToken = default)
    {
        return await this.executeOperationAsync(
            httpRequestMessage,
            endPointType,
            "PostAsync",
            () => this.client.SendAsync(httpRequestMessage, cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> DeleteAsync(
        HttpRequestMessage httpRequestMessage,
        EndPointType endPointType,
        CancellationToken cancellationToken = default)
    {
        return await this.executeOperationAsync(
            httpRequestMessage,
            endPointType,
            "DeleteAsync",
            () => this.client.SendAsync(httpRequestMessage, cancellationToken));
    }

    private async Task<HttpResponseMessage> executeOperationAsync(
        HttpRequestMessage httpRequestMessage,
        EndPointType endPointType,
        string operationName,
        Func<Task<HttpResponseMessage>> operation)
    {
        this.dataEstateHealthLogger.LogInformation(
            $"httpRequestMessage for endPointType - {endPointType}, {operationName} in ApiClient requestUri :{httpRequestMessage.RequestUri.ToJson()}, method : {httpRequestMessage.Method.ToJson()}",
            isSensitive: true);

        return await PollyRetryPolicies
            .GetHttpClientTransientRetryPolicy(
                LoggerRetryActionFactory.CreateHttpClientRetryAction(this.dataEstateHealthLogger, nameof(EndPointType)))
            .ExecuteAsync(
                async () =>
                {
                    try
                    {
                        HttpResponseMessage response = await operation.Invoke();

                        return response;
                    }
                    catch (Exception exception)
                    {
                        this.dataEstateHealthLogger.LogError(
                            $"Api client {operationName} operation failed for endPointType - {endPointType}",
                            exception);

                        throw;
                    }
                });
    }
}
