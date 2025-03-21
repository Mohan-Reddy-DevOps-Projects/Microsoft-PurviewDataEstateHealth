namespace DEH.Infrastructure.Repositories;

    using Azure.Monitor.Query;
    using Azure.Security.KeyVault.Secrets;
    using DEH.Domain.LogAnalytics;
    using LogAnalytics.Client;
    using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
    using Microsoft.Azure.Purview.DataEstateHealth.Core;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.DataGovernance.Common;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class DEHAnalyticsJobLogsRepository : IDEHAnalyticsJobLogsRepository
    {
        private readonly IKeyVaultAccessorService keyVaultAccessorService;
        private string? workspaceId;
        private string? workspaceKey;
        private readonly LogAnalyticsManager.LogAnalyticsQueryClient logsAnalyticsReader;
        private readonly AzureCredentialFactory credentialFactory;
        private readonly IDataEstateHealthRequestLogger logger;


        public DEHAnalyticsJobLogsRepository(IServiceProvider scope, IDataEstateHealthRequestLogger logger)
        {
            this.credentialFactory = scope.GetRequiredService<AzureCredentialFactory>();
            this.keyVaultAccessorService = scope.GetRequiredService<IKeyVaultAccessorService>();
            var keyVaultConfig = scope.GetService<IOptions<KeyVaultConfiguration>>();
            this.logger = logger;

            var task = Task.Run(async () =>
            {
                await this.GetWorkspaceCredentials().ConfigureAwait(false);
            });
            Task.WaitAll(task);
            // Log analytics reader
            LogAnalyticsManager manager = new LogAnalyticsManager(this.credentialFactory.CreateDefaultAzureCredential());
            this.logsAnalyticsReader = manager.WithWorkspace(this.workspaceId);
        }

        private async Task GetWorkspaceCredentials()
        {
            KeyVaultSecret workspaceId = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsWorkspaceId", default(CancellationToken)).ConfigureAwait(false);
            KeyVaultSecret workspaceKey = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsKey", default(CancellationToken)).ConfigureAwait(false);
            this.workspaceId = workspaceId.Value;
            this.workspaceKey = workspaceKey.Value;
        }

        public async Task<IReadOnlyList<DEHAnalyticsJobLogs>> GetDEHJobLogs(String query, TimeSpan timeSpan)
        {
            IReadOnlyList<DEHAnalyticsJobLogs> joblist = new List<DEHAnalyticsJobLogs>();

            try
            {
                var response = await this.logsAnalyticsReader.Query<DEHAnalyticsJobLogs>(query, timeSpan).ConfigureAwait(false);

                joblist = response.Value;

                if (joblist.Count == 0)
                {
                    this.logger.LogInformation($"No complete jobs found");
                }
                return joblist;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Failed to get Control Job Events from logs: {ex.Message}", ex);
                return joblist;
            }

        }
    }

