// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

/// <summary>
/// Storage account repository interface
/// </summary>
public interface IJobDefinitionRepository
{
    /// <summary>
    /// Gets existing job list
    /// </summary>
    /// <param name="maxPerPage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// 
    Task<List<JobDefinitionModel>> GetBulk(int maxPerPage, string callbackName, JobExecutionStatus lastExecutionStatus, CancellationToken cancellationToken);
}
