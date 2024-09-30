// -----------------------------------------------------------------------
// <copyright file="LogAnalyticsToGenevaJobStage.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Security.KeyVault.Secrets;
using LogAnalytics.Client;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.LogAnalyticsToGenevaJob.DTOs;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.Common;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public class LogAnalyticsToGenevaJobStage : IJobCallbackStage
{
    private readonly LogAnalyticsToGenevaJobMetadata metadata;
    private readonly JobCallbackUtils<LogAnalyticsToGenevaJobMetadata> jobCallbackUtils;

    private readonly IServiceScope scope;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IOptionsMonitor<AuditServiceConfiguration> auditServiceConfiguration;
    private readonly IKeyVaultAccessorService keyVaultAccessorService;
    private readonly string keyVaultBaseURL;
    private readonly AzureCredentialFactory credentialFactory;
    private readonly LogAnalyticsManager.LogAnalyticsQueryClient logsAnalyticsReader;
    private readonly LogAnalyticsClient logsAnalyticsWriter;

    private string workspaceId;
    private string workspaceKey;

    internal LogAnalyticsToGenevaJobStage(
        IServiceScope scope,
        LogAnalyticsToGenevaJobMetadata metadata,
        JobCallbackUtils<LogAnalyticsToGenevaJobMetadata> jobCallbackUtils)
    {

        this.scope = scope;
        this.metadata = metadata;
        this.credentialFactory = scope.ServiceProvider.GetService<AzureCredentialFactory>();
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.auditServiceConfiguration = scope.ServiceProvider.GetService<IOptionsMonitor<AuditServiceConfiguration>>();
        this.jobCallbackUtils = jobCallbackUtils;
        this.keyVaultAccessorService = scope.ServiceProvider.GetService<IKeyVaultAccessorService>();
        var keyVaultConfig = scope.ServiceProvider.GetService<IOptions<KeyVaultConfiguration>>();
        this.keyVaultBaseURL = keyVaultConfig.Value.BaseUrl.ToString();

        using (this.logger.LogElapsed("LogAnalyticsToGenevaJobStage Constructor"))
        {
            // Get logAnalyticsWriter credentials
            var task = Task.Run(async () =>
            {
                await this.GetWorkspaceCredentials();
            });
            Task.WaitAll(task);

            this.logsAnalyticsWriter = new LogAnalyticsClient(workspaceId: this.workspaceId, sharedKey: this.workspaceKey);

            // Log analytics reader
            LogAnalyticsManager manager = new LogAnalyticsManager(this.credentialFactory.CreateDefaultAzureCredential());
            this.logsAnalyticsReader = manager.WithWorkspace(this.workspaceId);
            //this.logger.LogInformation("LogAnalyticsToGenevaJobStage Finished");
        }
    }

    public string StageName => nameof(LogAnalyticsToGenevaJobStage);

    public bool IsStagePreconditionMet()
    {
        return true;
    }

    public async Task<JobExecutionResult> Execute()
    {
        using (this.logger.LogElapsed($"{this.GetType().Name}: Starting LogAnalyticsToGeneva Job." + this.metadata.TenantId))
        {
            // job final state initializaation
            var jobStarted = DateTimeOffset.UtcNow;
            string finalExecutionStatusDetails = $"Job Started at {jobStarted.ToString()}";
            var finalExecutionStatus = JobExecutionStatus.Faulted;
            this.logger.LogInformation(finalExecutionStatusDetails);

            var (fromDate, toDate) = this.GetDateRange();
            finalExecutionStatusDetails += await this.ProcessJobLogsEvents<DEHJobLogEvent>("PDG_deh_job_logs.kql", fromDate, toDate);

            finalExecutionStatus = JobExecutionStatus.Succeeded;

            return this.jobCallbackUtils.GetExecutionResult(finalExecutionStatus, finalExecutionStatusDetails);
        }
    }

    private (DateTimeOffset fromDate, DateTimeOffset toDate) GetDateRange()
    {
        // Calculate the initial fromDate
        DateTimeOffset fromDate = DateTimeOffset.UtcNow.AddMinutes(-10);

        // Adjust fromDate based on LastPollTime
        if (fromDate.Subtract(this.metadata.LastPollTime).TotalDays <= 7)
        {
            fromDate = this.metadata.LastPollTime.AddMilliseconds(1);
        }

        // Calculate the two DateTimeOffset values for toDate
        var oneMinuteAgo = DateTimeOffset.UtcNow.AddMinutes(-1);
        var tenMinutesAfterFromDate = fromDate.AddMinutes(10);

        // Get the minimum DateTimeOffset for toDate
        var toDate = oneMinuteAgo < tenMinutesAfterFromDate ? oneMinuteAgo : tenMinutesAfterFromDate;
        this.logger.LogInformation($"Date Range of LogAnalyticsToGeneva KQL: {fromDate.ToString()} - {toDate.ToString()}");
        return (fromDate, toDate);
    }

    private async Task<string> ProcessJobLogsEvents<T>(string DEHJobLogsKQL, DateTimeOffset fromDate, DateTimeOffset toDate) where T : DEHJobLogEvent
    {
        string finalStatus = string.Empty;
        DateTimeOffset started = DateTimeOffset.UtcNow;
        using (this.logger.LogElapsed($"{this.GetType().Name}: {this.StageName} | Processing DEH Job Logs with KQL: {DEHJobLogsKQL}"))
        {
            try
            {
                this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Querying {DEHJobLogsKQL} from {fromDate} to {toDate}.");
                var jobLogEvents = await this.logsAnalyticsReader.Query<T>(await this.LoadKQL(DEHJobLogsKQL), fromDate, toDate);
                foreach (var jobLogEvent in jobLogEvents.Value.ToList())
                {
                    this.logger.LogInformation($"{JsonConvert.SerializeObject(jobLogEvent)}");
                }
            }
            catch (Exception ex)
            {
                // set the final details when faulted
                finalStatus += $" | KQL: {DEHJobLogsKQL} | Failed on {DateTimeOffset.Now.ToString()} | Duration {(DateTimeOffset.UtcNow - started).TotalSeconds} seconds. Reason: {ex.ToString()}";
                this.logger.LogError(finalStatus, ex);
            }
            this.metadata.LastPollTime = toDate;
            return finalStatus;
        }
    }

    private async Task GetWorkspaceCredentials()
    {

        KeyVaultSecret workspaceId = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsWorkspaceId", default(CancellationToken));
        KeyVaultSecret workspaceKey = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsKey", default(CancellationToken));

        this.workspaceId = workspaceId.Value;
        this.workspaceKey = workspaceKey.Value;
    }

    private async Task<string> LoadKQL(string queryName)
    {
        string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        string[] files = Directory.GetFiles(currentDirectory, queryName, SearchOption.AllDirectories);

        string filePath = "";
        if (files.Length > 0)
        {
            filePath = files[0];
        }

        this.logger.LogInformation("KQL Folder Path: " + filePath);

        string scriptText = "";
        using (this.logger.LogElapsed("Start Loading the script"))
        {
            this.logger.LogInformation(Path.Combine(currentDirectory, queryName));
            scriptText = await File.ReadAllTextAsync(Path.Combine(currentDirectory, queryName));
        }

        this.logger.LogInformation("KQL script: " + queryName + scriptText);
        return scriptText;
    }

    public bool IsStageComplete()
    {
        // we want this stage to run alwayas as it is was not completed
        return false;
    }
}
