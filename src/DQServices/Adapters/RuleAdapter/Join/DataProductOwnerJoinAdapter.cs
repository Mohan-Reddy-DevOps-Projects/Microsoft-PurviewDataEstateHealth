namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DataProductOwnerJoinAdapter : JoinAdapter
{

    private readonly string[][] dataProductOwnerDef =
    [
        ["DataProductId", "String"],
        ["DataProductOwnerId", "String"]
    ];

    private readonly string[][] outputSchemaDef =
    [
        ["DataProductOwnerCount", "long"],
        ["DataProductOwnerIds", "string"],
    ];

    private List<DatasetSchemaItemWrapper> dataProductOwnerSchema;
    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductOwnerJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.dataProductOwnerSchema = SchemaUtils.GenerateSchemaFromDefinition(this.dataProductOwnerDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DP_OWNER_PATH, this.dataProductOwnerSchema).JObject }
        });
        inputDataset.Alias = "DataProductOwner";
        inputDataset.Primary = false;

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
