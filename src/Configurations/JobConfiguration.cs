// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Job configuration.
/// </summary>
public class JobConfiguration
{
    /// <summary>
    /// Background jobs storage account name.
    /// </summary>
    public string StorageAccountName { get; set; }
}
