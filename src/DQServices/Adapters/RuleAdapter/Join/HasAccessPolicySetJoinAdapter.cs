namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class HasAccessPolicySetJoinAdapter : JoinAdapter
{

    private readonly string[][] accessPolicySetDef =
    [
        ["AccessPolicySetId", "String"],
        ["PolicyAppliedOn", "String"],
        ["PolicyAppliedOnId", "String"],
        ["ActiveFlag", "Number", "true"]
    ];

    private readonly string[][] outputSchemaDef =
    [
        ["DataProductHasAccessPolicySet", "boolean"]
    ];

    private List<DatasetSchemaItemWrapper> accessPolicySetSchema;
    private List<SparkSchemaItemWrapper> outputSchema;

    public HasAccessPolicySetJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.accessPolicySetSchema = SchemaUtils.GenerateSchemaFromDefinition(this.accessPolicySetDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset1 = new InputDatasetWrapper(new JObject()
        {
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_ACCESS_POLICY_SET_PATH, this.accessPolicySetSchema).JObject }
        });
        inputDataset1.Alias = "AccessPolicySet";
        inputDataset1.Primary = false;

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
