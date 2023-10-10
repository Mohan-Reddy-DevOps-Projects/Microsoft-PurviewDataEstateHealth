// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Newtonsoft.Json;

internal class HealthActionRepository : IHealthActionRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly string location;

    const string healthActionDataAllDomainsJson = @"[{""id"":""41fc9360-0d20-48d4-a5d4-be4632c46e56"",""name"":""Addclassification"",""description"":""Thisisadescriptionthatdescribestheaction."",""ownerContact"":{""displayName"":""CecilFolk"",""objectId"":""12345678-1234-1234-1234-123456789012""},""healthControlName"":""Completeness"",""healthControlState"":""Active"",""createdAt"":""2023-09-29T18:25:43.511Z"",""lastRefreshedAt"":""2023-09-29T18:25:43.511Z"",""targetDetails"":[{""targetKind"":""BusinessDomain"",""targetId"":""555b901c-fea5-4847-83c2-8516c4d31777"",""targetName"":""Finance"",""ownerContact"":{""displayName"":""CecilFolk"",""objectId"":""12345678-1234-1234-1234-123456789012""}}]},{""id"":""123522a2-45f2-b971-a5d4-3c9e1195efff"",""name"":""AddDQrule"",""description"":""Thisisadescriptionthatdescribestheaction."",""ownerContact"":{""displayName"":""JohnDoe"",""objectId"":""12345678-1234-1234-1234-123456789012""},""healthControlName"":""Completeness"",""healthControlState"":""Active"",""createdAt"":""2023-08-17T15:25:43.511Z"",""lastRefreshedAt"":""2023-08-31T15:25:43.511Z"",""targetDetails"":[{""targetKind"":""DataProduct"",""targetId"":""aee48875-a852-4018-a59a-886ef2e29cf9"",""targetName"":""SalesCommissions_Q4"",""ownerContact"":{""displayName"":""JohnDoe"",""objectId"":""12345678-1234-1234-1234-123456789012""}}]}]";

    const string healthActionDataOneDomainJson = @"{ ""id"": ""21fc9360-0d20-48d4-a5d4-be4632c46e86"", ""name"": ""Add description"", ""description"": ""This is a description that describes the action."", ""ownerContact"": { ""displayName"": ""Cecil Folk"", ""objectId"": ""12345678-1234-1234-1234-123456789012"" }, ""healthControlName"": ""Completeness"", ""healthControlState"": ""Active"", ""createdAt"": ""2023-08-17T15:00:00.000Z"", ""lastRefreshedAt"": ""2023-08-31T15:00:00.000Z"", ""targetDetails"": [ { ""targetKind"": ""BusinessDomain"", ""targetId"": ""69eb901c-fea5-4847-83c2-8516c4d31586"", ""targetName"": ""Marketing"", ""ownerContact"": { ""displayName"": ""Cecil Folk"", ""objectId"": ""12345678-1234-1234-1234-123456789012"" } }, { ""targetKind"": ""DataProduct"", ""targetId"": ""69eb901c-fea5-4847-83c2-8516c4d31586"", ""targetName"": ""Marketing Leads"", ""ownerContact"": { ""displayName"": ""Cecil Folk"", ""objectId"": ""12345678-1234-1234-1234-123456789012"" } }, { ""targetKind"": ""DataAsset"", ""targetId"": ""69eb901c-fea5-4847-83c2-8516c4d31586"", ""targetName"": ""Table A"", ""ownerContact"": { ""displayName"": ""Cecil Folk"", ""objectId"": ""12345678-1234-1234-1234-123456789012"" } } ] }";

    public HealthActionRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         string location = null)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.location = location;
    }

    public async Task<IHealthActionModel> GetSingle(HealthActionKey healthActionKey, CancellationToken cancellationToken)
    {
        // Read json file HealthActionDataOneDomain.json
        var healthActionEntity = JsonConvert.DeserializeObject<HealthActionEntity>(healthActionDataOneDomainJson);

        return await Task.FromResult(this.modelAdapterRegistry
            .AdapterFor<IHealthActionModel, HealthActionEntity>()
            .ToModel(healthActionEntity));
    }

    public async Task<IBatchResults<IHealthActionModel>> GetMultiple(
        CancellationToken cancellationToken,
        string continuationToken = null)
    { 
        var healthActionEntititiesList = JsonConvert.DeserializeObject<IList<HealthActionEntity>>(healthActionDataAllDomainsJson);

        var healthActionModelList = new List<IHealthActionModel>();
        foreach (var healthActionsEntity in healthActionEntititiesList)
        {
            healthActionModelList.Add(this.modelAdapterRegistry
                               .AdapterFor<IHealthActionModel, HealthActionEntity>()
                                              .ToModel(healthActionsEntity));
        }

        return await Task.FromResult(new BaseBatchResults<IHealthActionModel>
        {
            Results = healthActionModelList,
            ContinuationToken = null
        });
    }

    public IHealthActionRepository ByLocation(string location)
    {
        return new HealthActionRepository(
            this.modelAdapterRegistry,
            location);
    }
}
