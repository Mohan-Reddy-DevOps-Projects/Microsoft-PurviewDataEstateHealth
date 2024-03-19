namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Output;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DataQualityOutputAdapter
{
    public static IEnumerable<DHRawScore> ToScorePayload(
        IEnumerable<DataQualityDataProductOutputEntity> outputResult,
        IDataEstateHealthRequestLogger logger)
    {
        var result = new List<DHRawScore>();

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

            var scores = new List<DHScoreUnitWrapper>();

            foreach (var ruleKeyValue in outputEntity.Result)
            {
                if (ruleKeyValue.Key != DataEstateHealthConstants.ALWAYS_FAIL_RULE_ID)
                {
                    var unit = new DHScoreUnitWrapper(new JObject());
                    unit.AssessmentRuleId = ruleKeyValue.Key; ;
                    unit.Score = ruleKeyValue.Value == "PASS" ? 1 : 0;
                    scores.Add(unit);
                }
            }

            result.Add(new DHRawScore()
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
                Scores = scores
            });
        }

        return result;
    }
}
