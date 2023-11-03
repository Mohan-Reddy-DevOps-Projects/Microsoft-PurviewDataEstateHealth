// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Configurations for the Synapse Spark Pool.
/// </summary>
public sealed class SynapseSparkConfiguration
{
    /// <summary>
    /// Gets or sets the Azure region for the authentication.
    /// This property is used to specify the geographical region of the Azure data center.
    /// </summary>
    public string AzureRegion { get; set; }

    /// <summary>
    /// Gets or sets the resource group.
    /// </summary>
    public string ResourceGroup { get; set; }

    /// <summary>
    /// Gets or sets the subscription id.
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the Synapse workspace name.
    /// </summary>
    public string Workspace { get; set; }
}

