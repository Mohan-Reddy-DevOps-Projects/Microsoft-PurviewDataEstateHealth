// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Options;
using System.Threading;

internal class MDQFailedJobRepository : IMDQFailedJobRepository
{
    private readonly ITableStorageClient<MDQFailedJobTableConfiguration> tableStorageClient;
    private readonly MDQJobEntityAdapter converter = new MDQJobEntityAdapter();
    private readonly MDQFailedJobTableConfiguration tableConfiguration;

    public MDQFailedJobRepository(ITableStorageClient<MDQFailedJobTableConfiguration> tableStorageClient, IOptions<MDQFailedJobTableConfiguration> tableConfiguration)
    {
        this.tableStorageClient = tableStorageClient;
        this.tableConfiguration = tableConfiguration.Value;
    }

    /// <inheritdoc/>
    public async Task<MDQJobModel> Create(MDQJobModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        MDQFailedJobEntity entity = this.converter.ToEntity(model, model.AccountId.ToString());
        await this.tableStorageClient.AddEntityAsync(this.tableConfiguration.TableName, entity, cancellationToken);

        return this.converter.ToModel(entity);
    }

    /// <inheritdoc/>
    public async Task Delete(Guid rowKey, CancellationToken cancellationToken)
    {
        await this.tableStorageClient.DeleteEntityAsync(this.tableConfiguration.TableName, MDQJobEntityAdapter.DefaultPartitionKey, rowKey.ToString(), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MDQJobModel> GetSingle(Guid rowKey, CancellationToken cancellationToken)
    {
        MDQFailedJobEntity existingStorageEntity = await this.tableStorageClient.GetEntityIfExistsAsync<MDQFailedJobEntity>(this.tableConfiguration.TableName, MDQJobEntityAdapter.DefaultPartitionKey, rowKey.ToString(), cancellationToken);

        return this.converter.ToModel(existingStorageEntity);
    }

    /// <inheritdoc/>
    public async Task<List<MDQJobModel>> GetBulk(int maxPerPage, CancellationToken cancellationToken)
    {
        var jobs = await this.tableStorageClient.GetEntitiesAsync<MDQFailedJobEntity>(
            this.tableConfiguration.TableName,
            x => x.PartitionKey == MDQJobEntityAdapter.DefaultPartitionKey,
            maxPerPage,
            cancellationToken).ConfigureAwait(false);
        return jobs.Select(job => this.converter.ToModel(job)).ToList();
    }

    /// <inheritdoc/>
    public async Task Update(MDQJobModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        MDQFailedJobEntity entity = this.converter.ToEntity(model, model.DQJobId.ToString());
        await this.tableStorageClient.UpdateEntityAsync(this.tableConfiguration.TableName, entity, cancellationToken);
    }
}
