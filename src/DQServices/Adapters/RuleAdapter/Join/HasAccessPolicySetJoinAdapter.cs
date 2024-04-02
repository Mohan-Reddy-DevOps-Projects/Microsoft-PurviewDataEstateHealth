namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class HasAccessPolicySetJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductHasAccessPolicySet", "boolean"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public HasAccessPolicySetJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset1 = this.GetInputDataset(DomainModelType.AccessPolicySet);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as APSPDataProductId,
                    CASE
                        WHEN COUNT(AccessPolicySet.AccessPolicySetId) > 0 THEN 'true'
                        ELSE 'false'
                    END as DataProductHasAccessPolicySet
                FROM DataProduct 
                LEFT JOIN AccessPolicySet
                    ON AccessPolicySet.PolicyAppliedOn = 'DataProduct'
                    AND DataProduct.DataProductID = AccessPolicySet.PolicyAppliedOnId
                    AND AccessPolicySet.ActiveFlag = 1
                GROUP BY DataProduct.DataProductID
            ) TAccessPolicySetPurpose ON DataProduct.DataProductID = TAccessPolicySetPurpose.APSPDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
