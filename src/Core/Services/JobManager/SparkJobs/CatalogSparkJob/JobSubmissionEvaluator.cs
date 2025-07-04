﻿namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using LogAnalytics.Client;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.SparkJobs.CatalogSparkJob.DTOs;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.JobManagerModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.Common;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;

/// <summary>
/// Response object for DEH Job Log events
/// </summary>
public class DehJobLogEvent
{
    public string AccountId { get; set; }
    public string JobName { get; set; }
    public string Status { get; set; }

    public override string ToString()
    {
        return $"AccountId: {AccountId}, JobName: {JobName}, Status: {Status}";
    }
}

internal class JobSubmissionEvaluator
{
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IKeyVaultAccessorService keyVaultAccessorService;
    private readonly string keyVaultBaseURL;
    private readonly AzureCredentialFactory credentialFactory;
    private readonly LogAnalyticsManager.LogAnalyticsQueryClient logsAnalyticsReader;
    private readonly LogAnalyticsClient logsAnalyticsWriter;
    private string workspaceId;
    private string workspaceKey;
    private readonly IDataHealthApiService dataHealthApiService;
    private readonly IAccountExposureControlConfigProvider exposureControl;

    internal JobSubmissionEvaluator(
        IServiceScope scope)
    {

        this.credentialFactory = scope.ServiceProvider.GetService<AzureCredentialFactory>();
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.keyVaultAccessorService = scope.ServiceProvider.GetService<IKeyVaultAccessorService>();
        var keyVaultConfig = scope.ServiceProvider.GetService<IOptions<KeyVaultConfiguration>>();
        this.keyVaultBaseURL = keyVaultConfig.Value.BaseUrl.ToString();
        this.dataHealthApiService = scope.ServiceProvider.GetService<IDataHealthApiService>();
        this.exposureControl = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();

        using (this.logger.LogElapsed("JobSubmissionEvaluator Constructor"))
        {
            // Get logAnalyticsWriter credentials
            var task = Task.Run(async () =>
            {
                await this.GetWorkspaceCredentials();
            });
            Task.WaitAll(task);

            this.logsAnalyticsWriter = new LogAnalyticsClient(workspaceId: this.workspaceId, sharedKey: this.workspaceKey);

            // Log analytics reader
            var manager = new LogAnalyticsManager(this.credentialFactory.CreateDefaultAzureCredential());
            this.logsAnalyticsReader = manager.WithWorkspace(this.workspaceId);
            //this.logger.LogInformation("MetersToBillingJobStage Finished");
        }
    }

    private async Task<StorageConfiguration> GetStorageConfigSettings(string accountId, string tenantId)
    {
        //Get Fabric Configuration
        StorageConfiguration returnConfig = new StorageConfiguration();
        returnConfig = await this.dataHealthApiService.GetStorageConfigSettings(accountId, tenantId);
        return returnConfig;
    }

    public async Task<bool> IsStorageSyncEnabledAndConfigured(string accountId, string tenantId)
    {
        try
        {
            var storageConfig = await this.GetStorageConfigSettings(accountId, tenantId);
            if (storageConfig == null || storageConfig.Status == "Disabled")
            {
                this.logger.LogInformation($"StorageSync is not configured or is disabled for account: {accountId}");
                return false;
            }
            this.logger.LogInformation($"StorageSync is configured and enabled for account: {accountId}");
            return true;
        }
        catch (Exception ex)
        {
            // Log error if the query or other process fails
            this.logger.LogError($"Failed to get storage config for account {accountId}: {ex.Message}", ex);
            return true; // Consider returning `true` as a default to ensure job submission attempt
        }
    }

    public async Task<bool> IsStorageSyncConfigured(string accountId, string tenantId)
    {
        try
        {
            var storageConfig = await this.GetStorageConfigSettings(accountId, tenantId);
            if (storageConfig == null)
            {
                this.logger.LogInformation($"StorageSync is not configured for account: {accountId}");
                return false;
            }
            this.logger.LogInformation($"StorageSync is configured for account: {accountId}");
            return true;
        }
        catch (Exception ex)
        {
            // Log error if the query or other process fails
            this.logger.LogError($"Failed to get storage config for account {accountId}: {ex.Message}", ex);
            return true; // Consider returning `true` as a default to ensure job submission attempt
        }
    }

    public async Task<bool> IsDEHRanInLast24Hours(string accountId)
    {
        // Define time window for query
        var fromDate = DateTimeOffset.UtcNow.AddHours(-24).AddMinutes(-30);
        var toDate = DateTimeOffset.UtcNow;

        // Determine which table to use based on exposure control flag
        string tableName = "DEH_Job_Logs_CL";
        try
        {
            if (this.exposureControl?.IsEnableControlRedesignBillingImprovements(accountId, string.Empty, string.Empty) == true)
            {
                tableName = "DEH_Job_Logs_V2_CL";
            }
        }
        catch (Exception ex)
        {
            this.logger.LogWarning($"Failed to check exposure control flag, using default table: {ex.Message}");
        }

        // KQL query string to check for relevant job logs
        string kqlDeh = $"""
                         {tableName}
                         | project AccountId=AccountId_g, JobName=JobName_s, Status=JobStatus_s
                         | where AccountId == "{accountId}" and Status == "Completed" and JobName == "DomainModel"
                         | limit 1
                         """;
        try
        {
            // Execute the query and fetch events
            var dehJobLogEvents = await this.logsAnalyticsReader.Query<DehJobLogEvent>(kqlDeh, fromDate, toDate);

            // If no events were found, return false
            if (dehJobLogEvents?.Value?.Count == 0)
            {
                this.logger.LogInformation($"No job found within the last 24 hours for account: {accountId}");
                return false;
            }
            foreach (var dehJobLogEvent in dehJobLogEvents.Value)
            {
                this.logger.LogInformation($"Job found: {dehJobLogEvent}");
            }
            return true;
        }
        catch (Exception ex)
        {
            // Log error if the query or other process fails
            this.logger.LogError($"Failed to get Job Events from logs for account {accountId}: {ex.Message}", ex);
            return true; // Consider returning `true` as a default to ensure job submission attempt
        }
    }

    private async Task GetWorkspaceCredentials()
    {
        var workspaceId = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsWorkspaceId", default);
        var workspaceKey = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsKey", default);

        this.workspaceId = workspaceId.Value;
        this.workspaceKey = workspaceKey.Value;
    }
}
