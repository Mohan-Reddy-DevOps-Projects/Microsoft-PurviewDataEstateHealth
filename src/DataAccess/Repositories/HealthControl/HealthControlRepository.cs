// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.Purview.ArtifactStoreClient;
using Newtonsoft.Json;
using Microsoft.Azure.Purview.DataAccess.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

internal class HealthControlRepository : IHealthControlRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    const string healthControlChildDataJson = @"[{""id"":""08b367a6-9020-4edd-9278-6c7fc4cac630"",""kind"":""DataGovernance"",""name"":""Metadatacompleteness"",""isCompositeControl"":""true"",""controlType"":""System"",""ownerContact"":{""displayName"":""CecilFolk"",""objectId"":""8374a147-4c90-46e2-bfa0-4525eb036530""},""currentScore"":80,""targetScore"":82,""scoreUnit"":""%"",""healthStatus"":""Healthy"",""healthStatusColor"":""#0055FF"",""controlStatus"":""Active"",""createdAt"":""2023-09-29T18:25:43.511Z"",""startsAt"":""2023-09-29T18:25:43.511Z"",""endsAt"":""2023-09-29T18:25:43.511Z"",""trendUrl"":""/trends/healthControls/5e209b70-cf6e-46cb-945a-572c3e2e847d"",""businessDomainsListLink"":""/businessDomains?healthcontrol=5e209b70-cf6e-46cb-945a-572c3e2e847d"",""description"":""This overall control helps determine current data governance maturity posture based on the Cloud Data Management Council's framework."",},{""id"":""977746fb-a4fa-4e56-9400-1bdc0c0ae766"",""kind"":""DataGovernance"",""name"":""Quality"",""isCompositeControl"":""true"",""controlType"":""System"",""ownerContact"":{""displayName"":""CecilFolk"",""objectId"":""8374a147-4c90-46e2-bfa0-4525eb036530"",},""currentScore"":60,""targetScore"":75,""scoreUnit"":""%"",""healthStatus"":""Healthy"",""healthStatusColor"":""#0055FF"",""controlStatus"":""Active"",""createdAt"":""2023-09-29T18:25:43.511Z"",""startsAt"":""2023-09-29T18:25:43.511Z"",""endsAt"":""2023-09-29T18:25:43.511Z"",""trendUrl"":""/trends/healthControls/819433af-db62-4514-81f6-0a51381696fd"",""businessDomainsListLink"":""/businessDomains?healthControl=819433af-db62-4514-81f6-0a51381696fd"",""description"":""This overall control helps determine current data governance maturity posture based on the Cloud Data Management Council's framework.""},{""id"":""ee07fbf3-4763-461c-80d7-c18eb653a9ac"",""kind"":""DataGovernance"",""name"":""Usage"",""isCompositeControl"":""false"",""controlType"":""System"",""ownerContact"":{""displayName"":""CecilFolk"",""objectId"":""8374a147-4c90-46e2-bfa0-4525eb036530"",},""currentScore"":60,""targetScore"":75,""scoreUnit"":""%"",""healthStatus"":""Unhealthy"",""healthStatusColor"":""#0055F"",""controlStatus"":""Active"",""createdAt"":""2023-09-29T18:25:43.511Z"",""startsAt"":""2023-09-29T18:25:43.511Z"",""endsAt"":""2023-09-29T18:25:43.511Z"",""trendUrl"":""/trends/healthControls/819433af-db62-4514-81f6-0a51381696fd"",""businessDomainsListLink"":""businessDomains?healthControl=819433af-db62-4514-81f6-0a51381696fd"",""description"":""This overall control helps determine current data governance maturity posture based on the Cloud Data Management Council's framework.""}]";

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IArtifactStoreAccessorService artifactStoreAccessorService;

    public HealthControlRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         IProcessingStorageManager processingStorageManager,
         IServerlessQueryRequestBuilder queryRequestBuilder,
         IServerlessQueryExecutor queryExecutor,
         IArtifactStoreAccessorServiceBuilder artifactStoreAccessorServiceBuilder)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.processingStorageManager = processingStorageManager;
        this.queryRequestBuilder = queryRequestBuilder;
        this.queryExecutor = queryExecutor;
        this.artifactStoreAccessorService = artifactStoreAccessorServiceBuilder.Build();
    }

    public async Task<IBatchResults<HealthControlModel>> GetMultiple(
         HealthControlKey healthControlKey,
         CancellationToken cancellationToken,
         string continuationToken = null)
    {
        var healthControlSqlEntitiesList = JsonConvert.DeserializeObject<IList<HealthControlSqlEntity>>(healthControlChildDataJson);

        var healthControlsModelList = new List<HealthControlModel>();
        foreach (var healthControlsEntity in healthControlSqlEntitiesList)
        {
            healthControlsModelList.Add(this.modelAdapterRegistry
                               .AdapterFor<HealthControlModel, HealthControlSqlEntity>()
                                              .ToModel(healthControlsEntity));
        }

        return await Task.FromResult(new BaseBatchResults<HealthControlModel>
        {
            Results = healthControlsModelList,
            ContinuationToken = null
        });
    }

    public async Task<IBatchResults<HealthControlModel>> GetMultiple(
        HealthControlsKey healthControlsKey,
        CancellationToken cancellationToken,
        string continuationToken = null)
    {
        string containerPath = await this.processingStorageManager.ConstructContainerPath(healthControlsKey.CatalogId.ToString(), healthControlsKey.AccountId, cancellationToken);

        HealthControlQuery query = this.queryRequestBuilder.Build<HealthControlRecord>(containerPath) as HealthControlQuery;

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        IList<HealthControlSqlEntity> healthControlEntititiesList = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        var parentControlIdFilter = new IndexedPropertyEqualityFilter(
            $"{nameof(HealthControlArtifactStoreEntity.HealthControlKind).UncapitalizeFirstChar()}",
            HealthControlKind.DataGovernance.ToString(),
            true);

        ArtifactStoreEntityQueryResult<HealthControlArtifactStoreEntity> entityList =
            await this.artifactStoreAccessorService.ListResourcesByCategoryAsync<HealthControlArtifactStoreEntity>(
                accountId: healthControlsKey.AccountId,
                entityType: DataEstateHealthEntityTypes.DataEstateHealthControl.ToString(),
                filterText: new List<string>() { parentControlIdFilter.Predicate },
                parameters: parentControlIdFilter.Parameters,
                continuationToken: continuationToken,
                byPassObligations: true);

        HealthControlSqlEntity healthControlSqlEntity = null;

        if (healthControlEntititiesList.Count == 0)
        {
            healthControlSqlEntity = new HealthControlSqlEntity()
            {
                AccessEntitlementScore = -1,
                AuthoritativeDataSourceScore = -1,
                CatalogingScore = -1,
                ClassificationScore = -1,
                CurrentScore = -1,
                DataConsumptionPurposeScore = -1,
                DataQualityScore = -1,
                MetadataCompletenessScore = -1,
                OwnershipScore = -1,
                QualityScore = -1,
                UseScore = -1,
                LastRefreshedAt = DateTime.MinValue
            };
        }
        else
        {
            healthControlSqlEntity = healthControlEntititiesList[0];
        }

        List<HealthControlEntity> healthControlEntities = this.CreateHealthControlEntities(entityList.Items.Select(x => x.Properties).ToList(), healthControlSqlEntity);

        var healthControlModelList = new List<HealthControlModel>();

        foreach(HealthControlEntity healthControlEntity in healthControlEntities)
        {
            healthControlModelList.Add(this.modelAdapterRegistry
                   .AdapterFor<HealthControlModel, HealthControlEntity>()
                                  .ToModel(healthControlEntity));
        }

        return new BaseBatchResults<HealthControlModel>
        {
            Results = healthControlModelList,
            ContinuationToken = null
        };
    }

    private List<HealthControlEntity> CreateHealthControlEntities(List<HealthControlArtifactStoreEntity> healthControlArtifactStoreEntities, HealthControlSqlEntity healthControlSqlEntity)
    {
        List<HealthControlEntity> healthControlEntities = new();

        Dictionary<OOTBControlTypes, double> controlNameScoreDictionary = this.CreateControlNameScoreDictionary(healthControlSqlEntity);

        foreach (HealthControlArtifactStoreEntity healthControlArtifactStoreEntity in healthControlArtifactStoreEntities)
        {
            double currentScore = controlNameScoreDictionary[OOTBControlTypes.Parse(healthControlArtifactStoreEntity.Name)];

            healthControlEntities.Add(new HealthControlEntity()
            {
                CurrentScore = currentScore,
                LastRefreshedAt = healthControlSqlEntity.LastRefreshedAt,
                ControlStatus = currentScore < 0 ? HealthResourceStatus.NotStarted : healthControlArtifactStoreEntity.ControlStatus,
                ControlType = healthControlArtifactStoreEntity.ControlType,
                CreatedAt = healthControlArtifactStoreEntity.CreatedAt,
                Description = healthControlArtifactStoreEntity.Description,
                EndsAt = healthControlArtifactStoreEntity.EndsAt,
                HealthStatus = this.GenerateHealthStatus(currentScore),
                IsCompositeControl = healthControlArtifactStoreEntity.IsCompositeControl,
                ModifiedAt = healthControlArtifactStoreEntity.ModifiedAt,
                Name = healthControlArtifactStoreEntity.Name,
                ObjectId = healthControlArtifactStoreEntity.ObjectId,
                OwnerContact = healthControlArtifactStoreEntity.OwnerContact,
                ParentControlId = healthControlArtifactStoreEntity.ParentControlId,
                ScoreUnit = healthControlArtifactStoreEntity.ScoreUnit,
                StartsAt = healthControlArtifactStoreEntity.StartsAt,
                TargetScore = healthControlArtifactStoreEntity.TargetScore,
                TrendUrl = healthControlArtifactStoreEntity.TrendUrl,
                HealthControlKind = healthControlArtifactStoreEntity.HealthControlKind,
                Version = null,
            });
        }

        return healthControlEntities;
    }

    private string GenerateHealthStatus(double score)
    {
        if (score >= 80)
        {
            return "Healthy";
        }
        else if (score >= 50)
        {
            return "Fair";
        }
        else
        {
            return "Not healthy";
        }
    }

    private Dictionary<OOTBControlTypes, double> CreateControlNameScoreDictionary(HealthControlSqlEntity healthControlSqlEntity)
    {
        return new Dictionary<OOTBControlTypes, double>()
        {
            {OOTBControlTypes.DataGovernanceScore, healthControlSqlEntity.CurrentScore },
            {OOTBControlTypes.AccessEntitlement, healthControlSqlEntity.AccessEntitlementScore },
            {OOTBControlTypes.MetadataCompleteness, healthControlSqlEntity.MetadataCompletenessScore },
            {OOTBControlTypes.DataQuality, healthControlSqlEntity.DataQualityScore },
            {OOTBControlTypes.Classification, healthControlSqlEntity.ClassificationScore },
            {OOTBControlTypes.DataConsumptionPurpose, healthControlSqlEntity.DataConsumptionPurposeScore },
            {OOTBControlTypes.AuthoritativeDataSource, healthControlSqlEntity.AuthoritativeDataSourceScore },
            {OOTBControlTypes.Quality, healthControlSqlEntity.QualityScore },
            {OOTBControlTypes.Use, healthControlSqlEntity.UseScore },
            {OOTBControlTypes.Cataloging, healthControlSqlEntity.CatalogingScore },
            {OOTBControlTypes.Ownership, healthControlSqlEntity.OwnershipScore },
        };
    }

    private static FilterModel CopyFilterModel(FilterSortModel<HealthControlModel> filterSortModel)
    {
        IDictionary<string, string> filterDictionary = filterSortModel?.FilterModel?.FilterDictionary ?? new Dictionary<string, string>();

        var filterModel = new FilterModel(new Dictionary<string, string>(filterDictionary));
        return filterModel;
    }
}
