namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataProductTermsOfUseJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductHasDataUsagePurpose", "boolean"],
        ["DataProductTermsOfUseCount", "long"],
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductTermsOfUseJoinAdapter(RuleAdapterContext context) : base(context)
    {
        this.outputSchema = SchemaUtils.GenerateSparkSchemaFromDefinition(this.outputSchemaDef);
    }

    public override JoinAdapterResult Adapt()
    {
        var inputDataset = this.GetInputDataset(DomainModelType.DataProductTermsOfUse);

        return new JoinAdapterResult
        {
            JoinSql = @"LEFT JOIN (
                SELECT
                    DataProduct.DataProductID as TOUDataProductId,
                    COUNT(DataProductTermsOfUse.TermsOfUseId) as DataProductTermsOfUseCount,
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
