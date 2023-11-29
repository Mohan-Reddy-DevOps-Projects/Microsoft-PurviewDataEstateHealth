// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Newtonsoft.Json;

internal class HealthControlRepository : IHealthControlRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly string location;

    const string healthControlChildDataJson = @"[{""id"":""08b367a6-9020-4edd-9278-6c7fc4cac630"",""kind"":""DataGovernance"",""name"":""Metadatacompleteness"",""isCompositeControl"":""true"",""controlType"":""System"",""ownerContact"":{""displayName"":""CecilFolk"",""objectId"":""8374a147-4c90-46e2-bfa0-4525eb036530""},""currentScore"":80,""targetScore"":82,""scoreUnit"":""%"",""healthStatus"":""Healthy"",""healthStatusColor"":""#0055FF"",""controlStatus"":""Active"",""createdAt"":""2023-09-29T18:25:43.511Z"",""startsAt"":""2023-09-29T18:25:43.511Z"",""endsAt"":""2023-09-29T18:25:43.511Z"",""trendUrl"":""/trends/healthControls/5e209b70-cf6e-46cb-945a-572c3e2e847d"",""businessDomainsListLink"":""/businessDomains?healthcontrol=5e209b70-cf6e-46cb-945a-572c3e2e847d"",""description"":""This overall control helps determine current data governance maturity posture based on the Cloud Data Management Council's framework."",},{""id"":""977746fb-a4fa-4e56-9400-1bdc0c0ae766"",""kind"":""DataGovernance"",""name"":""Quality"",""isCompositeControl"":""true"",""controlType"":""System"",""ownerContact"":{""displayName"":""CecilFolk"",""objectId"":""8374a147-4c90-46e2-bfa0-4525eb036530"",},""currentScore"":60,""targetScore"":75,""scoreUnit"":""%"",""healthStatus"":""Healthy"",""healthStatusColor"":""#0055FF"",""controlStatus"":""Active"",""createdAt"":""2023-09-29T18:25:43.511Z"",""startsAt"":""2023-09-29T18:25:43.511Z"",""endsAt"":""2023-09-29T18:25:43.511Z"",""trendUrl"":""/trends/healthControls/819433af-db62-4514-81f6-0a51381696fd"",""businessDomainsListLink"":""/businessDomains?healthControl=819433af-db62-4514-81f6-0a51381696fd"",""description"":""This overall control helps determine current data governance maturity posture based on the Cloud Data Management Council's framework.""},{""id"":""ee07fbf3-4763-461c-80d7-c18eb653a9ac"",""kind"":""DataGovernance"",""name"":""Usage"",""isCompositeControl"":""false"",""controlType"":""System"",""ownerContact"":{""displayName"":""CecilFolk"",""objectId"":""8374a147-4c90-46e2-bfa0-4525eb036530"",},""currentScore"":60,""targetScore"":75,""scoreUnit"":""%"",""healthStatus"":""Unhealthy"",""healthStatusColor"":""#0055F"",""controlStatus"":""Active"",""createdAt"":""2023-09-29T18:25:43.511Z"",""startsAt"":""2023-09-29T18:25:43.511Z"",""endsAt"":""2023-09-29T18:25:43.511Z"",""trendUrl"":""/trends/healthControls/819433af-db62-4514-81f6-0a51381696fd"",""businessDomainsListLink"":""businessDomains?healthControl=819433af-db62-4514-81f6-0a51381696fd"",""description"":""This overall control helps determine current data governance maturity posture based on the Cloud Data Management Council's framework.""}]";

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    private readonly IServerlessQueryExecutor queryExecutor;

    public HealthControlRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         IProcessingStorageManager processingStorageManager,
         IServerlessQueryRequestBuilder queryRequestBuilder,
         IServerlessQueryExecutor queryExecutor,
        string location = null)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.processingStorageManager = processingStorageManager;
        this.queryRequestBuilder = queryRequestBuilder;
        this.queryExecutor = queryExecutor;
        this.location = location;
    }

    public async Task<IBatchResults<IHealthControlModel<HealthControlProperties>>> GetMultiple(
         HealthControlKey healthControlKey,
         CancellationToken cancellationToken,
         string continuationToken = null)
    {
        var healthControlEntitiesList = JsonConvert.DeserializeObject<IList<HealthControlEntity>>(healthControlChildDataJson);

        var healthControlsModelList = new List<IHealthControlModel<HealthControlProperties>>();
        foreach (var healthControlsEntity in healthControlEntitiesList)
        {
            healthControlsModelList.Add(this.modelAdapterRegistry
                               .AdapterFor<IHealthControlModel<HealthControlProperties>, HealthControlEntity>()
                                              .ToModel(healthControlsEntity));
        }

        return await Task.FromResult(new BaseBatchResults<IHealthControlModel<HealthControlProperties>>
        {
            Results = healthControlsModelList,
            ContinuationToken = null
        });
    }

    public async Task<IBatchResults<IHealthControlModel<HealthControlProperties>>> GetMultiple(
        HealthControlsKey healthControlsKey,
        CancellationToken cancellationToken,
        string continuationToken = null)
    {
        string containerPath = await this.processingStorageManager.ConstructContainerPath(healthControlsKey.CatalogId.ToString(), healthControlsKey.AccountId, cancellationToken);

        HealthControlQuery query = this.queryRequestBuilder.Build<HealthControlRecord>(containerPath) as HealthControlQuery;

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        IList<DataGovernanceHealthControlEntity> healthControlEntititiesList = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        var healthControlModelList = new List<IHealthControlModel<HealthControlProperties>>();
        foreach (var healthControlEntity in healthControlEntititiesList)
        {
            healthControlModelList.Add(this.modelAdapterRegistry
                               .AdapterFor<IHealthControlModel<HealthControlProperties>, HealthControlEntity>()
                                              .ToModel(healthControlEntity));
        }

        return await Task.FromResult(new BaseBatchResults<IHealthControlModel<HealthControlProperties>>
        {
            Results = healthControlModelList,
            ContinuationToken = null
        });
    }

    public IHealthControlRepository ByLocation(string location)
    {
        return new HealthControlRepository(
            this.modelAdapterRegistry,
            this.processingStorageManager,
            this.queryRequestBuilder,
            this.queryExecutor,
            location);
    }
}
