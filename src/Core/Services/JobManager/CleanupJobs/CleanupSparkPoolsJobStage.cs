// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Text.Json;
using System.Threading.Tasks;

internal class CleanupSparkPoolsJobStage : IJobCallbackStage
{
    private readonly IDataEstateHealthRequestLogger logger;

    private readonly ISynapseSparkExecutor synapseSparkExecutor;

    public CleanupSparkPoolsJobStage(
        IServiceScope scope,
        JobManagementClient client)
    {
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.synapseSparkExecutor = scope.ServiceProvider.GetService<ISynapseSparkExecutor>();
    }

    public string StageName => nameof(CleanupSparkPoolsJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        using (this.logger.LogElapsed($"Cleanup spark pools"))
        {
            try
            {
                var allSparkPools = await this.synapseSparkExecutor.ListSparkPools(CancellationToken.None).ConfigureAwait(false);

                var oldSparkPools = allSparkPools.Where(
                    p => p.CreatedOn < DateTime.UtcNow.AddDays(-1))
                    .ToList();

                this.logger.LogInformation($"Cleanup spark pools - All spark pool count: {allSparkPools.Count}. Old spark pool count: {oldSparkPools.Count}");
                this.logger.LogInformation($"Cleanup spark pools - Old spark pools: {string.Join(",", oldSparkPools.Select(p => p.Name))}");

                this.logger.LogTipInformation($"All spark pool count: {allSparkPools.Count}");

                foreach (var pool in oldSparkPools)
                {
                    var allJobs = await this.synapseSparkExecutor.ListJobs(pool.Name, CancellationToken.None).ConfigureAwait(false);
                    var runningJobs = allJobs?.Where(j => !SparkJobUtils.IsJobCompleted(j)).ToList();
                    this.logger.LogInformation($"Cleanup spark pools - Start to delete spark pool: {pool.Id}. All jobs count: {allJobs?.Count ?? 0}. Running jobs count: {runningJobs?.Count ?? 0}.");
                    if (runningJobs == null || runningJobs.Count == 0)
                    {
                        this.logger.LogInformation($"Cleanup spark pools - Deleting spark pool: {pool.Id}");
                        try
                        {
                            await this.synapseSparkExecutor.DeleteSparkPool(pool.Name, CancellationToken.None).ConfigureAwait(false);
                            this.logger.LogInformation($"Cleanup spark pools - Deleted spark pool: {pool.Id}");
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError($"Cleanup spark pools - Failed to delete spark pool: {pool.Id}. Error: {ex.Message}", ex);
                        }
                    }
                    else
                    {
                        this.logger.LogInformation($"Cleanup spark pools - Spark pool {pool.Id} has running jobs, skip deletion. Job count: {runningJobs?.Count ?? 0}.");
                        this.logger.LogInformation($"Cleanup spark pools - Running jobs status: \n{JsonSerializer.Serialize(runningJobs)}");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Cleanup spark pools - Failed to cleanup spark pools: {ex.Message}", ex);
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
