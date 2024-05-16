// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading;

internal class JobDefinitionRepository : IJobDefinitionRepository
{
    private readonly ITableStorageClient<JobDefinitionTableConfiguration> tableStorageClient;
    private readonly JobDefinitionAdapter converter = new JobDefinitionAdapter();
    private readonly JobDefinitionTableConfiguration tableConfiguration;

    public JobDefinitionRepository(ITableStorageClient<JobDefinitionTableConfiguration> tableStorageClient, IOptions<JobDefinitionTableConfiguration> tableConfiguration)
    {
        this.tableStorageClient = tableStorageClient;
        this.tableConfiguration = tableConfiguration.Value;
    }

    /// <inheritdoc/>
    public async Task<List<JobDefinitionModel>> GetBulk(int maxPerPage, string callbackName, JobExecutionStatus lastExecutionStatus, CancellationToken cancellationToken)
    {
        var jobs = await this.tableStorageClient.GetEntitiesAsync<JobDefinitionEntity>(
            this.tableConfiguration.TableName,
            x => x.Callback == callbackName && x.LastExecutionStatus == lastExecutionStatus.ToString(),
            maxPerPage,
            cancellationToken).ConfigureAwait(false);
        return jobs.Select(job => this.converter.ToModel(job)).ToList();
    }
}
