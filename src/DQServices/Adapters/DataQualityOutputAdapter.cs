namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Output;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

public class DataQualityOutputAdapter
{
    public static IEnumerable<DHRawScore> ToScorePayload(
        IEnumerable<DataQualityDataProductOutputEntity> outputResult,
        IDataEstateHealthRequestLogger logger)
    {
        var entityDict = new Dictionary<string, DHRawScore>();

        // entity id -> rule id -> score list
        var ruleScoreDict = new Dictionary<string, Dictionary<string, List<double>>>();

        foreach (var outputEntity in outputResult)
        {
            if (string.IsNullOrEmpty(outputEntity.BusinessDomainId))
            {
                logger.LogWarning($"Skip data product without business domain id when parsing MDQ result, dataProductId:{outputEntity.DataProductID}");
                continue;
            }

            if (!outputEntity.DataProductStatusDisplayName.Equals(DataEstateHealthConstants.DP_PUBLISHED_STATUS_TEXT, System.StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogInformation($"Skip not published data product when parsing MDQ result, dataProductId:{outputEntity.DataProductID}");
                continue;
            }

            if (!entityDict.ContainsKey(outputEntity.DataProductID))
            {
                entityDict[outputEntity.DataProductID] = new DHRawScore()
                {
                    EntityType = RowScoreEntityType.DataProduct,
                    EntityPayload = new JObject()
                {
                    { DQOutputFields.DP_ID, outputEntity.DataProductID },
                    { DQOutputFields.DP_NAME, outputEntity.DataProductDisplayName },
                    { DQOutputFields.DP_STATUS, outputEntity.DataProductStatusDisplayName },
                    { DQOutputFields.BD_ID, outputEntity.BusinessDomainId },
                    { DQOutputFields.DP_OWNER_IDS, outputEntity.DataProductOwnerIds }
                },
                    Scores = new List<DHScoreUnitWrapper>()
                };
            }


            foreach (var ruleKeyValue in outputEntity.Result)
            {
                if (ruleKeyValue.Key != DataEstateHealthConstants.ALWAYS_FAIL_RULE_ID)
                {
                    if (!ruleScoreDict.ContainsKey(outputEntity.DataProductID))
                    {
                        ruleScoreDict[outputEntity.DataProductID] = new Dictionary<string, List<double>>();
                    }

                    if (!ruleScoreDict[outputEntity.DataProductID].ContainsKey(ruleKeyValue.Key))
                    {
                        ruleScoreDict[outputEntity.DataProductID][ruleKeyValue.Key] = new List<double>();
                    }

                    ruleScoreDict[outputEntity.DataProductID][ruleKeyValue.Key].Add(ruleKeyValue.Value == "PASS" ? 1 : 0);

                    // TODO perhaps will delete this log if it logs too much
                    logger.LogInformation($"MDQ rule result, dataProductId:${outputEntity.DataProductID}, ruleId:{ruleKeyValue.Key}, result:${ruleKeyValue.Value}");
                }
            }
        }

        foreach (var entityKeyVal in entityDict)
        {
            var scores = new List<DHScoreUnitWrapper>();
            foreach (var ruleKeyVal in ruleScoreDict[entityKeyVal.Key])
            {
                var unit = new DHScoreUnitWrapper(new JObject());
                unit.AssessmentRuleId = ruleKeyVal.Key; ;
                unit.Score = ruleKeyVal.Value.Average();
                scores.Add(unit);
            }
            entityKeyVal.Value.Scores = scores;
        }

        return entityDict.Values;
    }
}
