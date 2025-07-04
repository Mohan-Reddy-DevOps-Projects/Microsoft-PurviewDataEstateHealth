﻿namespace Microsoft.Purview.DataEstateHealth.DHModels.Clients;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

public class DataQualityHttpClient : ServiceClient<DataQualityHttpClient>
{
    private readonly Uri BaseUri;

    private readonly IDataEstateHealthRequestLogger Logger;

    private readonly HttpClient Client;

    public DataQualityHttpClient(HttpClient httpClient, Uri baseUri, IDataEstateHealthRequestLogger logger)
    {
        this.Client = httpClient;
        this.BaseUri = baseUri;
        this.Logger = logger;
    }

    public async Task CreateObserver(ObserverWrapper observer, string tenantId, string accountId)
    {
        var requestUri = this.CreateRequestUri("/mdq/observers");

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Add("x-ms-client-tenant-id", tenantId);
        request.Headers.Add("x-ms-account-id", accountId);

        request.Content = this.CreateRequestContent(observer.JObject);

        var response = await this.Client.SendAsync(request).ConfigureAwait(false);
        this.HandleResponseStatusCode(response);
        var responseBody = await this.ParseResponse<JObject>(response).ConfigureAwait(false);
    }

    public async Task<string> TriggerJobRun(
        string tenantId,
        string accountId,
        string dataProductId,
        string dataAssetId,
        JobSubmitPayload payload)
    {
        var requestUri = this.CreateRequestUri($"/business-domains/{DataEstateHealthConstants.DEH_DOMAIN_ID}/data-products/{dataProductId}/data-assets/{dataAssetId}/mdq-observations");

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Add("x-ms-client-tenant-id", tenantId);
        request.Headers.Add("x-ms-account-id", accountId);

        request.Content = this.CreateRequestContent(payload);

        var response = await this.Client.SendAsync(request).ConfigureAwait(false);
        this.HandleResponseStatusCode(response);
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false); ;

        // return job id
        return responseBody.Substring(1, responseBody.Length - 2);
    }

    public async Task DeleteObserver(string tenantId, string accountId, string dataProductId, string dataAssetId)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["dataProductId"] = dataProductId;
        queryParams["dataAssetId"] = dataAssetId;

        var requestUri = this.CreateRequestUri("/mdq/observers", queryParams.ToString());

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Headers.Add("x-ms-client-tenant-id", tenantId);
        request.Headers.Add("x-ms-account-id", accountId);

        var response = await this.Client.SendAsync(request).ConfigureAwait(false);
        this.HandleResponseStatusCode(response);
    }

    private HttpContent CreateRequestContent(object obj)
    {
        var content = JsonConvert.SerializeObject(obj);
        return new StringContent(content, Encoding.UTF8, "application/json");
    }

    private Uri CreateRequestUri(string pathname, string query = null)
    {
        var builder = new UriBuilder(this.BaseUri)
        {
            Path = pathname,
            Query = query
        };
        return builder.Uri;
    }

    private void HandleResponseStatusCode(HttpResponseMessage response)
    {
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            this.Logger.LogError("Data quality service request failed", ex);
            throw;
        }
    }

    private async Task<T> ParseResponse<T>(HttpResponseMessage response)
    {
        try
        {
            var output = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonConvert.DeserializeObject<T>(output);
            return result!;
        }
        catch (JsonSerializationException ex)
        {
            this.Logger.LogError("Fail to deserialize response content", ex);
            throw;
        }
    }
}
