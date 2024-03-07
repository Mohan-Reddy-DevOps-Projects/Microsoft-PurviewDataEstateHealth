namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DataAssetCountJoinAdapter : JoinAdapter
{

    private readonly string[][] dataProductAssetDef =
    [
        ["DataProductId", "String"],
        ["DataAssetId", "String"],
        ["ActiveFlag", "Number", "true"]
    ];

    private readonly string[][] outputSchemaDef =
    [
        ["DataAssetCount", "long"],
    ];

    private List<DatasetSchemaItemWrapper> dataProductAssetSchema;
    private List<SparkSchemaItemWrapper> outputSchema;

    public DataAssetCountJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.dataProductAssetSchema = SchemaUtils.GenerateSchemaFromDefinition(this.dataProductAssetDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DP_DA_ASSIGNMENT_PATH, this.dataProductAssetSchema).JObject }
        });
        inputDataset.Alias = "DataProductAssetAssignment";
        inputDataset.Primary = false;

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT DataProduct.DataProductID as DACountDataProductId, COUNT(DataProductAssetAssignment.DataAssetId) as DataAssetCount  
                FROM DataProduct 
                LEFT JOIN DataProductAssetAssignment ON DataProduct.DataProductID = DataProductAssetAssignment.DataProductId
                AND DataProductAssetAssignment.ActiveFlag = 1
                GROUP BY DataProduct.DataProductID
            ) DataAssetCount ON DataProduct.DataProductID = DataAssetCount.DACountDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset },
            SchemaFromJoin = this.outputSchema
        };
    }
}
