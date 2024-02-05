namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public static class ObserverAdapter
{
    public static ObserverWrapper FromControlAssessment()
    {
        var observer = new BasicObserverWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, BasicObserverWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });

        observer.Name = "deh_control_id_" + new Guid().ToString();

        observer.InputDatasets = GetDataProductInputDatasets();
        observer.Rules = GetRules();

        return observer;
    }

    private static IEnumerable<RuleWrapper> GetRules()
    {
        var hasDescriptionRule = new CustomTruthRuleWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, CustomTruthRuleWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });
        hasDescriptionRule.Id = "DPDescriptionNotNull";
        hasDescriptionRule.Name = "DPDescriptionNotNull";
        hasDescriptionRule.Condition = "HasDescription === true";

        return new RuleWrapper[]
        {
            hasDescriptionRule
        };
    }

    private static IEnumerable<InputDatasetWrapper> GetDataProductInputDatasets()
    {
        var inputDataset = new InputDatasetWrapper(new JObject());
        inputDataset.Alias = "primary";

        return new InputDatasetWrapper[]
        {
            inputDataset
        }.AsEnumerable();
    }

    private static IEnumerable<DatasetSchemaItemWrapper> GetDataProductSchema()
    {
        var descriptionCol = new DatasetSchemaStringItemWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, DatasetSchemaStringItemWrapper.EntityType },
        });
        descriptionCol.Name = "Description";

        return new DatasetSchemaItemWrapper[]
        {
            descriptionCol
        }.AsEnumerable();
    }
}
