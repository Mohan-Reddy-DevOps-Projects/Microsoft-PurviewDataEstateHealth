// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

internal class MDQFailedJobStage : IJobCallbackStage
{
    private const int GetBulkSize = 1000;

    private readonly JobCallbackUtils<MDQFailedJobMetadata> jobCallbackUtils;

    private readonly MDQFailedJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger logger;

    private readonly IMDQFailedJobRepository mdqFailedJobRepsository;

    private readonly IDataHealthApiService dataHealthApiService;

    public MDQFailedJobStage(
        IServiceScope scope,
        MDQFailedJobMetadata metadata,
        JobCallbackUtils<MDQFailedJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.mdqFailedJobRepsository = scope.ServiceProvider.GetService<IMDQFailedJobRepository>();
        this.dataHealthApiService = scope.ServiceProvider.GetService<IDataHealthApiService>();
    }

    public string StageName => nameof(MDQFailedJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        this.logger.LogInformation("Start to execute MDQFailedJobStage.");
        var jobs = await this.mdqFailedJobRepsository.GetBulk(GetBulkSize, CancellationToken.None).ConfigureAwait(false);
        this.logger.LogTipInformation("Retrieved MDQ failed jobs", new JObject { { "jobCount", jobs.Count } });
        foreach (var job in jobs.OrderByDescending(item => item.CreatedAt).Take(100))
        {
            this.logger.LogTipInformation("Retry MDQ failed job", new JObject {
                { "jobId", job.DQJobId },
                { "retryCount", job.RetryCount },
                { "createdAt", job.CreatedAt },
                { "accountId", job.AccountId }
            });
            this.dataHealthApiService.TriggerMDQJobCallback(job, true);
        }
        this.metadata.MDQFailedJobProcessed = true;
        return this.jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Succeeded, "", DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
    }

    public bool IsStageComplete()
    {
        return this.metadata.MDQFailedJobProcessed;
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }
}
