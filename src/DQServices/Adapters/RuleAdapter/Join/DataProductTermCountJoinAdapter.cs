namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DataProductTermCountJoinAdapter : JoinAdapter
{

    private readonly string[][] dataProductTermDef =
    [
        ["GlossaryTermID", "String"],
        ["DataProductId", "String"],
        ["ActiveFlag", "Number", "true"]
    ];

    private readonly string[][] outputSchemaDef =
    [
        ["DataProductTermCount", "long"],
    ];

    private List<DatasetSchemaItemWrapper> dataProductTermSchema;
    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductTermCountJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.dataProductTermSchema = SchemaUtils.GenerateSchemaFromDefinition(this.dataProductTermDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DP_TERM_ASSIGNMENT_PATH, this.dataProductTermSchema).JObject }
        });
        inputDataset.Alias = "GlossaryTermDataProductAssignment";
        inputDataset.Primary = false;

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT DataProduct.DataProductID as DPTCountDataProductId, COUNT(GlossaryTermDataProductAssignment.GlossaryTermID) as DataProductTermCount  
                FROM DataProduct 
                LEFT JOIN GlossaryTermDataProductAssignment ON DataProduct.DataProductID = GlossaryTermDataProductAssignment.DataProductId
                AND GlossaryTermDataProductAssignment.ActiveFlag = 1
                GROUP BY DataProduct.DataProductID
            ) DataProductTermCount ON DataProduct.DataProductID = DataProductTermCount.DPTCountDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset },
            SchemaFromJoin = this.outputSchema
        };
    }
}
