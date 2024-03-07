namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetProjectAsItem;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
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
        var inputDatasets = this.GetDataProductInputDatasets().ToList();
        inputDatasets.AddRange(convertedResult.inputDatasetsFromJoin);
        observer.InputDatasets = inputDatasets;

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

    private IEnumerable<InputDatasetWrapper> GetDataProductInputDatasets()
    {
        var inputDataset = new InputDatasetWrapper(new JObject()
        {
            // TODO why set not work
            { "dataset", this.GetDataProductDataset().JObject }
        });
        inputDataset.Alias = "DataProduct";
        inputDataset.Primary = true;

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
        dataset.DatasourceFQN = this.context.endpoint + "/";
        dataset.CompressionCodec = "snappy";

        var datasetLocation = new DatasetGen2FileLocationWrapper(new JObject()
        {
            { DynamicEntityWrapper.keyType, DatasetGen2FileLocationWrapper.EntityType },
            { DynamicEntityWrapper.keyTypeProperties, new JObject() }
        });
        datasetLocation.FileSystem = this.context.containerName;
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
        dataProductRef.ReferenceId = this.context.dataProductId;
        dataset.DataProduct = dataProductRef;

        var dataAssetRef = new ReferenceObjectWrapper(new JObject());
        dataAssetRef.Type = ReferenceType.DataAssetReference;
        dataAssetRef.ReferenceId = this.context.dataAssetId;
        dataset.DataAsset = dataAssetRef;

        return dataset;
    }

    private IEnumerable<DatasetSchemaItemWrapper> GetDataProductSchema()
    {
        return SchemaUtils.GenerateSchemaFromDefinition([
            ["DataProductID", "String"],
            ["DataProductDisplayName", "String"],
            ["DataProductDescription", "String"],
            ["UseCases", "String"],
            ["Endorsed", "Boolean"],
        ]);
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
