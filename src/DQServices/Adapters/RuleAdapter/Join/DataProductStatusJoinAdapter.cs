namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DataProductStatusJoinAdapter : JoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductStatusDisplayName", "string"]
    ];

    private readonly string[][] statusSchemaDef =
    [
        ["DataProductStatusID", "String"],
        ["DataProductStatusDisplayName", "String"]
    ];

    private List<DatasetSchemaItemWrapper> statusSchema;
    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductStatusJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.statusSchema = SchemaUtils.GenerateSchemaFromDefinition(this.statusSchemaDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DP_STATUS_FOLDER_PATH, this.statusSchema).JObject }
        });
        inputDataset.Alias = "DataProductStatus";
        inputDataset.Primary = false;

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
