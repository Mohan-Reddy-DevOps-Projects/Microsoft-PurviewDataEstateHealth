// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Newtonsoft.Json;

internal class HealthScoreRepository : IHealthScoreRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly string location;

    const string healthScoreDataAllDomainsJson = @"[{""scoreKind"":""DataGovernance"",""name"":""Governance score"",""description"":""Review your governance maturity posture based on the Cloud Data Management Council's framework."",""reportId"":""01f57eba-87e9-4f1c-b340-b8812ae21cff"",""actualValue"":90.4,""targetValue"":80,""measureUnit"":""%"",""performanceIndicatorRules"":[{""ruleOrder"":0,""minValue"":0,""maxValue"":40,""displayText"":""Unhealthy"",""defaultColor"":""#D13438""},{""ruleOrder"":1,""minValue"":41,""maxValue"":80,""displayText"":""Medium"",""defaultColor"":""#E8CC78""},{""ruleOrder"":2,""minValue"":81,""maxValue"":100,""displayText"":""Healthy"",""defaultColor"":""#80D091""}]},{""scoreKind"":""DataQuality"",""name"":""Dataqualityscore"",""description"":""Evaluate the quality of your data products and data assets to determine the irusefulness towards business goals."",""reportId"":""eae23f9f-4806-46eb-82a7-e5b879a6ea68"",""actualValue"":70,""targetValue"":30,""measureUnit"":""%"",""performanceIndicatorRules"":[{""ruleOrder"":0,""minValue"":0,""maxValue"":40,""displayText"":""Unhealthy"",""defaultColor"":""#D13438""},{""ruleOrder"":1,""minValue"":41,""maxValue"":80,""displayText"":""Medium"",""defaultColor"":""#E8CC78""},{""ruleOrder"":2,""minValue"":81,""maxValue"":100,""displayText"":""Healthy"",""defaultColor"":""#80D091""}]},{""scoreKind"":""DataCuration"",""name"":""Data curation"",""description"":""Review curated and uncurated data."",""reportId"":""eae24f9f-4806-46eb-82a7-e5b879a6ea68"",""totalCuratedCount"":158,""totalCanBeCuratedCount"":781,""totalCannotBeCuratedCount"":1714}]";

    const string healthScoreDataOneDomainJson = @"[{""scoreKind"":""DataGovernance"",""name"":""Governance score"",""description"":""Review your governance maturity posture based on the Cloud Data Management Council's framework."",""reportId"":""01f57eba-87e9-4f1c-b340-b8812ae21cff"",""actualValue"":35,""targetValue"":24,""measureUnit"":""%"",""performanceIndicatorRules"":[{""ruleOrder"":0,""minValue"":0,""maxValue"":40,""displayText"":""Unhealthy"",""defaultColor"":""#D13438""},{""ruleOrder"":1,""minValue"":41,""maxValue"":80,""displayText"":""Medium"",""defaultColor"":""#E8CC78""},{""ruleOrder"":2,""minValue"":81,""maxValue"":100,""displayText"":""Healthy"",""defaultColor"":""#80D091""}]},{""scoreKind"":""DataQuality"",""name"":""Dataqualityscore"",""description"":""Evaluate the quality of your data products and data assets to determine the irusefulness towards business goals."",""reportId"":""eae23f9f-4806-46eb-82a7-e5b879a6ea68"",""actualValue"":70,""targetValue"":30,""measureUnit"":""%"",""performanceIndicatorRules"":[{""ruleOrder"":0,""minValue"":0,""maxValue"":40,""displayText"":""Unhealthy"",""defaultColor"":""#D13438""},{""ruleOrder"":1,""minValue"":41,""maxValue"":80,""displayText"":""Medium"",""defaultColor"":""#E8CC78""},{""ruleOrder"":2,""minValue"":81,""maxValue"":100,""displayText"":""Healthy"",""defaultColor"":""#80D091""}]},{""scoreKind"":""DataCuration"",""name"":""Data curation"",""description"":""Review curated and uncurated data."",""reportId"":""eae24f9f-4806-46eb-82a7-e5b879a6ea68"",""totalCuratedCount"":158,""totalCanBeCuratedCount"":781,""totalCannotBeCuratedCount"":1714}]";

    public HealthScoreRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         string location = null)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.location = location;
    }

    public async Task<IBatchResults<IHealthScoreModel<HealthScoreProperties>>> GetMultiple(
         HealthScoreKey healthScoreKey,
         CancellationToken cancellationToken,
         string continuationToken = null)
    {
        var healthScoreEntitiesList = JsonConvert.DeserializeObject<IList<HealthScoreEntity>>(healthScoreDataOneDomainJson);

        var healthScoreModelList = new List<IHealthScoreModel<HealthScoreProperties>>();
        foreach (var healthScoresEntity in healthScoreEntitiesList)
        {
            healthScoreModelList.Add(this.modelAdapterRegistry
                               .AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScoreEntity>()
                                              .ToModel(healthScoresEntity));
        }

        return await Task.FromResult(new BaseBatchResults<IHealthScoreModel<HealthScoreProperties>>
        {
            Results = healthScoreModelList,
            ContinuationToken = null
        });
    }

    public async Task<IBatchResults<IHealthScoreModel<HealthScoreProperties>>> GetMultiple(
        CancellationToken cancellationToken,
        string continuationToken = null)
    {
        var healthScoreEntititiesList = JsonConvert.DeserializeObject<IList<HealthScoreEntity>>(healthScoreDataAllDomainsJson);

        var healthScoreModelList = new List<IHealthScoreModel<HealthScoreProperties>>();
        foreach (var healthScoresEntity in healthScoreEntititiesList)
        {
            healthScoreModelList.Add(this.modelAdapterRegistry
                               .AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScoreEntity>()
                                              .ToModel(healthScoresEntity));
        }

        return await Task.FromResult(new BaseBatchResults<IHealthScoreModel<HealthScoreProperties>>
        {
            Results = healthScoreModelList,
            ContinuationToken = null
        });
    }

    public IHealthScoreRepository ByLocation(string location)
    {
        return new HealthScoreRepository(
            this.modelAdapterRegistry,
            location);
    }
}
