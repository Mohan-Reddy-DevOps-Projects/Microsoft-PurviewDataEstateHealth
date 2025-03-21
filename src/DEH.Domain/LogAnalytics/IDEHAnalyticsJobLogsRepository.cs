namespace DEH.Domain.LogAnalytics;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDEHAnalyticsJobLogsRepository
{
    public Task<IReadOnlyList<DEHAnalyticsJobLogs>> GetDEHJobLogs(string query, TimeSpan timeSpan);

}
