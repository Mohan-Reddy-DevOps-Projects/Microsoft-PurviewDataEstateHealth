namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;
using System.Threading.Tasks;

public interface IDataQualityExecutionService
{
    public Task<string> SubmitDQJob(string accountId);
}
