namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetProjectAsItem;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public static class ObserverUtils
{
    // Returns a partial location
    private static DatasetGen2FileLocationWrapper GetBasicGen2FileLocation(RuleAdapterContext context)
    {
        var datasetLocation = new DatasetGen2FileLocationWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, DatasetGen2FileLocationWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });
        datasetLocation.FileSystem = context.containerName;
        return datasetLocation;
    }

    // Returns a partial dataset
    private static DeltaDatasetWrapper GetBasicDataset(RuleAdapterContext context, string folderPath, List<DatasetSchemaItemWrapper> schema)
    {
        var dataset = new DeltaDatasetWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, DeltaDatasetWrapper.entityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });
        dataset.ProjectAs = Array.Empty<DatasetProjectAsItemWrapper>();
        dataset.DatasourceFQN = context.endpoint + "/";
        dataset.CompressionCodec = "snappy";

        var datasetLocation = GetBasicGen2FileLocation(context);
        datasetLocation.FolderPath = folderPath;
        dataset.Location = new[] { datasetLocation };
        var datasetSchema = new DatasetSchemaWrapper(new JObject());
        datasetSchema.Origin = "DEH";
        datasetSchema.Items = schema;
        dataset.NativeSchema = datasetSchema;

        var businessDomainRef = new ReferenceObjectWrapper(new JObject());
        businessDomainRef.Type = ReferenceType.BusinessDomainReference;
        businessDomainRef.ReferenceId = DataEstateHealthConstants.DEH_DOMAIN_ID;
        dataset.BusinessDomain = businessDomainRef;

        var dataProductRef = new ReferenceObjectWrapper(new JObject());
        dataProductRef.Type = ReferenceType.DataProductReference;
        dataProductRef.ReferenceId = context.dataProductId;
        dataset.DataProduct = dataProductRef;

        var dataAssetRef = new ReferenceObjectWrapper(new JObject());
        dataAssetRef.Type = ReferenceType.DataAssetReference;
        dataAssetRef.ReferenceId = context.dataAssetId;
        dataset.DataAsset = dataAssetRef;

        return dataset;
    }

    public static InputDatasetWrapper GetInputDataset(
        RuleAdapterContext context,
        DomainModelType domainModelType,
        bool isPrimary = false,
        string alias = null)
    {
        var domainModel = DomainModelUtils.GetDomainModel(domainModelType);

        var dataset = GetBasicDataset(context, domainModel.FolderPath, domainModel.Schema);

        // TODO Jar
        var inputDataset = new InputDatasetWrapper(new JObject()
        {
            { "dataset", dataset.JObject }
        });
        inputDataset.Alias = alias ?? domainModelType.ToString();
        inputDataset.Primary = isPrimary;

        return inputDataset;
    }
}
