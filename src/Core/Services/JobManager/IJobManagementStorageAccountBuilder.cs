// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.WindowsAzure.Storage;

/// <summary>
/// Job management storage account builder interface
/// </summary>
public interface IJobManagementStorageAccountBuilder
{
    /// <summary>
    /// Builds the job management storage account.
    /// </summary>
    public Task<CloudStorageAccount> Build();
}
