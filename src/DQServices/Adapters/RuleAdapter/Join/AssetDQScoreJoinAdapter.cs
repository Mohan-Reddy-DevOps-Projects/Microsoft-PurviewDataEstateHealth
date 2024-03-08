namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class AssetDQScoreJoinAdapter : JoinAdapter
{

    private readonly string[][] dataProductAssetDef =
    [
        ["DataProductId", "String"],
        ["DataAssetId", "String"],
        ["ActiveFlag", "Number", "true"]
    ];

    private readonly string[][] dqAssetRuleExecutionDef =
    [
        ["DataAssetId", "String"],
        ["AssetResultScore", "Number", "false"]
    ];

    private readonly string[][] outputSchemaDef =
    [
        ["DataProductHasDQScore", "boolean"],
        ["DataProductAllRelatedAssetsHaveDQScore", "boolean"]
    ];

    private List<DatasetSchemaItemWrapper> dataProductAssetSchema;
    private List<DatasetSchemaItemWrapper> dqAssetRuleExecutionSchema;
    private List<SparkSchemaItemWrapper> outputSchema;

    public AssetDQScoreJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.dataProductAssetSchema = SchemaUtils.GenerateSchemaFromDefinition(this.dataProductAssetDef);
        this.dqAssetRuleExecutionSchema = SchemaUtils.GenerateSchemaFromDefinition(this.dqAssetRuleExecutionDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset1 = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DP_DA_ASSIGNMENT_PATH, this.dataProductAssetSchema).JObject }
        });
        inputDataset1.Alias = "DataProductAssetAssignmentForDQSocre";
        inputDataset1.Primary = false;

        var inputDataset2 = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DQ_DA_RULE_EXECUTION_PATH, this.dataProductAssetSchema).JObject }
        });
        inputDataset2.Alias = "DataQualityAssetRuleExecution";
        inputDataset2.Primary = false;

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
                        WHEN COUNT(DataProductAssetAssignmentForDQSocre.DataAssetId) > 0
                            AND COUNT(DataProductAssetAssignmentForDQSocre.DataAssetId) = COUNT(TDataQualityAssetRuleExecution.DataAssetId) THEN 'true' 
                        ELSE 'false'
                    END AS DataProductAllRelatedAssetsHaveDQScore
                FROM DataProduct 
                LEFT JOIN DataProductAssetAssignmentForDQSocre ON DataProduct.DataProductID = DataProductAssetAssignmentForDQSocre.DataProductId
                    AND DataProductAssetAssignmentForDQSocre.ActiveFlag = 1
                LEFT JOIN (
                    SELECT 
                        DataAssetId,
                        MAX(AssetResultScore) AS AssetResultScore
                    FROM DataQualityAssetRuleExecution
                    WHERE AssetResultScore IS NOT NULL
                    GROUP BY DataAssetId
                ) TDataQualityAssetRuleExecution ON DataProductAssetAssignmentForDQSocre.DataAssetId = TDataQualityAssetRuleExecution.DataAssetId
                GROUP BY DataProduct.DataProductID
            ) DataProductAssetDQScore ON DataProduct.DataProductID = DataProductAssetDQScore.DADQSCountDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1, inputDataset2 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
