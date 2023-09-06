// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

/// <summary>
/// Interface for job callback stage that will be part of background job
/// </summary>
internal interface IJobCallbackStage
{
    /// <summary>
    /// Stage name.
    /// </summary>
    string StageName { get; }

    /// <summary>
    /// Initialize any models needed by the stage 
    /// </summary>
    /// <returns></returns>
    Task InitializeStage()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Check for pre-condition(s) this stage is dependent on.
    /// </summary>
    /// <returns></returns>
    bool IsStagePreconditionMet();

    /// <summary>
    /// Executes the stage logic
    /// </summary>
    /// <returns></returns>
    Task<JobExecutionResult> Execute();

    /// <summary>
    /// Check if stage execution is completed.
    /// </summary>
    /// <returns></returns>
    bool IsStageComplete();
}
