namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataProductBusinessDomainJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["BusinessDomainId", "string"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductBusinessDomainJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = this.GetInputDataset(DomainModelType.DataProductBusinessDomainAssignment);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT DataProductID as DPBDDataProductId,
                    BusinessDomainId
                FROM DataProductBusinessDomainAssignment
                WHERE ActiveFlag = 1
                ) TDataProductBusinessDomainAssignment
                ON DataProduct.DataProductID = TDataProductBusinessDomainAssignment.DPBDDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset },
            SchemaFromJoin = this.outputSchema
        };
    }
}
