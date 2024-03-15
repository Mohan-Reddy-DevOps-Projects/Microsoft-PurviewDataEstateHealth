namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class AssetWithClassificationCountJoinAdapter : JoinAdapter
{

    private readonly string[][] dataProductAssetDef =
    [
        ["DataProductId", "String"],
        ["DataAssetId", "String"],
        ["ActiveFlag", "Number", "true"]
    ];

    private readonly string[][] dataAseetColumnClassificationDef = [
        ["DataAssetId", "String"],
        ["ColumnId", "String"],
        ["ClassificationId", "String"]
    ];

    private readonly string[][] outputSchemaDef =
    [
        ["DataProductRelatedDataAssetsWithClassificationCount", "long"],
    ];

    private List<DatasetSchemaItemWrapper> dataProductAssetSchema;
    private List<DatasetSchemaItemWrapper> dataAseetColumnClassificationSchema;
    private List<SparkSchemaItemWrapper> outputSchema;

    public AssetWithClassificationCountJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.dataProductAssetSchema = SchemaUtils.GenerateSchemaFromDefinition(this.dataProductAssetDef);
        this.dataAseetColumnClassificationSchema = SchemaUtils.GenerateSchemaFromDefinition(this.dataAseetColumnClassificationDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset1 = new InputDatasetWrapper(new JObject()
        {
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DP_DA_ASSIGNMENT_PATH, this.dataProductAssetSchema).JObject }
        });
        inputDataset1.Alias = "DataProductAssetAssignmentForDAC";
        inputDataset1.Primary = false;

        var inputDataset2 = new InputDatasetWrapper(new JObject()
        {
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DA_CLASSIFICATION_ASSIGNMENT_PATH, this.dataAseetColumnClassificationSchema).JObject }
        });
        inputDataset2.Alias = "DataAssetColumnClassificationAssignment";
        inputDataset2.Primary = false;

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as DACDataProductId,
                    COUNT(DataAssetColumnClassificationAssignment.ClassificationId) as DataProductRelatedDataAssetsWithClassificationCount
                FROM DataProduct 
                LEFT JOIN DataProductAssetAssignmentForDAC
                    ON DataProduct.DataProductID = DataProductAssetAssignmentForDAC.DataProductId
                    AND DataProductAssetAssignmentForDAC.ActiveFlag = 1
                LEFT JOIN DataAssetColumnClassificationAssignment
                    ON DataProductAssetAssignmentForDAC.DataAssetId = DataAssetColumnClassificationAssignment.DataAssetId
                GROUP BY DataProduct.DataProductID
            ) TDataAssetColumnClassificationAssignment ON DataProduct.DataProductID = TDataAssetColumnClassificationAssignment.DACDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1, inputDataset2 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
