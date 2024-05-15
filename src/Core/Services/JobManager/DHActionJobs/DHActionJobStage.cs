// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class DHActionJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<ActionCleanUpJobMetadata> jobCallbackUtils;

    private readonly ActionCleanUpJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger logger;

    private readonly IDataHealthApiService dataHealthApiService;

    public DHActionJobStage(
        IServiceScope scope,
        ActionCleanUpJobMetadata metadata,
        JobCallbackUtils<ActionCleanUpJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.dataHealthApiService = scope.ServiceProvider.GetService<IDataHealthApiService>();
    }

    public string StageName => nameof(DHActionJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        this.logger.LogInformation("Start to execute DHActionJobStage.");
        var isSuccess = await this.dataHealthApiService.CleanUpActionsJobCallback(this.metadata.AccountServiceModel, CancellationToken.None);
        this.metadata.ActionCleanUpCompleted = isSuccess;
        var jobStageStatus = isSuccess ? JobExecutionStatus.Succeeded : JobExecutionStatus.Failed;
        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, "", DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
    }

    public bool IsStageComplete()
    {
        return this.metadata.ActionCleanUpCompleted;
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }
}
