namespace Microsoft.Purview.DataEstateHealth.DHModels.Clients;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using System.Threading.Tasks;

public class DataQualityHttpClient : IDataQualityHttpClient
{
    private readonly DataQualityConfiguration dataQualityConfiguration;

    public DataQualityHttpClient(DataQualityConfiguration dataQualityConfiguration)
    {
        this.dataQualityConfiguration = dataQualityConfiguration;
    }

    public Task CreateObserver(ObserverWrapper observer)
    {
        return Task.CompletedTask;
    }

    public Task<string> TriggerJobRun(string dataProductId, string dataAssetId)
    {
        return Task.FromResult(string.Empty);
    }
}
