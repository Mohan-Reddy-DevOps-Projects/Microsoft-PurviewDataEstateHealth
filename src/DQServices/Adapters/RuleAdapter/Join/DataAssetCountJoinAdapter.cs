namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataAssetCountJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataAssetCount", "long"],
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataAssetCountJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = this.GetInputDataset(DomainModelType.DataProductAssetAssignment);

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
