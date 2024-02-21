namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
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

    public ObserverAdapter(
        string endpoint,
        string containerName,
        string dataProductId,
        string dataAssetId)
    {
        this.endpoint = endpoint;
        this.containerName = containerName;
        this.dataProductId = dataProductId;
        this.dataAssetId = dataAssetId;
    }

    public ObserverWrapper FromControlAssessment()
    {
        var observer = new BasicObserverWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, BasicObserverWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });

        observer.Name = "deh_control_id_" + Guid.NewGuid().ToString();
        observer.Description = "MDQ observer";

        observer.InputDatasets = this.GetDataProductInputDatasets();
        observer.Rules = this.GetRules();
        observer.FavouriteColumnPaths = Array.Empty<string>();

        var dataProductRef = new ReferenceObjectWrapper(new JObject());
        dataProductRef.Type = ReferenceType.DataProductReference;
        dataProductRef.ReferenceId = Guid.NewGuid().ToString();
        observer.DataProduct = dataProductRef;

        var dataAssetRef = new ReferenceObjectWrapper(new JObject());
        dataAssetRef.Type = ReferenceType.DataAssetReference;
        dataAssetRef.ReferenceId = Guid.NewGuid().ToString();
        observer.DataAsset = dataAssetRef;

        return observer;
    }

    private IEnumerable<RuleWrapper> GetRules()
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

    private IEnumerable<InputDatasetWrapper> GetDataProductInputDatasets()
    {
        var inputDataset = new InputDatasetWrapper(new JObject());
        inputDataset.Alias = "primary";
        inputDataset.Primary = true;
        inputDataset.Dataset = this.GetDataProductDataset();

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

        var datasetLocation = new DatasetGen2FileLocationWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, DatasetGen2FileLocationWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });
        datasetLocation.FileSystem = this.containerName;
        datasetLocation.FolderPath = "Sink/DataProductSchema";
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
        descriptionCol.Name = "Description";

        return new DatasetSchemaItemWrapper[]
        {
            descriptionCol
        };
    }
}
