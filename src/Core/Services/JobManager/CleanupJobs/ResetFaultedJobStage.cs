// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Newtonsoft.Json;
using System.Threading.Tasks;

internal class ResetFaultedJobStage : IJobCallbackStage
{
    private const int GetBulkSize = 50;

    private readonly IDataEstateHealthRequestLogger logger;

    private readonly JobManagementClient jobManagementClient;

    private readonly IJobDefinitionRepository jobDefinitionRepository;

    private readonly IJobManager backgroundJobManager;


    public ResetFaultedJobStage(
        IServiceScope scope,
        JobManagementClient client)
    {
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.jobManagementClient = client;
        this.jobDefinitionRepository = scope.ServiceProvider.GetService<IJobDefinitionRepository>();
        this.backgroundJobManager = scope.ServiceProvider.GetService<IJobManager>();
    }

    public string StageName => nameof(ResetFaultedJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        using (this.logger.LogElapsed($"Reset faulted background job"))
        {
            try
            {
                var faultedJobs = await this.jobDefinitionRepository.GetBulk(GetBulkSize, nameof(CatalogSparkJobCallback), JobExecutionStatus.Faulted, CancellationToken.None).ConfigureAwait(false);

                this.logger.LogInformation($"Faulted job count: {faultedJobs.Count}");

                foreach (var jobDefinition in faultedJobs)
                {
                    this.logger.LogInformation($"Reset faulted job: {jobDefinition.JobId} started");
                    BackgroundJob faultedJob = await this.jobManagementClient.GetJob(jobDefinition.JobPartition, jobDefinition.JobId).ConfigureAwait(false);
                    if (faultedJob != null)
                    {
                        var jobMetadata = JsonConvert.DeserializeObject<DataPlaneSparkJobMetadata>(faultedJob.Metadata);
                        var accountModel = jobMetadata.AccountServiceModel;
                        this.logger.LogInformation($"Faulted job detail, jobId: {jobDefinition.JobId}, account: {accountModel.Name}, accountId: {accountModel.Id}");
                        await this.backgroundJobManager.ProvisionCatalogSparkJob(accountModel).ConfigureAwait(false);
                        this.logger.LogInformation($"Reset faulted job: {jobDefinition.JobId} completed.");
                    }
                    else
                    {
                        this.logger.LogCritical($"Try to reset faulted job, but job with id {jobDefinition.JobId} not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Failed to reset faulted job: {ex.Message}", ex);
            }
            return new JobExecutionResult { Status = JobExecutionStatus.Completed };
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
