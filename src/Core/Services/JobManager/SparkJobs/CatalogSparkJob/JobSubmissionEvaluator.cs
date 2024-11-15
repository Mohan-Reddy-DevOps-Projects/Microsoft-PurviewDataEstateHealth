namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

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

    internal JobSubmissionEvaluator(
        IServiceScope scope)
    {

        this.credentialFactory = scope.ServiceProvider.GetService<AzureCredentialFactory>();
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.keyVaultAccessorService = scope.ServiceProvider.GetService<IKeyVaultAccessorService>();
        var keyVaultConfig = scope.ServiceProvider.GetService<IOptions<KeyVaultConfiguration>>();
        this.keyVaultBaseURL = keyVaultConfig.Value.BaseUrl.ToString();
        this.dataHealthApiService = scope.ServiceProvider.GetService<IDataHealthApiService>();

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

    public async Task<bool> IsStorageSyncConfigured(string accountId, string tenantId)
    {
        try
        {
            var storageConfig = await this.GetStorageConfigSettings(accountId, tenantId);
            if (storageConfig == null || storageConfig.Status == "Disabled")
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

    public async Task<bool> IsDEHDQActive(string accountId)
    {
        // Define time window for query
        var fromDate = DateTimeOffset.UtcNow.AddMinutes(-25);
        var toDate = DateTimeOffset.UtcNow;

        // KQL query string to check for relevant job logs
        var kqlDehDq = $@"let Jobs = DQ_Job_Logs_CL
                            | where AccountId_g == ""{accountId}""
                            | join kind=inner DQ_Spark_Logs_CL on $left.JobId_g == $right.JobId_g
                            | extend MDQ = iff(Entity_s startswith ""mdq_"", ""MDQ"", ""DQ"")
                            | sort by TimeGenerated
                            | project AccountId = AccountId_g, JobId=JobId_g, JobTimestamp = TimeGenerated, MDQ;
                                Jobs 
                                | where MDQ == ""MDQ"" 
                                | limit 1
                                | union Jobs 
                                | where MDQ != ""DQ"" 
                                | limit 1";
        try
        {
            // Execute the query and fetch events
            var dehDQEvents = await this.logsAnalyticsReader.Query<DEHDQEvent>(kqlDehDq, fromDate, toDate);

            // If no events were found, return false
            if (dehDQEvents?.Value?.Count == 0)
            {
                this.logger.LogInformation($"No control or DQ job found within the last 25 hours for account: {accountId}");
                return false;
            }
            foreach (var dehDQEvent in dehDQEvents.Value)
            {
                this.logger.LogInformation($"Control or DQ Job found: {dehDQEvent}");
            }
            return true;
        }
        catch (Exception ex)
        {
            // Log error if the query or other process fails
            this.logger.LogError($"Failed to get DEH or DQ Events from logs for account {accountId}: {ex.Message}", ex);
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
