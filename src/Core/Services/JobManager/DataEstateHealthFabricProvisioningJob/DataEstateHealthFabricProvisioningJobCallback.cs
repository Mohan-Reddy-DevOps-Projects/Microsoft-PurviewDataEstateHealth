// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

/// <inheritdoc />
/// <summary>
/// Provisions Fabric resources for an account
/// </summary>
[JobCallback(Name = nameof(DataEstateHealthFabricProvisioningJobCallback))]
internal class DataEstateHealthFabricProvisioningJobCallback : StagedWorkerJobCallback<DataEstateHealthFabricProvisioningJobMetadata>
{
    /// <inheritdoc />
    public DataEstateHealthFabricProvisioningJobCallback(IServiceScope scope)
        : base(scope)
    {
    }

    /// <inheritdoc />
    protected override string JobName => nameof(DataEstateHealthFabricProvisioningJobCallback);

    /// <inheritdoc />
    protected override void OnJobConfigure()
    {
        this.JobStages = new List<IJobCallbackStage>
        {
            new DataEstateHealthFabricWorkspaceProvisioningStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new DataEstateHealthFabricLakehouseProvisioningStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new DataEstateHealthFabricOTPipelineProvisioningStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new DataEstateHealthFabricEventStreamProvisioningStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new DataEstateHealthFabricDataWarehouseProvisioningStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new DataEstateHealthPersistProvisioningMetadataStage(this.Scope, this.Metadata, this.JobCallbackUtils)
        };
    }

    /// <inheritdoc />
    protected override Task<bool> IsJobPreconditionMet()
    {
        /***
         * validate if provisioning is already done for this account. 
         * we can either use metadata or call fabric to validate.
         * */

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    protected override Task TransitionToJobSucceeded()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task TransitionToJobFailed()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        return Task.CompletedTask;
    }
}
