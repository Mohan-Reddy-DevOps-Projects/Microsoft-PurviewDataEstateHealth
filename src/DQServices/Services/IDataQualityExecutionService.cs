namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDataQualityExecutionService
{
    public Task<string> SubmitDQJob(string tenantId, string accountId, DHControlNodeWrapper control, DHAssessmentWrapper assessment, string healthJobId);

    public Task<IEnumerable<DHRawScore>> ParseDQResult(DHComputingJobWrapper job);
}
