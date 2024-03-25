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
            case DHCheckPoint.DataProductDescriptionLength: return "length(regexReplace(DataProductDescription, '(<(.*?)>)', ''))";
            case DHCheckPoint.DataProductBusinessUseLength: return "length(regexReplace(UseCases, '(<(.*?)>)', ''))";
            case DHCheckPoint.DataProductEndorsed: return "Endorsed";
            case DHCheckPoint.DataProductStatus: return "DataProductStatusDisplayName";
            case DHCheckPoint.DataProductOwnerCount:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataProductOwner);
                return "DataProductOwnerCount";
            case DHCheckPoint.DataProductRelatedDataAssetsCount:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataAssetCount);
                return "DataAssetCount";
            case DHCheckPoint.DataProductRelatedTermsCount:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataProductTerm);
                return "DataProductTermCount";
            case DHCheckPoint.DataProductHasDataAccessPolicy:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.HasAccessPolicySet);
                return "DataProductHasAccessPolicySet";
            case DHCheckPoint.DataProductHasDataUsagePurpose:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataProductTermsOfUseCount);
                return "DataProductHasDataUsagePurpose";
            case DHCheckPoint.DataProductTermsOfUseCount:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataProductTermsOfUseCount);
                return "DataProductTermsOfUseCount";
            case DHCheckPoint.DataProductHasDQScore:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataProductAssetDQScore);
                return "DataProductHasDQScore";
            case DHCheckPoint.DataProductAllRelatedAssetsHaveDQScore:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataProductAssetDQScore);
                return "DataProductAllRelatedAssetsHaveDQScore";
            case DHCheckPoint.DataProductAllRelatedAssetsHaveOwner:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataProductAssetsOwner);
                return "DataProductAllRelatedAssetsHaveOwner";
            case DHCheckPoint.DataProductRelatedDataAssetsWithClassificationCount:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataAssetClassification);
                return "DataProductRelatedDataAssetsWithClassificationCount";
            case DHCheckPoint.DataProductDomainHasOwner:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.BusinessDomainData);
                return "DataProductDomainHasOwner";
            case DHCheckPoint.DataProductDomainDescriptionLength:
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.BusinessDomainData);
                return "length(regexReplace(BusinessDomainDescription, '(<(.*?)>)', ''))";
            case DHCheckPoint.DataProductAllRelatedTermsMinimalDescriptionLength:
                // TODO handle rich text
                ruleAdapterContext.joinRequirements.Add(JoinRequirement.DataProductTerm);
                return "DataProductAllRelatedTermsMinimalDescriptionLength";
            default: throw new NotImplementedException("Checkpoint: " + checkpoint.ToString());
        }
    }
}
