// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

public class DHAssessmentRulesAdapter
{
    public static RuleAdapterResult ToDqRules(RuleAdapterContext context, IEnumerable<DHAssessmentRuleWrapper> controlRules)
    {
        var customRules = new List<CustomTruthRuleWrapper>();

        foreach (var controlRule in controlRules)
        {
            var customRule = new CustomTruthRuleWrapper(new JObject()
            {
                { DynamicEntityWrapper.keyType, CustomTruthRuleWrapper.EntityType },
                { DynamicEntityWrapper.keyTypeProperties, new JObject() }
            });
            customRule.Id = controlRule.Id;
            customRule.Name = controlRule.Id;
            customRule.Status = RuleStatus.Active;
            customRule.Condition = DHRuleAdapter.ToDQExpression(context, controlRule.Rule);

            customRules.Add(customRule);
        }

        List<string> projectionSqlList = new List<string>(); ;
        List<SparkSchemaItemWrapper> schemaFromJoin = new List<SparkSchemaItemWrapper>();
        List<InputDatasetWrapper> inputDatasetsFromJoin = new List<InputDatasetWrapper>();

        if (context.joinRequirements.Count > 0)
        {
            projectionSqlList.Add(" SELECT * FROM DataProduct ");

            foreach (var joinRequirement in context.joinRequirements)
            {
                JoinAdapter joinAdapter = null;

                switch (joinRequirement)
                {
                    case JoinRequirement.BusinessDomain:
                        joinAdapter = new DataProductBusinessDomainJoinAdapter(context);
                        break;
                    case JoinRequirement.DataProductStatus:
                        joinAdapter = new DataProductStatusJoinAdapter(context);
                        break;
                    case JoinRequirement.DataAssetCount:
                        joinAdapter = new DataAssetCountJoinAdapter(context);
                        break;
                    case JoinRequirement.DataProductOwner:
                        joinAdapter = new DataProductOwnerJoinAdapter(context);
                        break;
                    case JoinRequirement.DataProductTerm:
                        joinAdapter = new DataProductTermJoinAdapter(context);
                        break;
                    case JoinRequirement.HasAccessPolicySetAndPurpose:
                        joinAdapter = new HasAccessPolicySetAndPurposeJoinAdapter(context);
                        break;
                    case JoinRequirement.DataProductAssetDQScore:
                        joinAdapter = new AssetDQScoreJoinAdapter(context);
                        break;
                    case JoinRequirement.DataProductAssetsOwner:
                        joinAdapter = new DataProductAssetHasOwnerJoinAdapter(context);
                        break;
                    case JoinRequirement.DataAssetClassification:
                        joinAdapter = new AssetWithClassificationCountJoinAdapter(context);
                        break;
                    case JoinRequirement.BusinessDomainData:
                        joinAdapter = new DataProductBusinessDomainDataJoinAdapter(context);
                        break;
                    default:
                        throw new System.NotImplementedException("Join requirement: " + joinRequirement.ToString());
                }

                var joinResult = joinAdapter.Adapt();
                projectionSqlList.Add(joinResult.JoinSql);
                schemaFromJoin.AddRange(joinResult.SchemaFromJoin);
                inputDatasetsFromJoin.AddRange(joinResult.inputDatasetsFromJoin);
            }
        }

        if (context.fitlerDomainIds != null && context.fitlerDomainIds.Any())
        {
            var filterDomainIds = string.Join(",", context.fitlerDomainIds.Select(domainId => $"'{domainId}'"));
            projectionSqlList.Add($" WHERE BusinessDomainId IN ({filterDomainIds}) ");
        }

        return new RuleAdapterResult()
        {
            ProjectionSql = string.Join(" ", projectionSqlList),
            SchemaFromJoin = schemaFromJoin,
            inputDatasetsFromJoin = inputDatasetsFromJoin,
            CustomRules = customRules
        };
    }
}
