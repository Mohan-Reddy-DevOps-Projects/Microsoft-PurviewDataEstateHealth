namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class ObserverAdapter
{
    private RuleAdapterContext context;

    public ObserverAdapter(RuleAdapterContext context)
    {
        this.context = context;
    }

    public BasicObserverWrapper FromControlAssessment()
    {
        var observer = new BasicObserverWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, BasicObserverWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });

        observer.Name = "deh_control_" + Guid.NewGuid().ToString();
        observer.Description = "MDQ observer";

        // Analyse rule and join requirements
        var convertedResult = DHAssessmentRulesAdapter.ToDqRules(this.context, this.context.assessment.Rules);

        // Add more input datasets for join needs
        var inputDatasets = this.GetDataProductInputDatasets();
        foreach (var inputDataset in convertedResult.inputDatasetsFromJoin)
        {
            if (!inputDatasets.ContainsKey(inputDataset.Alias))
            {
                inputDatasets[inputDataset.Alias] = inputDataset;
            }
        }
        observer.InputDatasets = inputDatasets.Values;

        // Add projection sql and view schema
        var projection = new ProjectionWrapper(new JObject());
        projection.Type = ProjectionType.sql;
        projection.Script = convertedResult.ProjectionSql;
        var viewSchema = new DatasetSchemaWrapper(new JObject());
        viewSchema.Origin = "DEH";
        var viewSchemaItems = this.GetSparkDataProductSchema().ToList();
        viewSchemaItems.AddRange(convertedResult.SchemaFromJoin);
        projection.NativeSchema = viewSchemaItems;
        observer.Projection = projection;

        // Add all rules
        var rules = new List<CustomTruthRuleWrapper>() { this.GetAlwaysFailedRule() };
        rules.AddRange(convertedResult.CustomRules);
        observer.Rules = rules;
        observer.FavouriteColumnPaths = Array.Empty<string>();

        var dataProductRef = new ReferenceObjectWrapper(new JObject());
        dataProductRef.Type = ReferenceType.DataProductReference;
        dataProductRef.ReferenceId = this.context.dataProductId;
        observer.DataProduct = dataProductRef;

        var dataAssetRef = new ReferenceObjectWrapper(new JObject());
        dataAssetRef.Type = ReferenceType.DataAssetReference;
        dataAssetRef.ReferenceId = this.context.dataAssetId;
        observer.DataAsset = dataAssetRef;

        return observer;
    }

    private CustomTruthRuleWrapper GetAlwaysFailedRule()
    {
        var alwaysFailedRule = new CustomTruthRuleWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, CustomTruthRuleWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });
        alwaysFailedRule.Id = DataEstateHealthConstants.ALWAYS_FAIL_RULE_ID;
        alwaysFailedRule.Name = DataEstateHealthConstants.ALWAYS_FAIL_RULE_ID;
        alwaysFailedRule.Condition = "1 == 2";
        alwaysFailedRule.Status = RuleStatus.Active;

        return alwaysFailedRule;
    }

    private Dictionary<string, InputDatasetWrapper> GetDataProductInputDatasets()
    {
        var dataProductDataset = ObserverUtils.GetInputDataset(this.context, DomainModelType.DataProduct, true);

        return new Dictionary<string, InputDatasetWrapper>()
        {
            { dataProductDataset.Alias, dataProductDataset }
        };
    }

    private IEnumerable<SparkSchemaItemWrapper> GetSparkDataProductSchema()
    {
        return SchemaUtils.GenerateSparkSchemaFromDefinition([
            ["DataProductID", "string"],
            ["DataProductDisplayName", "string"],
            ["DataProductDescription", "string"],
            ["UseCases", "string"],
            ["Endorsed", "boolean"],
        ]);
    }
}
