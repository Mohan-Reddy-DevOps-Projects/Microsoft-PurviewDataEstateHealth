namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Azure.Purview.DataEstateHealth.DataAccess.EntityModel.DataQualityOutput;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Output;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Newtonsoft.Json.Linq;
using System;
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
                    { DqOutputFields.DP_ID, outputEntity.DataProductID },
                    { DqOutputFields.DP_NAME, outputEntity.DataProductDisplayName },
                    { DqOutputFields.DP_STATUS, outputEntity.DataProductStatusDisplayName },
                    { DqOutputFields.BD_ID, outputEntity.BusinessDomainId },
                    { DqOutputFields.DP_OWNER_IDS, outputEntity.DataProductOwnerIds }
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

    public static IEnumerable<DHRawScore> ToScorePayloadForBusinessDomain(
        IEnumerable<DataQualityBusinessDomainOutputEntity> outputResult,
        IDataEstateHealthRequestLogger logger)
    {
        var entityDict = new Dictionary<string, DHRawScore>();

        // entity id -> rule id -> score list
        var ruleScoreDict = new Dictionary<string, Dictionary<string, List<double>>>();

        foreach (var outputEntity in outputResult)
        {
            if (String.IsNullOrEmpty(outputEntity.BusinessDomainId))
            {
                logger.LogWarning($"Skip business domain without business domain id when parsing MDQ result, businessDomainId:{outputEntity.BusinessDomainId}");
                continue;
            }

            if (!entityDict.ContainsKey(outputEntity.BusinessDomainId))
            {
                entityDict[outputEntity.BusinessDomainId] = new DHRawScore()
                {
                    EntityType = RowScoreEntityType.BusinessDomain,
                    EntityPayload = new JObject()
                {
                    { DqOutputFields.BD_ID, outputEntity.BusinessDomainId },
                    { "BusinessDomainCriticalDataElementCount", outputEntity.BusinessDomainCriticalDataElementCount }
                },
                    Scores = new List<DHScoreUnitWrapper>()
                };
            }

            foreach (var ruleKeyValue in outputEntity.Result.Where(ruleKeyValue => ruleKeyValue.Key != DataEstateHealthConstants.ALWAYS_FAIL_RULE_ID))
            {
                if (!ruleScoreDict.ContainsKey(outputEntity.BusinessDomainId))
                {
                    ruleScoreDict[outputEntity.BusinessDomainId] = new Dictionary<string, List<double>>();
                }

                if (!ruleScoreDict[outputEntity.BusinessDomainId].ContainsKey(ruleKeyValue.Key))
                {
                    ruleScoreDict[outputEntity.BusinessDomainId][ruleKeyValue.Key] = new List<double>();
                }

                ruleScoreDict[outputEntity.BusinessDomainId][ruleKeyValue.Key].Add(ruleKeyValue.Value == "PASS" ? 1 : 0);

                // TODO perhaps will delete this log if it logs too much
                logger.LogInformation($"MDQ rule result, businessDomainId:${outputEntity.BusinessDomainId}, ruleId:{ruleKeyValue.Key}, result:${ruleKeyValue.Value}");
            }
        }

        foreach (var entityKeyVal in entityDict)
        {
            var scores = ruleScoreDict[entityKeyVal.Key]
                .Select(ruleKeyVal => new DHScoreUnitWrapper(new JObject()) { AssessmentRuleId = ruleKeyVal.Key, Score = ruleKeyVal.Value.Average() })
                .ToList();
            entityKeyVal.Value.Scores = scores;
        }

        return entityDict.Values;
    }
}
