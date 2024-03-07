namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetProjectAsItem;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public abstract class JoinAdapter
{
    protected RuleAdapterContext context;

    public JoinAdapter(RuleAdapterContext context)
    {
        this.context = context;
    }

    public abstract JoinAdapterResult Adapt();

    // Returns a partial location
    protected DatasetGen2FileLocationWrapper GetBasicGen2FileLocation()
    {
        var datasetLocation = new DatasetGen2FileLocationWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, DatasetGen2FileLocationWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });
        datasetLocation.FileSystem = this.context.containerName;
        return datasetLocation;
    }

    // Returns a partial dataset
    protected DeltaDatasetWrapper GetBasicDataset(string folderPath, List<DatasetSchemaItemWrapper> schema)
    {
        var dataset = new DeltaDatasetWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, DeltaDatasetWrapper.entityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });
        dataset.ProjectAs = Array.Empty<DatasetProjectAsItemWrapper>();
        dataset.DatasourceFQN = this.context.endpoint + "/";
        dataset.CompressionCodec = "snappy";

        var datasetLocation = this.GetBasicGen2FileLocation();
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
        dataProductRef.ReferenceId = this.context.dataProductId;
        dataset.DataProduct = dataProductRef;

        var dataAssetRef = new ReferenceObjectWrapper(new JObject());
        dataAssetRef.Type = ReferenceType.DataAssetReference;
        dataAssetRef.ReferenceId = this.context.dataAssetId;
        dataset.DataAsset = dataAssetRef;

        return dataset;
    }
}
