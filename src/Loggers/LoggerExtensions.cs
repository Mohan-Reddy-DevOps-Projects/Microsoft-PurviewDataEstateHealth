// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Logger;

using System.Diagnostics;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Add logger services
/// </summary>
public static class LoggerExtensions
{
    private const string RootTraceID = "RootTraceId";    

    /// <summary>
    /// Add the logger services to IServiceCollection
    /// </summary>
    /// <param name="serviceCollection"></param>
    public static IServiceCollection AddLogger(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDataEstateHealthLogger, DataEstateHealthLogger>();
        serviceCollection.AddScoped<IDataEstateHealthRequestLogger, DataEstateHealthLogger>();

        return serviceCollection;
    }

    /// <summary>
    /// There is currently no reliable way to get the root ID of an Activity, especially if the activity originates
    /// from a remote source. This will get the root ID associated with a distributed transaction, so we can use it to look
    /// up all associated logs from that transaction.  This should not be needed once Geneva supports Otel Events
    /// </summary>
    /// <param name="activity"></param>
    /// <returns></returns>
    public static string GetRootId(this Activity activity)
    {
        if (activity == null)
        {
            return null;
        }
        
        // We want a specified root trace ID to override the default "RootID" property.
        string rootTraceId = (string)activity.GetTagItem(RootTraceID);
        
        // If a root ID is not specified, we want to first check to see if the current activity has a root ID. If it does not
        // it will either have a parent ID or it is the root activity.
        rootTraceId ??= activity.RootId ?? activity.ParentId ?? activity.TraceId.ToString();

        return rootTraceId;
    }

    /// <summary>
    /// Sets a root trace tag for tracking root IDs across distributed transactions.
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="rootTraceId"></param>
    public static void SetRootIdTag(this Activity activity, string rootTraceId)
    {
        activity.SetTag(RootTraceID, rootTraceId ?? string.Empty);
    }
}
