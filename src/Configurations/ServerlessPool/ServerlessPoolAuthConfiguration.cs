// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Configurations for the Synapse Serverless Pool Authentication.
/// </summary>
public sealed class ServerlessPoolAuthConfiguration : AadAppConfiguration
{
    /// <summary>
    /// If the PowerBI authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; }
}
