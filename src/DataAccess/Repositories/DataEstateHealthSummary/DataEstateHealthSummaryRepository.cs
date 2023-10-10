// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Newtonsoft.Json;

internal class DataEstateHealthSummaryRepository : IDataEstateHealthSummaryRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly string location;

    const string summaryDataJson = @"{ ""healthActionsSummary"": { ""totalOpenActionsCount"": 32, ""totalCompletedActionsCount"": 105, ""totalDismissedActionsCount"": 15, ""healthActionsTrendLink"": ""/s/actions/domains"", ""healthActionsLastRefreshDate"": ""2023-09-29T18:25:43.511Z"" }, ""businessDomainsSummary"": { ""totalBusinessDomainsCount"": 4, ""businessDomainsList"": [ { ""businessDomainName"": ""Finance"", ""businessDomainId"": ""8cae4ce6-85e0-4816-9579-d40840109e0b"" }, { ""businessDomainName"": ""Legal"", ""businessDomainId"": "" 10247fce-26ab-45f8-a82d-af37def43669 "" }, { ""businessDomainName"": ""Marketing"", ""businessDomainId"": "" ef35b13c-40bd-4aa8-87da-58eb1100af6f "" }, { ""businessDomainName"": ""IT"", ""businessDomainId"": "" bed33e7a-2d2f-4843-b3d0-2cf9fee4b7e9 "" } ], ""businessDomainsTrendLink"": ""/s/businessDomains/domains"", ""businessDomainsLastRefreshDate"": ""2023-09-29T18:25:43.511Z"" }, ""dataProductsSummary"": { ""totalDataProductsCount"": 48, ""dataProductsTrendLink"": ""/s/dataProducts/domains"", ""dataProductsLastRefreshDate"": ""2023-09-29T18:25:43.511Z"" }, ""dataAssetsSummary"": { ""totalCuratedDataAssetsCount"": 264, ""totalCuratableDataAssetsCount"": 1645, ""totalNonCuratableDataAssetsCount"": 3416, ""dataAssetsTrendLink"": ""/s/dataAssets/domains"", ""dataAssetsLastRefreshDate"": ""2023-09-29T18:25:43.511Z"" }, ""healthReportsSummary"": { ""totalReportsCount"": 10, ""activeReportsCount"": 5, ""draftReportsCount"": 3 } }";

    public DataEstateHealthSummaryRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         string location = null)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.location = location;
    }

    public async Task<IDataEstateHealthSummaryModel> GetSingle(SummaryKey summaryKey, CancellationToken cancellationToken)
    {
        var dataEstateHealthSummaryEntity = JsonConvert.DeserializeObject<DataEstateHealthSummaryEntity>(summaryDataJson);

        return await Task.FromResult(this.modelAdapterRegistry
            .AdapterFor<IDataEstateHealthSummaryModel, DataEstateHealthSummaryEntity>()
            .ToModel(dataEstateHealthSummaryEntity));
    }

    public IDataEstateHealthSummaryRepository ByLocation(string location)
    {
        return new DataEstateHealthSummaryRepository(
            this.modelAdapterRegistry,
            location);
    }
}
