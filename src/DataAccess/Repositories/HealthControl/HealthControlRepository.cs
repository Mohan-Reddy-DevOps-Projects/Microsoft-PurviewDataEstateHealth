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
using Microsoft.Purview.ArtifactStoreClient.Models;
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
            $"{nameof(HealthControlArtifactStoreEntity.ParentControlId).UncapitalizeFirstChar()}",
            Guid.Empty.ToString(),
            true);

        ArtifactStoreEntityQueryResult<HealthControlArtifactStoreEntity> entityList =
            await this.artifactStoreAccessorService.ListResourcesByCategoryAsync<HealthControlArtifactStoreEntity>(
                accountId: healthControlsKey.AccountId,
                entityType: DataEstateHealthEntityTypes.DataEstateHealthControl.ToString(),
                filterText: new List<string>() { parentControlIdFilter.Predicate },
                parameters: parentControlIdFilter.Parameters,
                continuationToken: continuationToken,
                byPassObligations: true);

        HealthControlSqlEntity healthControlSqlEntity = healthControlEntititiesList[0];
        HealthControlArtifactStoreEntity healthControlArtifactStoreEntity = entityList.Items.First().Properties;

        var healthControlEntity = new HealthControlEntity()
        {
            CurrentScore = healthControlSqlEntity.CurrentScore,
            LastRefreshedAt = healthControlSqlEntity.LastRefreshedAt,
            ControlStatus = healthControlArtifactStoreEntity.ControlStatus,
            ControlType = healthControlArtifactStoreEntity.ControlType,
            CreatedAt = healthControlArtifactStoreEntity.CreatedAt,
            Description = healthControlArtifactStoreEntity.Description,
            EndsAt = healthControlArtifactStoreEntity.EndsAt,
            HealthStatus = healthControlArtifactStoreEntity.HealthStatus,
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
        };

        //Grab the first one and add it for now - "Data governance score". 
        var healthControlModelList = new List<HealthControlModel>();

        healthControlModelList.Add(this.modelAdapterRegistry
                           .AdapterFor<HealthControlModel, HealthControlEntity>()
                                          .ToModel(healthControlEntity));

        return await Task.FromResult(new BaseBatchResults<HealthControlModel>
        {
            Results = healthControlModelList,
            ContinuationToken = null
        });
    }

    private static FilterModel CopyFilterModel(FilterSortModel<HealthControlModel> filterSortModel)
    {
        IDictionary<string, string> filterDictionary = filterSortModel?.FilterModel?.FilterDictionary ?? new Dictionary<string, string>();

        var filterModel = new FilterModel(new Dictionary<string, string>(filterDictionary));
        return filterModel;
    }
}
