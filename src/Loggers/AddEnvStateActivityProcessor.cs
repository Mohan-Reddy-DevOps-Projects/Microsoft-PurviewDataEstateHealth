// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using System.Diagnostics;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using OpenTelemetry;

/// <summary>
/// Activity processor that adds standard columns to each activity
/// </summary>
public class AddEnvStateActivityProcessor : BaseProcessor<Activity>
{
    EnvironmentConfiguration environmentConfiguration;
    /// <summary>
    /// Constructor for activity processor
    /// </summary>
    /// <param name="environmentConfiguration"></param>
    public AddEnvStateActivityProcessor(EnvironmentConfiguration environmentConfiguration)
    {
        this.environmentConfiguration = environmentConfiguration;
    }

    /// <inheritdoc/>
    public override void OnEnd(Activity data)
    {
        // Custom state information
        data.AddTag("env.name", environmentConfiguration.Environment.ToString());
        data.AddTag("RoleLocation", environmentConfiguration.Location);
        data.AddTag("cloud.role", Environment.GetEnvironmentVariable("CONTAINER_APP_NAME"));
        data.AddTag("cloud.roleInstance", Environment.GetEnvironmentVariable("CONTAINER_APP_REVISION"));
        data.AddTag("cloud.roleVer", Environment.GetEnvironmentVariable("BUILD_VERSION"));
        data.AddTag("ServiceID", Environment.GetEnvironmentVariable("SERVICE_ID"));

        base.OnEnd(data);
    }
}
