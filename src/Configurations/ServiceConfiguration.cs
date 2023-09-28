// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Configurations for the Web Server.
/// </summary>
public class ServiceConfiguration
{
    /// <summary>
    /// Listen port for the Api Service. 
    /// </summary>
    public int ApiServicePort { get; set; }

    /// <summary>
    /// Listen port for the Worker Service. 
    /// </summary>
    public int WorkerServicePort { get; set; }
}
