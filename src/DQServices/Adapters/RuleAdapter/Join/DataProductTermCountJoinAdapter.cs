namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataProductTermCountJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductTermCount", "long"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductTermCountJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset1 = this.GetInputDataset(DomainModelType.GlossaryTermDataProductAssignment);
        var inputDataset2 = this.GetInputDataset(DomainModelType.GlossaryTerm);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as DPTCDataProductId,
                    COUNT(GlossaryTerm.GlossaryTermId) as DataProductTermCount
                FROM DataProduct 
                LEFT JOIN GlossaryTermDataProductAssignment ON DataProduct.DataProductID = GlossaryTermDataProductAssignment.DataProductId
                    AND GlossaryTermDataProductAssignment.ActiveFlag = 1
                LEFT JOIN GlossaryTerm ON GlossaryTermDataProductAssignment.GlossaryTermID = GlossaryTerm.GlossaryTermId
                    AND GlossaryTerm.Status = 'Published'
                GROUP BY DataProduct.DataProductID
            ) DPTCDataProductTerm ON DataProduct.DataProductID = DPTCDataProductTerm.DPTCDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1, inputDataset2 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
