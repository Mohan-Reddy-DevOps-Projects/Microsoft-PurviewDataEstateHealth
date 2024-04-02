namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataProductStatusJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductStatusDisplayName", "string"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductStatusJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = this.GetInputDataset(DomainModelType.DataProductStatus);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT DataProductStatusID as DPSDataProductStatusID,
                    DataProductStatusDisplayName
                FROM DataProductStatus
            ) TDataProductStatus ON DataProduct.DataProductStatusID = TDataProductStatus.DPSDataProductStatusID",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset },
            SchemaFromJoin = this.outputSchema
        };
    }
}
