namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DataProductAssetHasOwnerJoinAdapter : JoinAdapter
{

    private readonly string[][] dataProductAssetDef =
    [
        ["DataProductId", "String"],
        ["DataAssetId", "String"],
        ["ActiveFlag", "Number", "true"]
    ];

    private readonly string[][] dataAssetOwnerDef =
    [
        ["DataAssetId", "String"],
        ["DataAssetOwnerId", "String"],
        ["ActiveFlag", "Number", "true"]
    ];

    private readonly string[][] outputSchemaDef =
    [
        ["DataProductAllRelatedAssetsHaveOwner", "boolean"],
    ];

    private List<DatasetSchemaItemWrapper> dataProductAssetSchema;
    private List<DatasetSchemaItemWrapper> dataAssetOwnerSchema;
    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductAssetHasOwnerJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.dataProductAssetSchema = SchemaUtils.GenerateSchemaFromDefinition(this.dataProductAssetDef);
        this.dataAssetOwnerSchema = SchemaUtils.GenerateSchemaFromDefinition(this.dataAssetOwnerDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset1 = new InputDatasetWrapper(new JObject()
        {
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DP_DA_ASSIGNMENT_PATH, this.dataProductAssetSchema).JObject }
        });
        inputDataset1.Alias = "DataProductAssetAssignmentForDAOwner";
        inputDataset1.Primary = false;

        var inputDataset2 = new InputDatasetWrapper(new JObject()
        {
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DA_OWNER_ASSIGNMENT_PATH, this.dataAssetOwnerSchema).JObject }
        });
        inputDataset2.Alias = "DataAssetOwnerAssignment";
        inputDataset2.Primary = false;

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as ADODataProductId,
                    CASE
                        WHEN COUNT(DataProductAssetAssignmentForDAOwner.DataAssetId) = COUNT(DataAssetOwnerAssignment.DataAssetOwnerId) THEN 'true'
                        ELSE 'false'
                    END as DataProductAllRelatedAssetsHaveOwner
                FROM DataProduct 
                LEFT JOIN DataProductAssetAssignmentForDAOwner
                    ON DataProduct.DataProductID = DataProductAssetAssignmentForDAOwner.DataProductId
                    AND DataProductAssetAssignmentForDAOwner.ActiveFlag = 1
                LEFT JOIN DataAssetOwnerAssignment
                    ON DataProductAssetAssignmentForDAOwner.DataAssetId = DataAssetOwnerAssignment.DataAssetId
                    AND DataAssetOwnerAssignment.ActiveFlag = 1
                GROUP BY DataProduct.DataProductID
            ) TDataProductAssetAssignment ON DataProduct.DataProductID = TDataProductAssetAssignment.ADODataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1, inputDataset2 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
