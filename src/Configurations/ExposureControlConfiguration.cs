// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using Microsoft.Purview.DataGovernance.Reporting.Common;

/// <summary>
/// The configuration for exposure control service.
/// </summary>
public class ExposureControlConfiguration : BaseCertificateConfiguration, IExposureControlConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether the exposure control service is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the name of the environment.
    /// </summary>
    public string EnvironmentName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use ESTSR.
    /// </summary>
    public bool UseEstsr { get; set; }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the logging level.
    /// </summary>
    public string LoggingLevel { get; set; }

    /// <summary>
    /// Gets or sets the interval, in minutes, at which the cache is refreshed.
    /// </summary>
    public int CacheRefreshIntervalInMinutes { get; set; }
}
