namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;

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
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductOwner);
                return "DataProductOwnerCount";
            case DHCheckPoint.DataProductRelatedDataAssetsCount:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataAssetCount);
                return "DataAssetCount";
            case DHCheckPoint.DataProductRelatedTermsCount:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductTermCount);
                return "DataProductTermCount";
            case DHCheckPoint.DataProductHasDataAccessPolicy:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.HasAccessPolicySet);
                return "DataProductHasAccessPolicySet";
            case DHCheckPoint.DataProductHasDataUsagePurpose:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductTermsOfUseCount);
                return "DataProductHasDataUsagePurpose";
            case DHCheckPoint.DataProductTermsOfUseCount:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductTermsOfUseCount);
                return "DataProductTermsOfUseCount";
            case DHCheckPoint.DataProductHasDQScore:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductDQScore);
                return "DataProductHasDQScore";
            case DHCheckPoint.DataProductAllRelatedAssetsHaveDQScore:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductDQScore);
                return "DataProductAllRelatedAssetsHaveDQScore";
            case DHCheckPoint.DataProductRelatedAssetsHaveDQScore:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductAssetDQScore);
                return "DataProductRelatedAssetsHaveDQScore";
            case DHCheckPoint.DataProductAllRelatedAssetsHaveOwner:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductAllAssetsOwner);
                return "DataProductAllRelatedAssetsHaveOwner";
            case DHCheckPoint.DataProductRelatedAssetsOwnerCount:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductAssetsOwner);
                return "DataProductRelatedAssetsOwnerCount";
            case DHCheckPoint.DataProductRelatedDataAssetsWithClassificationCount:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataAssetClassification);
                return "DataProductRelatedDataAssetsWithClassificationCount";
            case DHCheckPoint.DataProductDomainHasOwner:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.BusinessDomainData);
                return "DataProductDomainHasOwner";
            case DHCheckPoint.DataProductDomainDescriptionLength:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.BusinessDomainData);
                return "length(regexReplace(BusinessDomainDescription, '(<(.*?)>)', ''))";
            case DHCheckPoint.DataProductRelatedObjectivesCount:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductObjectivesCount);
                return "DataProductObjectivesCount";
            // TODO delete DataProductAllRelatedTermsMinimalDescriptionLength
            case DHCheckPoint.DataProductAllRelatedTermsMinimalDescriptionLength:
            case DHCheckPoint.DataProductRelatedTermsDescriptionLength:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.DataProductTerm);
                return "length(regexReplace(DataProductRelatedTermsDescription, '(<(.*?)>)', ''))";
            case DHCheckPoint.BusinessDomainCriticalDataElementCount:
                ruleAdapterContext.joinRequirements.Add(DataQualityJoinRequirement.BusinessDomainCriticalDataElement);
                return "BusinessDomainCriticalDataElementCount";
            default: throw new NotImplementedException("Checkpoint: " + checkpoint.ToString());
        }
    }
}
