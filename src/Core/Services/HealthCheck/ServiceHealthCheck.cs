// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Supports readiness probes to ensure service is not sent requests
/// before it is fully initialized.
/// </summary>
public sealed class ServiceHealthCheck : IHealthCheck
{
    /// <summary>
    /// Set to true when the service is initialized
    /// </summary>
    public bool Initialized { get; set; }

    /// <summary>
    /// Check service health when probe is invoked
    /// </summary>
    /// <param name="context">Health context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this.Initialized ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
    }
}

