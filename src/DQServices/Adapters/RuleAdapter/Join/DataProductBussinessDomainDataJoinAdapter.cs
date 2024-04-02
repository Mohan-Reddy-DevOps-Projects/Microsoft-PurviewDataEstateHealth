namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataProductBusinessDomainDataJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["BusinessDomainDescription", "string"],
        ["DataProductDomainHasOwner", "boolean"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductBusinessDomainDataJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = this.GetInputDataset(DomainModelType.BusinessDomain);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT BusinessDomainId as DPBusinessDomainId,
                    BusinessDomainDescription,
                    'true' AS DataProductDomainHasOwner
                FROM BusinessDomain
                ) TBusinessDomain
                ON TDataProductBusinessDomainAssignment.BusinessDomainId = TBusinessDomain.DPBusinessDomainId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset },
            SchemaFromJoin = this.outputSchema
        };
    }
}
