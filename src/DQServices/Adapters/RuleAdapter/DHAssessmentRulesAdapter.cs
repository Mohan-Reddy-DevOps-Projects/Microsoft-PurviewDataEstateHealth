// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
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
        Dictionary<string, InputDatasetWrapper> inputDatasetsFromJoin = new Dictionary<string, InputDatasetWrapper>();

        if (context.joinRequirements.Count > 0)
        {
            projectionSqlList.Add(" SELECT * FROM DataProduct ");

            foreach (var joinRequirement in context.joinRequirements)
            {
                DataQualityJoinAdapter joinAdapter = null;

                switch (joinRequirement)
                {
                    case DataQualityJoinRequirement.BusinessDomain:
                        joinAdapter = new DataProductBusinessDomainJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataProductStatus:
                        joinAdapter = new DataProductStatusJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataAssetCount:
                        joinAdapter = new DataAssetCountJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataProductOwner:
                        joinAdapter = new DataProductOwnerJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataProductTerm:
                        joinAdapter = new DataProductTermJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataProductTermCount:
                        joinAdapter = new DataProductTermCountJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.HasAccessPolicySet:
                        joinAdapter = new HasAccessPolicySetJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataProductDQScore:
                        joinAdapter = new DataProductDQScoreJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataProductAssetDQScore:
                        joinAdapter = new AssetDQScoreJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataProductAssetsOwner:
                        joinAdapter = new DataProductAssetHasOwnerJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataProductAllAssetsOwner:
                        joinAdapter = new DataProductAllAssetHaveOwnerJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataAssetClassification:
                        joinAdapter = new AssetWithClassificationCountJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.BusinessDomainData:
                        joinAdapter = new DataProductBusinessDomainDataJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataProductTermsOfUseCount:
                        joinAdapter = new DataProductTermsOfUseJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.DataProductObjectivesCount:
                        joinAdapter = new DataProductObjectivesCountJoinAdapter(context);
                        break;
                    case DataQualityJoinRequirement.BusinessDomainCriticalDataElement:
                        joinAdapter = new BusinessDomainCriticalDataElementCountJoinAdapter(context);
                        break;
                    default:
                        throw new System.NotImplementedException("Join requirement: " + joinRequirement.ToString());
                }

                var joinResult = joinAdapter.Adapt();
                projectionSqlList.Add(joinResult.JoinSql);
                schemaFromJoin.AddRange(joinResult.SchemaFromJoin);
                foreach (var inputDataset in joinResult.inputDatasetsFromJoin)
                {
                    if (!inputDatasetsFromJoin.ContainsKey(inputDataset.Alias))
                    {
                        inputDatasetsFromJoin[inputDataset.Alias] = inputDataset;
                    }
                }
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
            inputDatasetsFromJoin = inputDatasetsFromJoin.Values.ToList(),
            CustomRules = customRules
        };
    }
}
