namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetProjectAsItem;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class ObserverAdapter
{
    private string endpoint;
    private string containerName;
    private string dataProductId;
    private string dataAssetId;
    private DHAssessmentWrapper assessment;

    public ObserverAdapter(
        string endpoint,
        string containerName,
        string dataProductId,
        string dataAssetId,
        DHAssessmentWrapper assessment)
    {
        this.endpoint = endpoint;
        this.containerName = containerName;
        this.dataProductId = dataProductId;
        this.dataAssetId = dataAssetId;
        this.assessment = assessment;
    }

    public ObserverWrapper FromControlAssessment()
    {
        var observer = new BasicObserverWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, BasicObserverWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });

        observer.Name = "deh_control_" + Guid.NewGuid().ToString();
        observer.Description = "MDQ observer";

        observer.InputDatasets = this.GetDataProductInputDatasets();
        observer.Rules = this.GetRules();
        observer.FavouriteColumnPaths = Array.Empty<string>();

        var dataProductRef = new ReferenceObjectWrapper(new JObject());
        dataProductRef.Type = ReferenceType.DataProductReference;
        dataProductRef.ReferenceId = this.dataProductId;
        observer.DataProduct = dataProductRef;

        var dataAssetRef = new ReferenceObjectWrapper(new JObject());
        dataAssetRef.Type = ReferenceType.DataAssetReference;
        dataAssetRef.ReferenceId = this.dataAssetId;
        observer.DataAsset = dataAssetRef;

        return observer;
    }

    private IEnumerable<RuleWrapper> GetRules()
    {
        var convertedResult = DHAssessmentRulesAdapter.ToDqRules(this.assessment.Rules);

        var rules = convertedResult.CustomRules;
        rules.Add(this.GetAlwaysFailedRule());

        return rules;
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

    private IEnumerable<InputDatasetWrapper> GetDataProductInputDatasets()
    {
        var inputDataset = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetDataProductDataset().JObject }
        });
        inputDataset.Alias = "primary";
        inputDataset.Primary = true;
        /*inputDataset.Dataset = this.GetDataProductDataset();*/

        return new InputDatasetWrapper[]
        {
            inputDataset
        };
    }

    private DatasetWrapper GetDataProductDataset()
    {
        var dataset = new DeltaDatasetWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, DeltaDatasetWrapper.entityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });
        dataset.ProjectAs = Array.Empty<DatasetProjectAsItemWrapper>();
        dataset.DatasourceFQN = this.endpoint + "/";
        dataset.CompressionCodec = "snappy";

        var datasetLocation = new DatasetGen2FileLocationWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, DatasetGen2FileLocationWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });
        datasetLocation.FileSystem = this.containerName;
        datasetLocation.FolderPath = DataEstateHealthConstants.SOURCE_DP_FOLDER_PATH;
        dataset.Location = new[] { datasetLocation };

        var businessDomainRef = new ReferenceObjectWrapper(new JObject());
        businessDomainRef.Type = ReferenceType.BusinessDomainReference;
        businessDomainRef.ReferenceId = DataEstateHealthConstants.DEH_DOMAIN_ID;
        dataset.BusinessDomain = businessDomainRef;

        var datasetSchema = new DatasetSchemaWrapper(new JObject());
        datasetSchema.Origin = "DEH";
        datasetSchema.Items = this.GetDataProductSchema();
        dataset.NativeSchema = datasetSchema;

        var dataProductRef = new ReferenceObjectWrapper(new JObject());
        dataProductRef.Type = ReferenceType.DataProductReference;
        dataProductRef.ReferenceId = this.dataProductId;
        dataset.DataProduct = dataProductRef;

        var dataAssetRef = new ReferenceObjectWrapper(new JObject());
        dataAssetRef.Type = ReferenceType.DataAssetReference;
        dataAssetRef.ReferenceId = this.dataAssetId;
        dataset.DataAsset = dataAssetRef;

        return dataset;
    }

    private IEnumerable<DatasetSchemaItemWrapper> GetDataProductSchema()
    {
        var descriptionCol = new DatasetSchemaStringItemWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, DatasetSchemaStringItemWrapper.EntityType },
        });
        descriptionCol.Name = "DataProductDescription";

        return new DatasetSchemaItemWrapper[]
        {
            descriptionCol
        };
    }
}
