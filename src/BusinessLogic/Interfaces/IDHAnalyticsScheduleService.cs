
namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Interfaces;

using DEH.Domain.LogAnalytics;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDHAnalyticsScheduleService
{
    Task<DHControlGlobalSchedulePayloadWrapper> CreateOrUpdateAnalyticsGlobalScheduleAsync(DHControlGlobalSchedulePayloadWrapper entity);
    Task<DHControlGlobalSchedulePayloadWrapper> GetAnalyticsGlobalScheduleAsync();
    Task<IReadOnlyList<DEHAnalyticsJobLogs>> GetDEHJobLogs(string accountId);

}

