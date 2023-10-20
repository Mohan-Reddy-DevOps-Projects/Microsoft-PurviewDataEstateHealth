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

    const string healthControlDataJson = @"[{""id"":""5e209b70-cf6e-46cb-945a-572c3e2e847d"",""kind"":""DataGovernance"",""name"":""Datagovernancescore"",""isParentControl"":""true"",""controlType"":""System"",""ownerContact"":{""displayName"":""CecilFolk"",""objectId"":""8374a147-4c90-46e2-bfa0-4525eb036530""},""currentScore"":80,""targetScore"":82,""scoreUnit"":""%"",""healthStatus"":""Fair"",""healthStatusColor"":""#0055FF"",""controlStatus"":""Active"",""createdAt"":""2023-09-29T18:25:43.511Z"",""startsAt"":""2023-09-29T18:25:43.511Z"",""endsAt"":""2023-09-29T18:25:43.511Z"",""trendUrl"":""/trends/healthControls/5e209b70-cf6e-46cb-945a-572c3e2e847d"",""businessDomainsListLink"":""/businessDomains?healthcontrol=5e209b70-cf6e-46cb-945a-572c3e2e847d"",""description"":""This overall control helps determine current data governance maturity posture based on the Cloud Data Management Council's framework.""},{""id"":""819433af-db62-4514-81f6-0a51381696fd"",""kind"":""DataQuality"",""name"":""Dataquality"",""isParentControl"":""true"",""controlType"":""System"",""ownerContact"":{""displayName"":""CecilFolk"",""objectId"":""8374a147-4c90-46e2-bfa0-4525eb036530"",},""currentScore"":60,""targetScore"":75,""scoreUnit"":""%"",""healthStatus"":""Fair"",""healthStatusColor"":""#0055FF"",""controlStatus"":""Active"",""createdAt"":""2023-09-29T18:25:43.511Z"",""startsAt"":""2023-09-29T18:25:43.511Z"",""endsAt"":""2023-09-29T18:25:43.511Z"",""trendUrl"":""/trends/healthControls/819433af-db62-4514-81f6-0a51381696fd"",""businessDomainsListLink"":""/businessDomains?healthControl=819433af-db62-4514-81f6-0a51381696fd"",""description"":""This overall control helps determine current data governance maturity posture based on the Cloud Data Management Council's framework.""}]";

    public HealthControlRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         string location = null)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.location = location;
    }

    public async Task<IBatchResults<IHealthControlModel<HealthControlProperties>>> GetMultiple(
        CancellationToken cancellationToken,
        string continuationToken = null)
    {
         var healthControlEntititiesList = JsonConvert.DeserializeObject<IList<HealthControlEntity>>(healthControlDataJson);

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
            location);
    }
}
