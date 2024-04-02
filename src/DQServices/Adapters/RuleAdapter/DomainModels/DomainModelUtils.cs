namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using System.Collections.Generic;

internal static class DomainModelUtils
{
    private static Dictionary<DomainModelType, DomainModel> domainModelDict = new()
    {
        { DomainModelType.AccessPolicySet, new AccessPolicySetDomainModel() },
        { DomainModelType.BusinessDomain, new BusinessDomainDomainModel() },
        { DomainModelType.DataAssetColumnClassificationAssignment, new DataAssetColumnClassificationAssignmentDomainModel() },
        { DomainModelType.DataAssetOwnerAssignment, new DataAssetOwnerAssignmentDomainModel() },
        { DomainModelType.DataProduct, new DataProductDomainModel() },
        { DomainModelType.DataProductAssetAssignment, new DataProductAssetAssignmentDomainModel() },
        { DomainModelType.DataProductBusinessDomainAssignment, new DataProductBusinessDomainAssignmentDomainModel() },
        { DomainModelType.DataProductOwner, new DataProductOwnerDomainModel() },
        { DomainModelType.DataProductStatus, new DataProductStatusDomainModel() },
        { DomainModelType.DataProductTermsOfUse, new DataProductTermsOfUseDomainModel() },
        { DomainModelType.DataQualityAssetRuleExecution, new DataQualityAssetRuleExecutionDomainModel() },
        { DomainModelType.GlossaryTerm, new GlossaryTermDomainModel() },
        { DomainModelType.GlossaryTermDataProductAssignment, new GlossaryTermDataProductAssignmentDomainModel() },
    };

    public static DomainModel GetDomainModel(DomainModelType domainModelType)
    {
        return domainModelDict.GetValueOrDefault(domainModelType, null);
    }
}
