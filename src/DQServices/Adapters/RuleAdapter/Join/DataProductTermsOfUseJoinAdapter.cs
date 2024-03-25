namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DataProductTermsOfUseJoinAdapter : JoinAdapter
{

    private readonly string[][] dataProductTermsOfUseDef =
    [
        ["DataProductId", "String"],
        ["TermsOfUseId", "String"]
    ];

    private readonly string[][] outputSchemaDef =
    [
        ["DataProductHasDataUsagePurpose", "boolean"],
    ];

    private List<DatasetSchemaItemWrapper> dataProductTermsOfUseSchema;
    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductTermsOfUseJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.dataProductTermsOfUseSchema = SchemaUtils.GenerateSchemaFromDefinition(this.dataProductTermsOfUseDef);
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetBasicDataset(DataEstateHealthConstants.SOURCE_DP_TOU_PATH, this.dataProductTermsOfUseSchema).JObject }
        });
        inputDataset.Alias = "DataProductTermsOfUse";
        inputDataset.Primary = false;

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as TOUDataProductId,
                    CASE
                        WHEN COUNT(DataProductTermsOfUse.TermsOfUseId) > 0 THEN 'true'
                        ELSE 'false'
                    END as DataProductHasDataUsagePurpose
                FROM DataProduct 
                LEFT JOIN DataProductTermsOfUse ON DataProduct.DataProductID = DataProductTermsOfUse.DataProductId
                GROUP BY DataProduct.DataProductID
            ) DataProductTermsOfUse ON DataProduct.DataProductID = DataProductTermsOfUse.TOUDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset },
            SchemaFromJoin = this.outputSchema
        };
    }
}
