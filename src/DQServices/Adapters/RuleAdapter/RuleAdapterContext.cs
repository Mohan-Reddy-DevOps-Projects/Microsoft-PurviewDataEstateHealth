namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using System.Collections.Generic;

public class RuleAdapterContext
{
    public string endpoint;
    public string containerName;
    public string dataProductId;
    public string dataAssetId;
    public DHAssessmentWrapper assessment;

    public RuleAdapterContext(
        string endpoint,
        string containerName,
        string dataProductId,
        string dataAssetId,
        DHAssessmentWrapper assessment)
    {
        this.endpoint = endpoint;
        this.containerName = containerName;
        this.dataProductId = dataProductId;
        this.dataAssetId = dataAssetId;
        this.assessment = assessment;
        // Domain id, status string are always needed.
        this.joinRequirements = [JoinRequirement.BusinessDomain, JoinRequirement.DataProductStatus];
    }


    public SortedSet<JoinRequirement> joinRequirements { get; }
}
