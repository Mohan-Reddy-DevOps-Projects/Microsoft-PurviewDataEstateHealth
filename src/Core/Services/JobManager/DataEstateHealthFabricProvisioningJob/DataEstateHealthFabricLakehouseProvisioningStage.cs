// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.Azure.Purview.DataEstateHealth.Logger;

internal class DataEstateHealthFabricLakehouseProvisioningStage : IJobCallbackStage
{
    private readonly DataEstateHealthFabricProvisioningJobMetadata metadata;

    private readonly JobCallbackUtils<DataEstateHealthFabricProvisioningJobMetadata> jobCallbackUtils;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    public DataEstateHealthFabricLakehouseProvisioningStage(
        IServiceScope scope,
        DataEstateHealthFabricProvisioningJobMetadata metadata,
        JobCallbackUtils<DataEstateHealthFabricProvisioningJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    }

    public string StageName => nameof(DataEstateHealthFabricWorkspaceProvisioningStage);

    /// <inheritdoc />
    public bool IsStagePreconditionMet()
    {
        /***
        * validate if workspace is already provisioned for this account we can either use metadata or call fabric to validate.
        * if yes then set this.metadata.IsLakehouseProvisioned = true;
        * else continue execution
        * */
        return true;
    }

    /// <inheritdoc />
    public Task<JobExecutionResult> Execute()
    {
        /***
         * 1. call fabric api to provision lakehouse.
         * 2. if workspace is successfully created then set this.metadata.LakehouseId value returned by fabric and
         *      this.metadata.IsLakehouseProvisioned = true;
         *      else throw exception and fault the job
         * 3. postpone for 10 secs;
         */

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool IsStageComplete()
    {
        return this.metadata.IsLakehouseProvisioned;
    }

}
