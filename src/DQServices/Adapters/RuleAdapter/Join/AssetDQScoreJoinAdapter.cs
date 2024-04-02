namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class AssetDQScoreJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductHasDQScore", "boolean"],
        ["DataProductAllRelatedAssetsHaveDQScore", "boolean"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public AssetDQScoreJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset1 = this.GetInputDataset(DomainModelType.DataProductAssetAssignment);
        var inputDataset2 = this.GetInputDataset(DomainModelType.DataQualityAssetRuleExecution);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as DADQSCountDataProductId,
                    CASE  
                        WHEN COUNT(TDataQualityAssetRuleExecution.AssetResultScore) > 0 THEN 'true'  
                        ELSE 'false'
                    END AS DataProductHasDQScore,
                    CASE  
                        WHEN COUNT(DataProductAssetAssignment.DataAssetId) > 0
                            AND COUNT(DataProductAssetAssignment.DataAssetId) = COUNT(TDataQualityAssetRuleExecution.DataAssetId) THEN 'true' 
                        ELSE 'false'
                    END AS DataProductAllRelatedAssetsHaveDQScore
                FROM DataProduct 
                LEFT JOIN DataProductAssetAssignment ON DataProduct.DataProductID = DataProductAssetAssignment.DataProductId
                    AND DataProductAssetAssignment.ActiveFlag = 1
                LEFT JOIN (
                    SELECT 
                        DataAssetId,
                        MAX(AssetResultScore) AS AssetResultScore
                    FROM DataQualityAssetRuleExecution
                    WHERE AssetResultScore IS NOT NULL
                    GROUP BY DataAssetId
                ) TDataQualityAssetRuleExecution ON DataProductAssetAssignment.DataAssetId = TDataQualityAssetRuleExecution.DataAssetId
                GROUP BY DataProduct.DataProductID
            ) DataProductAssetDQScore ON DataProduct.DataProductID = DataProductAssetDQScore.DADQSCountDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1, inputDataset2 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
