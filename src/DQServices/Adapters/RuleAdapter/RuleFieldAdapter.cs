namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System;

internal static class RuleFieldAdapter
{
    public static string ToDqExpression(RuleAdapterContext ruleAdapterContext, DHCheckPoint checkpoint)
    {
        switch (checkpoint)
        {
            case DHCheckPoint.DataProductDescriptionLength: return "length(DataProductDescription)";
            case DHCheckPoint.DataProductBusinessUseLength: return "length(UseCases)";
            case DHCheckPoint.DataProductEndorsed: return "Endorsed";
            case DHCheckPoint.DataProductStatus: return "DataProductStatusDisplayName";
            case DHCheckPoint.DataProductOwnerCount:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataProductOwner);
                return "DataProductOwnerCount";
            case DHCheckPoint.DataProductRelatedDataAssetsCount:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataAssetCount);
                return "DataAssetCount";
            case DHCheckPoint.DataProductRelatedTermsCount:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataProductTermCount);
                return "DataProductTermCount";
            case DHCheckPoint.DataProductHasDataAccessPolicy:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.HasAccessPolicySetAndPurpose);
                return "DataProductHasAccessPolicySet";
            case DHCheckPoint.DataProductHasDataUsagePurpose:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.HasAccessPolicySetAndPurpose);
                return "DataProductHasDataUsagePurpose";
            // TODO jar for the below
            case DHCheckPoint.DataProductAllRelatedAssetsHaveOwner:
                return "DataProductHasAccessPolicySet";
            case DHCheckPoint.DataProductHasDQScore:
                return "DataProductHasAccessPolicySet";
            case DHCheckPoint.DataProductAllRelatedAssetsHaveDQScore:
                return "DataProductHasAccessPolicySet";
            default: throw new NotImplementedException();
        }
    }
}
