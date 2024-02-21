namespace Microsoft.Purview.DataEstateHealth.DHModels.Clients;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Rest;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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

    public Task CreateObserver(ObserverWrapper observer)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetErrorOutputContent(string dataProductId, string dataAssetId)
    {
        throw new NotImplementedException();
    }

    public Task<string> TriggerJobRun(string dataProductId, string dataAssetId)
    {
        return Task.FromResult(string.Empty);
    }
}
