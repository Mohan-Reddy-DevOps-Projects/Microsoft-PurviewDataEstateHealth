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
            default: throw new NotImplementedException();
        }
    }
}
