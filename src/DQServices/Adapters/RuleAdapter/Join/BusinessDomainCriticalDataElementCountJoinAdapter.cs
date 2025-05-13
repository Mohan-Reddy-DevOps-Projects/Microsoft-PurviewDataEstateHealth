namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class BusinessDomainCriticalDataElementCountJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["BusinessDomainCriticalDataElementCount", "long"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public BusinessDomainCriticalDataElementCountJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var criticalDataElementDataset = this.GetInputDataset(DomainModelType.CriticalDataElement);
        var dataProductCriticalDataElementAssignmentDataset = this.GetInputDataset(DomainModelType.DataProductCriticalDataElementAssignment);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DPBDA.BusinessDomainId as BDCDEBusinessDomainId,
                    COALESCE(COUNT(DISTINCT CDE.CriticalDataElementId), 0) as BusinessDomainCriticalDataElementCount
                FROM BusinessDomain BD
                LEFT JOIN DataProductBusinessDomainAssignment DPBDA ON BD.BusinessDomainId = DPBDA.BusinessDomainId
                LEFT JOIN DataProductCriticalDataElementAssignment DPCDE ON DPBDA.DataProductID = DPCDE.DataProductId
                LEFT JOIN CriticalDataElement CDE ON DPCDE.CriticalDataElementId = CDE.CriticalDataElementId
                GROUP BY DPBDA.BusinessDomainId
            ) BDCDEBusinessDomain ON TDataProductBusinessDomainAssignment.BusinessDomainId = BDCDEBusinessDomain.BDCDEBusinessDomainId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { criticalDataElementDataset, dataProductCriticalDataElementAssignmentDataset },
            SchemaFromJoin = this.outputSchema
        };
    }
} 