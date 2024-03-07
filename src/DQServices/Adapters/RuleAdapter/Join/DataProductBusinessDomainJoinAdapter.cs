namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DataProductBusinessDomainJoinAdapter : JoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["BusinessDomainId", "string"]
    ];

    private readonly string[][] dataProductBusinessDomainDef =
    [
        ["DataProductID", "String"],
        ["BusinessDomainId", "String"],
        ["ActiveFlag", "Number", "true"]
    ];

    private List<DatasetSchemaItemWrapper> dataProductBusinessDomain;
    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductBusinessDomainJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.dataProductBusinessDomain = SchemaUtils.GenerateSchemaFromDefinition(this.dataProductBusinessDomainDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DP_BD_ASSIGNMENT_PATH, this.dataProductBusinessDomain).JObject }
        });
        inputDataset.Alias = "DataProductBusinessDomainAssignment";
        inputDataset.Primary = false;

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
