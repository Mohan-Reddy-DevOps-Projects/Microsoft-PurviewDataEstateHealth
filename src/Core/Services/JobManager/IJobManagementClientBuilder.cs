// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

/// <summary>
/// JobManagementClientBuilder interface
/// </summary>
public interface IJobManagementClientBuilder
{
    /// <summary>
    /// Builds the JobManagementClient
    /// </summary>
    public JobManagementClient Build();
}
