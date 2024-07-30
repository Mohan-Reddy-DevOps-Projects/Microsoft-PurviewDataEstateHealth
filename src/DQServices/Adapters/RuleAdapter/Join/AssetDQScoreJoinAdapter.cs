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
        ["DADQSDataAssetId", "string"],
        ["DataProductRelatedAssetsHaveDQScore", "boolean"]
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
        var inputDataset3 = this.GetInputDataset(DomainModelType.DataQualityRuleColumnExecution);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as DADQSCountDataProductId,
                    DataProductAssetAssignment.DataAssetId as DADQSDataAssetId,
                    CASE
                        WHEN COUNT(TDataQualityAssetRuleExecution.ResultScore) > 0 THEN 'true'
                        ELSE 'false'
                    END AS DataProductRelatedAssetsHaveDQScore
                FROM DataProduct 
                LEFT JOIN DataProductAssetAssignment ON DataProduct.DataProductID = DataProductAssetAssignment.DataProductId
                    AND DataProductAssetAssignment.ActiveFlag = 1
                LEFT JOIN (
                    SELECT 
                        DataAssetId,
                        MAX(ResultScore) AS ResultScore
                    FROM (
                        SELECT DataAssetId, AssetResultScore AS ResultScore
                        FROM DataQualityAssetRuleExecution
                        UNION ALL
                        SELECT DataAssetId, ColumnResultScore AS ResultScore
                        FROM DataQualityRuleColumnExecution
                    ) AS TAssetDQScore
                    WHERE ResultScore IS NOT NULL
                    GROUP BY DataAssetId
                ) TDataQualityAssetRuleExecution ON DataProductAssetAssignment.DataAssetId = TDataQualityAssetRuleExecution.DataAssetId
                GROUP BY DataProduct.DataProductID, DataProductAssetAssignment.DataAssetId
            ) DataProductAssetDQScore ON DataProduct.DataProductID = DataProductAssetDQScore.DADQSCountDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1, inputDataset2, inputDataset3 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
