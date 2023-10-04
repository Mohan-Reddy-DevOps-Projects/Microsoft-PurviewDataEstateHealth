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

    public DataEstateHealthSummaryRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         string location = null)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.location = location;
    }

    public async Task<IDataEstateHealthSummaryModel> GetSingle(SummaryKey summaryKey)
    {
        // Read json file SummaryData.json
        string jsonString = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"MockData\SummaryData.json"));
        var dataEstateHealthSummaryEntity = JsonConvert.DeserializeObject<DataEstateHealthSummaryEntity>(jsonString);

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
