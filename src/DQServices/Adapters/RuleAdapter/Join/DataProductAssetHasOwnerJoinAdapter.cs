namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataProductAssetHasOwnerJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductAllRelatedAssetsHaveOwner", "boolean"],
        ["DataProductRelatedAssetsOwnerCount", "long"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductAssetHasOwnerJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset1 = this.GetInputDataset(DomainModelType.DataProductAssetAssignment);
        var inputDataset2 = this.GetInputDataset(DomainModelType.DataAssetOwnerAssignment);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as ADODataProductId,
                    CASE
                        WHEN COUNT(DISTINCT DataProductAssetAssignment.DataAssetId) = COUNT(DISTINCT DataAssetOwnerAssignment.DataAssetId) THEN 'true'
                        ELSE 'false'
                    END as DataProductAllRelatedAssetsHaveOwner,
                    DataAssetOwnerAssignment.DataAssetId as ADODataAssetId,
                    COUNT(DataAssetOwnerAssignment.DataAssetOwnerId) as DataProductRelatedAssetsOwnerCount
                FROM DataProduct 
                LEFT JOIN DataProductAssetAssignment
                    ON DataProduct.DataProductID = DataProductAssetAssignment.DataProductId
                    AND DataProductAssetAssignment.ActiveFlag = 1
                LEFT JOIN DataAssetOwnerAssignment
                    ON DataProductAssetAssignment.DataAssetId = DataAssetOwnerAssignment.DataAssetId
                    AND DataAssetOwnerAssignment.ActiveFlag = 1
                GROUP BY DataProduct.DataProductID, DataAssetOwnerAssignment.DataAssetId
            ) TDataProductAssetAssignment ON DataProduct.DataProductID = TDataProductAssetAssignment.ADODataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1, inputDataset2 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
