// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Job Manager Configuration
/// </summary>
public class JobManagerConfiguration
{
    /// <summary>
    /// Background jobs storage account resource id
    /// </summary>
    public string BackgroundJobStorageResourceId { get; set; }
}
