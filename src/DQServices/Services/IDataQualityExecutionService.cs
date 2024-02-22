namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDataQualityExecutionService
{
    public Task<string> SubmitDQJob(string accountId, string controlId, string healthJobId);

    public Task<IEnumerable<ScorePayload>> ParseDQResult(string accountId, string dataProductId, string dataAssetId, string jobId);
}
