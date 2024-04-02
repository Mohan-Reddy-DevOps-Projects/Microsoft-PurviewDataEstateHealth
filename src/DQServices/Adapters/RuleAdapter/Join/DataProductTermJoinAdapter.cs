namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class DataProductTermJoinAdapter : DataQualityJoinAdapter
{
    private readonly string[][] outputSchemaDef =
    [
        ["DataProductTermCount", "long"],
        ["DataProductAllRelatedTermsMinimalDescription", "string"]
    ];

    private List<SparkSchemaItemWrapper> outputSchema;

    public DataProductTermJoinAdapter(RuleAdapterContext context) : base(context)
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
                    DPTCountDataProductId,
                    DataProductTermCount,
                    CASE
                        WHEN DataProductAllRelatedTermsMinimalDescription IS NULL THEN ''
                        ELSE DataProductAllRelatedTermsMinimalDescription
                    END as DataProductAllRelatedTermsMinimalDescription
                FROM (
                    SELECT
                        DataProduct.DataProductID as DPTCountDataProductId,
                        COUNT(GlossaryTerm.GlossaryTermId) as DataProductTermCount
                    FROM DataProduct 
                    LEFT JOIN GlossaryTermDataProductAssignment ON DataProduct.DataProductID = GlossaryTermDataProductAssignment.DataProductId
                        AND GlossaryTermDataProductAssignment.ActiveFlag = 1
                    LEFT JOIN GlossaryTerm ON GlossaryTermDataProductAssignment.GlossaryTermID = GlossaryTerm.GlossaryTermId
                        AND GlossaryTerm.Status = 'Published'
                    GROUP BY DataProduct.DataProductID
                ) TDPTCount
                LEFT JOIN (
                    SELECT
                        GlossaryTermDataProductAssignment.DataProductId as DPTDescriptionInOrderDPId,
                        GlossaryTerm.GlossaryDescription as DataProductAllRelatedTermsMinimalDescription,
                        ROW_NUMBER() OVER (PARTITION BY GlossaryTermDataProductAssignment.DataProductId ORDER BY LENGTH(GlossaryTerm.GlossaryDescription) ASC) as DESCRIPTION_LENGTH_ROW_NUM
                    FROM GlossaryTermDataProductAssignment
                    INNER JOIN GlossaryTerm ON GlossaryTermDataProductAssignment.GlossaryTermID = GlossaryTerm.GlossaryTermId
                        AND GlossaryTerm.Status = 'Published'
                    WHERE GlossaryTermDataProductAssignment.ActiveFlag = 1
                ) DPTDescriptionInOrder ON TDPTCount.DPTCountDataProductId = DPTDescriptionInOrder.DPTDescriptionInOrderDPId
                    AND DPTDescriptionInOrder.DESCRIPTION_LENGTH_ROW_NUM = 1
            ) TDataProductTerm ON DataProduct.DataProductID = TDataProductTerm.DPTCountDataProductId",
            inputDatasetsFromJoin = new List<InputDatasetWrapper>() { inputDataset1, inputDataset2 },
            SchemaFromJoin = this.outputSchema
        };
    }
}
