// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using System.Collections.Generic;

/// <summary>
/// Configurations related to geneva service
/// </summary>
public class GenevaConfiguration
{
    /// <summary>
    /// Name of the metrics or hot path account
    /// </summary>
    public string MetricsAccount { get; set; }

    /// <summary>
    /// Namespace under the metrics account to emit values to
    /// </summary>
    public string MetricsNamespace { get; set; }

    /// <summary>
    /// Geneva service host name on AKS
    /// </summary>
    public string GenevaServicesHost { get; set; }

    /// <summary>
    /// Geneva service host port on AKS
    /// </summary>
    public int GenevaServicesPort { get; set; }

    /// <summary>
    /// Geneva fluentD port used for audit logging
    /// </summary>
    public int GenevaFluentdPort { get; set; }


    /// <summary>
    /// Geneva container app name used for audit logging
    /// </summary>
    public string GenevaContainerAppName { get; set; }

    /// <summary>
    /// Default dimensions to add to the metric being logged
    /// </summary>
    public Dictionary<string, string> DefaultDimensions { get; set; }
}
