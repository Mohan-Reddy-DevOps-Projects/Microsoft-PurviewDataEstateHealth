namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataProductOwnerJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductOwnerCount", "long"],
        ["DataProductOwnerIds", "string"],
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductOwnerJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = this.GetInputDataset(DomainModelType.DataProductOwner);

        return new JoinAdapterResult
        {
            // CONCAT_WS, COLLECT_LIST is spark sql, cannot validate in synapse sql 
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as DPODataProductId,
                    COUNT(DataProductOwner.DataProductOwnerId) as DataProductOwnerCount,
                    CONCAT_WS(',', COLLECT_LIST(DataProductOwner.DataProductOwnerId)) as DataProductOwnerIds
                FROM DataProduct 
                LEFT JOIN DataProductOwner ON DataProduct.DataProductID = DataProductOwner.DataProductId
                GROUP BY DataProduct.DataProductID
            ) TDataProductOwner ON DataProduct.DataProductID = TDataProductOwner.DPODataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset },
            SchemaFromJoin = this.outputSchema
        };
    }
}
