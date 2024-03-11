namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
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
    private static readonly string COL_NAME_RULE_NAME = "key";
    private static readonly string COL_NAME_RULE_RESULT = "value";

    public static async Task<IEnumerable<DHRawScore>> ToScorePayload(MemoryStream parquetStream, IDataEstateHealthRequestLogger logger)
    {
        var result = new List<DHRawScore>();

        using (var parquetReader = await ParquetReader.CreateAsync(parquetStream).ConfigureAwait(false))
        {
            // Get the schema  
            var schema = parquetReader.Schema;
            var dataFields = schema.GetDataFields();
            var idColumnField = dataFields.FirstOrDefault(c => c.Name == DQOutputFields.DP_ID, null);
            var nameColumnField = dataFields.FirstOrDefault(c => c.Name == DQOutputFields.DP_NAME, null);
            var statusColumnField = dataFields.FirstOrDefault(c => c.Name == DQOutputFields.DP_STATUS, null);
            var bdIdColumnField = dataFields.FirstOrDefault(c => c.Name == DQOutputFields.BD_ID, null);
            var ownerIdsColumnField = dataFields.FirstOrDefault(c => c.Name == DQOutputFields.DP_OWNER_IDS, null);
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
                            var nameColumn = await groupReader.ReadColumnAsync(nameColumnField).ConfigureAwait(false);
                            var statusColumn = await groupReader.ReadColumnAsync(statusColumnField).ConfigureAwait(false);
                            var bdIdColumn = await groupReader.ReadColumnAsync(bdIdColumnField).ConfigureAwait(false);
                            var ownerIdsColumn = await groupReader.ReadColumnAsync(ownerIdsColumnField).ConfigureAwait(false);
                            var ruleNamesColumn = await groupReader.ReadColumnAsync(ruleNameColumnField).ConfigureAwait(false);
                            var ruleResultsColumn = await groupReader.ReadColumnAsync(ruleResultColumnField).ConfigureAwait(false);

                            var id = ((string[])idColumn.Data)[0];
                            var name = ((string[])nameColumn.Data)[0];
                            var status = ((string[])statusColumn.Data)[0];
                            var bdId = ((string[])bdIdColumn.Data)[0];
                            var ownerIds = ((string[])ownerIdsColumn.Data)[0];
                            var ruleNames = ((string[])ruleNamesColumn.Data);
                            var ruleResults = ((string[])ruleResultsColumn.Data);

                            if (string.IsNullOrEmpty(bdId))
                            {
                                logger.LogWarning($"Skip data product without business domain id when parsing MDQ result, dataProductId:{id}");
                                continue;
                            }

                            var scores = new List<DHScoreUnitWrapper>();

                            for (int ruleIndex = 0; ruleIndex < ruleNames.Length; ruleIndex++)
                            {
                                if (ruleNames[ruleIndex] != DataEstateHealthConstants.ALWAYS_FAIL_RULE_ID)
                                {
                                    var unit = new DHScoreUnitWrapper(new JObject());
                                    unit.AssessmentRuleId = ruleNames[ruleIndex];
                                    unit.Score = ruleResults[ruleIndex] == "PASS" ? 1 : 0;
                                    scores.Add(unit);
                                }
                            }

                            result.Add(new DHRawScore()
                            {
                                EntityType = RowScoreEntityType.DataProduct,
                                EntityPayload = new JObject()
                                {
                                    { DQOutputFields.DP_ID, id },
                                    { DQOutputFields.DP_NAME, name },
                                    { DQOutputFields.DP_STATUS, status },
                                    { DQOutputFields.BD_ID, bdId },
                                    { DQOutputFields.DP_OWNER_IDS, ownerIds }
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
