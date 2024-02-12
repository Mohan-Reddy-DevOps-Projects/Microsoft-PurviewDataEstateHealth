// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using OpenTelemetry;
using OpenTelemetry.Logs;

/// <summary>
/// Log processor that adds standard columns to each log event
/// </summary>
public class AddEnvStateLogProcessor : BaseProcessor<LogRecord>
{

    EnvironmentConfiguration environmentConfiguration;

    /// <summary>
    ///  Constructor for log processor
    /// </summary>
    /// <param name="environmentConfiguration"></param>
    public AddEnvStateLogProcessor(EnvironmentConfiguration environmentConfiguration)
    {
        this.environmentConfiguration = environmentConfiguration;
    }

    /// <inheritdoc/>
    public override void OnEnd(LogRecord data)
    {
        // Custom state information
        var logState = new Dictionary<string, object>
        {
            ["env.name"] = this.environmentConfiguration.Environment.ToString(),
            ["RoleLocation"] = this.environmentConfiguration.Location,
            ["cloud.role"] = Environment.GetEnvironmentVariable("CONTAINER_APP_NAME"),
            ["cloud.roleInstance"] = Environment.GetEnvironmentVariable("CONTAINER_APP_REVISION"),
            ["cloud.roleVer"] = Environment.GetEnvironmentVariable("BUILD_VERSION"),
            ["genevaLogTableId"] = Environment.GetEnvironmentVariable("GENEVA_ERROR_LOG_TABLE_ID"),
            ["env_ex_msg"] = data?.Exception?.Message,
            ["env_ex_type"] = data?.Exception?.GetType().FullName,
            ["env_ex_stack"] = data?.Exception?.ToString(),
        };

        var state = new List<KeyValuePair<string, object>>();
        if (data.Attributes != null)
        {
            state = [.. data.Attributes];
        }

        data.Attributes = new ReadOnlyCollectionBuilder<KeyValuePair<string, object>>(state.Concat(logState))
            .ToReadOnlyCollection();

        base.OnEnd(data);
    }
}
