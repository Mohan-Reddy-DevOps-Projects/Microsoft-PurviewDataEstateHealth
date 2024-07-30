namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataProductDQScoreJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductHasDQScore", "boolean"],
        ["DataProductAllRelatedAssetsHaveDQScore", "boolean"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductDQScoreJoinAdapter(RuleAdapterContext context) : base(context)
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
                    DataProduct.DataProductID as DPDQSDataProductId,
                    CASE  
                        WHEN COUNT(TDataQualityAssetRuleExecution.ResultScore) > 0 THEN 'true'  
                        ELSE 'false'
                    END AS DataProductHasDQScore,
                    CASE  
                        WHEN COUNT(DataProductAssetAssignment.DataAssetId) > 0
                            AND COUNT(DISTINCT DataProductAssetAssignment.DataAssetId) = COUNT(DISTINCT TDataQualityAssetRuleExecution.DataAssetId) THEN 'true' 
                        ELSE 'false'
                    END AS DataProductAllRelatedAssetsHaveDQScore
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
                    )
                    WHERE ResultScore IS NOT NULL
                    GROUP BY DataAssetId
                ) TDataQualityAssetRuleExecution ON DataProductAssetAssignment.DataAssetId = TDataQualityAssetRuleExecution.DataAssetId
                GROUP BY DataProduct.DataProductID
            ) DataProductDQScore ON DataProduct.DataProductID = DataProductDQScore.DPDQSDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1, inputDataset2, inputDataset3 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
