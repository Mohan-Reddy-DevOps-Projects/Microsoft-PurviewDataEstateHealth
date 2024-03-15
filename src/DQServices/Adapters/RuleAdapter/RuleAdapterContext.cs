namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using System.Collections.Generic;

public class RuleAdapterContext
{
    public string endpoint;
    public string containerName;
    public string dataProductId;
    public string dataAssetId;
    public IEnumerable<string> fitlerDomainIds;
    public DHAssessmentWrapper assessment;

    public RuleAdapterContext(
        string endpoint,
        string containerName,
        string dataProductId,
        string dataAssetId,
        DHAssessmentWrapper assessment,
        IEnumerable<string> fitlerDomainIds)
    {
        this.endpoint = endpoint;
        this.containerName = containerName;
        this.dataProductId = dataProductId;
        this.dataAssetId = dataAssetId;
        this.assessment = assessment;
        this.joinRequirements = new SortedSet<JoinRequirement>(DataEstateHealthConstants.ALWAYS_REQUIRED_JOIN_REQUIREMENTS);
        this.fitlerDomainIds = fitlerDomainIds;
    }


    public SortedSet<JoinRequirement> joinRequirements { get; }
}
