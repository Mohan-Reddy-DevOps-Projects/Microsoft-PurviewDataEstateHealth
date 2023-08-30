// -----------------------------------------------------------------------
//  <copyright file="JobManagerConfiguration.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

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
