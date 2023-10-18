// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

internal class DataEstateHealthPersistProvisioningMetadataStage : IJobCallbackStage
{
    private readonly DataEstateHealthFabricProvisioningJobMetadata metadata;

    private readonly JobCallbackUtils<DataEstateHealthFabricProvisioningJobMetadata> jobCallbackUtils;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    public DataEstateHealthPersistProvisioningMetadataStage(
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
        return true;
    }

    /// <inheritdoc />
    public Task<JobExecutionResult> Execute()
    {
        /***
         * Persist All metadata like workspaceid, lakehouseid, pipelineid, eventstreamid, datawarehouse id for an account in DB.
         * and set the flag this.metadata.IsMetadataPersisted = true;
         */

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool IsStageComplete()
    {
        return this.metadata.IsMetadataPersisted;
    }

}
