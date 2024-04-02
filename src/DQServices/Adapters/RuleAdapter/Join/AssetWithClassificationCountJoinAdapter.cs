namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class AssetWithClassificationCountJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductRelatedDataAssetsWithClassificationCount", "long"],
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public AssetWithClassificationCountJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset1 = this.GetInputDataset(DomainModelType.DataProductAssetAssignment);
        var inputDataset2 = this.GetInputDataset(DomainModelType.DataAssetColumnClassificationAssignment);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as DACDataProductId,
                    DataProductAssetAssignment.DataAssetId as DACDataAssetId,
                    COUNT(DataAssetColumnClassificationAssignment.ClassificationId) as DataProductRelatedDataAssetsWithClassificationCount
                FROM DataProduct 
                LEFT JOIN DataProductAssetAssignment
                    ON DataProduct.DataProductID = DataProductAssetAssignment.DataProductId
                    AND DataProductAssetAssignment.ActiveFlag = 1
                LEFT JOIN DataAssetColumnClassificationAssignment
                    ON DataProductAssetAssignment.DataAssetId = DataAssetColumnClassificationAssignment.DataAssetId
                GROUP BY DataProduct.DataProductID, DataProductAssetAssignment.DataAssetId
            ) TDataAssetColumnClassificationAssignment ON DataProduct.DataProductID = TDataAssetColumnClassificationAssignment.DACDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1, inputDataset2 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
