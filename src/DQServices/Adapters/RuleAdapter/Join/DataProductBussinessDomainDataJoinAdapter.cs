namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DataProductBusinessDomainDataJoinAdapter : JoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["BusinessDomainDescription", "string"],
        ["DataProductDomainHasOwner", "boolean"]
    ];

    private readonly string[][] businessDomainDef =
    [
        ["BusinessDomainId", "String"],
        ["BusinessDomainDescription", "String"],
    ];

    private List<DatasetSchemaItemWrapper> businessDomain;
    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductBusinessDomainDataJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.businessDomain = SchemaUtils.GenerateSchemaFromDefinition(this.businessDomainDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_BD_PATH, this.businessDomain).JObject }
        });
        inputDataset.Alias = "BusinessDomain";
        inputDataset.Primary = false;

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
