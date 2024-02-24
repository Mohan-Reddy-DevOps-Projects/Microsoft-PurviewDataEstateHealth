// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using Microsoft.Purview.DataGovernance.Common;
using Microsoft.Purview.DataGovernance.Reporting.Common;

/// <summary>
/// Represents the configuration settings for Azure Storage related to the internal management of the service.
/// </summary>
public class AuxStorageConfiguration : AuthConfiguration, IAuxStorageConfiguration
{
    /// <summary>
    /// Gets or sets the name of the Azure Storage account.
    /// </summary>
    public string AccountName { get; set; }

    /// <summary>
    /// Gets or sets the name of the Azure Resource Group containing the Storage account.
    /// </summary>
    public string ResourceGroup { get; set; }

    /// <summary>
    /// Gets or sets the ID of the Azure subscription containing the Storage account and resource group.
    /// </summary>
    public string SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the endpoint suffix for the Azure Storage account.
    /// </summary>
    public string EndpointSuffix { get; set; }

    /// <summary>
    /// Gets or sets the resource URL for Blob Storage in Azure.
    /// </summary>
    public string BlobStorageResource { get; set; }
}
