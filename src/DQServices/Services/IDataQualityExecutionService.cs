namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDataQualityExecutionService
{
    public Task<string> SubmitDQJob(string accountId, string controlId, string healthJobId);

    public Task<IEnumerable<DHRawScore>> ParseDQResult(string accountId, string controlId, string healthJobId, string dqJobId);
}
