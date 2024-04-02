namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;

public abstract class DataQualityJoinAdapter
{
    protected RuleAdapterContext context;

    public DataQualityJoinAdapter(RuleAdapterContext context)
    {
        this.context = context;
    }

    public abstract JoinAdapterResult Adapt();

    protected InputDatasetWrapper GetInputDataset(DomainModelType domainModelType, bool isPrimary = false, string alias = null)
    {
        return ObserverUtils.GetInputDataset(this.context, domainModelType, isPrimary, alias);
    }
}
