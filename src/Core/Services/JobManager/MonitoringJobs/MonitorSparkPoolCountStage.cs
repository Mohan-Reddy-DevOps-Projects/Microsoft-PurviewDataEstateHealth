namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.MonitoringJobs;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

internal class MonitorSparkPoolCountStage : IJobCallbackStage
{
    private readonly IDataEstateHealthRequestLogger logger;

    private readonly ISynapseSparkExecutor synapseSparkExecutor;

    public MonitorSparkPoolCountStage(
        IServiceScope scope,
        JobManagementClient client)
    {
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.synapseSparkExecutor = scope.ServiceProvider.GetService<ISynapseSparkExecutor>();
    }

    public string StageName => nameof(MonitorSparkPoolCountStage);

    public async Task<JobExecutionResult> Execute()
    {
        using (this.logger.LogElapsed($"Monitoring spark pools"))
        {
            try
            {
                var allSparkPools = await this.synapseSparkExecutor.ListSparkPools(CancellationToken.None).ConfigureAwait(false);

                var oldSparkPools = allSparkPools.Where(
                    p => p.CreatedOn < DateTime.UtcNow.AddDays(-1))
                    .ToList();

                var tipInfo = new JObject
                {
                    {"allCount", allSparkPools.Count },
                    {"oldCount", oldSparkPools.Count }
                };

                this.logger.LogTipInformation($"Spark pools count", tipInfo);
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Monitoring spark pools - Failed to get spark pools: {ex.Message}", ex);
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
