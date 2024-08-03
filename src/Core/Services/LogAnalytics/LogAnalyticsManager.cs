// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------
namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure;
using global::Azure.Identity;
using global::Azure.Monitor.Query;

public class LogAnalyticsManager
{

    public LogsQueryClient Client { get; set; }

    public LogAnalyticsManager(DefaultAzureCredential credential)
    {
        this.Client = new LogsQueryClient(credential);
    }

    public LogAnalyticsQueryClient WithWorkspace(string workspaceId)
    {
        return new LogAnalyticsQueryClient(this, workspaceId);
    }

    public class LogAnalyticsQueryClient
    {
        public string WorkspaceId { get; set; }
        public LogAnalyticsManager LogAnalyticsManager { get; set; }

        internal LogAnalyticsQueryClient(LogAnalyticsManager logAnalyticsManager, string workspaceId)
        {
            this.WorkspaceId = workspaceId;

            this.LogAnalyticsManager = logAnalyticsManager;
        }

        public Task<Response<IReadOnlyList<T>>> Query<T>(string query, DateTimeOffset from, DateTimeOffset to, bool allowPartialErrors = true) where T : class
        {
            var options = new LogsQueryOptions()
            {
                AllowPartialErrors = allowPartialErrors,
            };

            var result = this.LogAnalyticsManager.Client.QueryWorkspaceAsync<T>(this.WorkspaceId, query, new QueryTimeRange(from, to), options);

            return result;
        }

        public Task<Response<IReadOnlyList<T>>> Query<T>(string query, TimeSpan timeSpan) where T : class
        {
            var result = this.LogAnalyticsManager.Client.QueryWorkspaceAsync<T>(this.WorkspaceId, query, timeSpan);

            return result;
        }

    }
}