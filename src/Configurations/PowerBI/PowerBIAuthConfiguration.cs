// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Configurations for the PowerBI Authentication.
/// </summary>
public class PowerBIAuthConfiguration : AadAppConfiguration
{
    /// <summary>
    /// If the PowerBI authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; }
}
