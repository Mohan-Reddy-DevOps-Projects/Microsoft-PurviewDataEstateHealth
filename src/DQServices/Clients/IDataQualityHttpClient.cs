namespace Microsoft.Purview.DataEstateHealth.DHModels.Clients;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using System.Threading.Tasks;

public interface IDataQualityHttpClient
{
    public Task CreateObserver(ObserverWrapper observer);

    public Task<string> TriggerJobRun(string dataProductId, string dataAssetId);

    public Task<string> GetErrorOutputContent(string dataProductId, string dataAssetId);
}
