namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataProductObjectivesCountJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductObjectivesCount", "long"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductObjectivesCountJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = this.GetInputDataset(DomainModelType.DataProductOKRAssignment);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as DPOCDataProductId,
                    COUNT(DataProductOKRAssignment.ObjectiveId) as DataProductObjectivesCount
                FROM DataProduct 
                LEFT JOIN DataProductOKRAssignment ON DataProduct.DataProductID = DataProductOKRAssignment.DataProductId
                GROUP BY DataProduct.DataProductID
            ) DPOCDataProductObjective ON DataProduct.DataProductID = DPOCDataProductObjective.DPOCDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset },
            SchemaFromJoin = this.outputSchema
        };
    }
} 