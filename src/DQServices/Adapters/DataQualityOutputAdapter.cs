namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Output;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Newtonsoft.Json.Linq;
using Parquet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class DataQualityOutputAdapter
{
    private static readonly string COL_NAME_DP_ID = "DataProductId";
    // TODO switch to new model
    //private static readonly string COL_NAME_DP_ID = "DataProductId";
    //private static readonly string COL_NAME_DP_NAME = "DataProductDisplayName";
    private static readonly string COL_NAME_RULE_NAME = "key";
    private static readonly string COL_NAME_RULE_RESULT = "value";

    public static async Task<IEnumerable<DHRawScore>> ToScorePayload(MemoryStream parquetStream)
    {
        var result = new List<DHRawScore>();

        using (var parquetReader = await ParquetReader.CreateAsync(parquetStream).ConfigureAwait(false))
        {
            // Get the schema  
            var schema = parquetReader.Schema;
            var dataFields = schema.GetDataFields();
            var idColumnField = dataFields.FirstOrDefault(c => c.Name == COL_NAME_DP_ID, null);
            // var nameColumnField = dataFields.FirstOrDefault(c => c.Name == COL_NAME_DP_NAME, null);
            var ruleNameColumnField = dataFields.FirstOrDefault(c => c.Name == COL_NAME_RULE_NAME, null);
            var ruleResultColumnField = dataFields.FirstOrDefault(c => c.Name == COL_NAME_RULE_RESULT, null);

            if (idColumnField != null && ruleNameColumnField != null && ruleResultColumnField != null)
            {
                for (int rowGroupIndex = 0; rowGroupIndex < parquetReader.RowGroupCount; rowGroupIndex++)
                {
                    using (ParquetRowGroupReader groupReader = parquetReader.OpenRowGroupReader(rowGroupIndex))
                    {

                        // Process the data in columns.  
                        for (int rowIndex = 0; rowIndex < groupReader.RowCount; rowIndex++)
                        {
                            // Read all columns in the Parquet file.  
                            var idColumn = await groupReader.ReadColumnAsync(idColumnField).ConfigureAwait(false);
                            //var nameColumn = await groupReader.ReadColumnAsync(nameColumnField).ConfigureAwait(false);
                            var ruleNamesColumn = await groupReader.ReadColumnAsync(ruleNameColumnField).ConfigureAwait(false);
                            var ruleResultsColumn = await groupReader.ReadColumnAsync(ruleResultColumnField).ConfigureAwait(false);

                            var id = ((string[])idColumn.Data)[0];
                            //var name = ((string[])nameColumn.Data)[0];
                            var ruleNames = ((string[])ruleNamesColumn.Data);
                            var ruleResults = ((string[])ruleResultsColumn.Data);

                            var scores = new List<DHScoreUnitWrapper>();

                            for (int ruleIndex = 0; ruleIndex < ruleNames.Length; ruleIndex++)
                            {
                                var unit = new DHScoreUnitWrapper(new JObject());
                                // TODO jar
                                if (ruleNames[ruleIndex] == "DPDescriptionNotNull")
                                {
                                    unit.AssessmentRuleId = "DPDescriptionNotNull";
                                }
                                else
                                {
                                    unit.AssessmentRuleId = ruleNames[ruleIndex];
                                }
                                unit.Score = ruleResults[ruleIndex] == "PASS" ? 1 : 0;
                                scores.Add(unit);
                            }

                            result.Add(new DHRawScore()
                            {
                                EntityType = RowScoreEntityType.DataProduct,
                                EntityPayload = new JObject()
                                {
                                    { DQOutputFields.DP_ID, id },
                                    // { COL_NAME_DP_NAME, name }
                                },
                                Scores = scores
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
