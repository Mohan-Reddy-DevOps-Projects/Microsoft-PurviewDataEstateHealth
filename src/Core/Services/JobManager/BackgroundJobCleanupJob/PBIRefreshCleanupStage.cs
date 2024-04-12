// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class PBIRefreshCleanupStage : IJobCallbackStage
{
    private const string PBIRefreshPartitionId = "PBI-REFRESH-CALLBACK";

    private readonly IDataEstateHealthRequestLogger logger;

    private readonly JobManagementClient jobManagementClient;

    public PBIRefreshCleanupStage(
        IServiceScope scope,
        JobManagementClient client)
    {
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.jobManagementClient = client;
    }

    public string StageName => nameof(PBIRefreshCleanupStage);

    public async Task<JobExecutionResult> Execute()
    {
        this.logger.LogInformation("Start PBIRefresh cleanup stage.");
        var jobs = await this.jobManagementClient.GetJobs(PBIRefreshPartitionId).ConfigureAwait(false);
        this.logger.LogInformation($"Find {jobs.Length} PBIRefresh jobs waiting to be deleted.");
        jobs = jobs.Take(100).ToArray();
        await Task.WhenAll(jobs.Select(job => this.DeletePBIRefreshJob(job.JobId)).ToArray()).ConfigureAwait(false);
        return new JobExecutionResult { Status = JobExecutionStatus.Succeeded };
    }

    private async Task DeletePBIRefreshJob(string jobId)
    {
        try
        {
            await this.jobManagementClient.DeleteJob(PBIRefreshPartitionId, jobId).ConfigureAwait(false);
            this.logger.LogInformation($"Successfully delete PBIRefresh job: {jobId}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Fail to delete PBIRefresh job: {jobId}.", ex);
        }
    }

    public bool IsStageComplete()
    {
        return false;
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }
}
