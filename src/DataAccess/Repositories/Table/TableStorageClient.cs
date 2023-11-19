// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Azure;
using global::Azure.Data.Tables;
using global::Azure.Identity;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;

internal sealed class TableStorageClient<TConfig> : ITableStorageClient<TConfig> where TConfig : StorageTableConfiguration, new()
{
    private const string Tag = nameof(TableStorageClient<TConfig>);

    private readonly TConfig tableConfiguration;
    private readonly TableServiceClient serviceClient;
    private readonly Func<string, string> getRequestType = (string requestType) => $"{Tag}_{requestType}";

    public TableStorageClient(IOptions<TConfig> tableConfig, AzureCredentialFactory credentialFactory)
    {
        this.tableConfiguration = tableConfig.Value;
        Uri authorityHost = new(this.tableConfiguration.Authority);
        DefaultAzureCredential tokenCredential = credentialFactory.CreateDefaultAzureCredential(authorityHost);
        this.serviceClient = new TableServiceClient(new Uri(this.tableConfiguration.TableServiceUri), tokenCredential);
    }

    public async Task AddEntityAsync<T>(
        string tableName,
        T entity,
        CancellationToken cancellationToken) where T : ITableEntity
    {
        ArgumentNullException.ThrowIfNull(tableName, nameof(tableName));
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        await this.ExecuteTableStorageOperation(
                this.getRequestType(nameof(AddEntityAsync)),
                async () =>
                {
                    TableClient tableClient = this.serviceClient.GetTableClient(tableName);
                    return await tableClient
                        .AddEntityAsync(
                            entity,
                            cancellationToken)
                        .ConfigureAwait(false);
                });
    }

    public async Task DeleteEntityAsync(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName, nameof(tableName));

        await this.ExecuteTableStorageOperation(
                this.getRequestType(nameof(DeleteEntityAsync)),
                async () =>
                {
                    TableClient tableClient = this.serviceClient.GetTableClient(tableName);
                    return await tableClient
                        .DeleteEntityAsync(
                            partitionKey,
                            rowKey,
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                });
    }

    /// <inheritdoc/>
    public async Task<T> GetEntityIfExistsAsync<T>(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken) where T : class, ITableEntity
    {
        ArgumentNullException.ThrowIfNull(tableName, nameof(tableName));

        return await this.ExecuteTableStorageOperation(
                this.getRequestType(nameof(GetEntityIfExistsAsync)),
                async () =>
                {
                    TableClient tableClient = this.serviceClient.GetTableClient(tableName);
                    NullableResponse<T> response = await tableClient
                        .GetEntityIfExistsAsync<T>(
                            partitionKey,
                            rowKey,
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    return response.HasValue ? response.Value : null;
                });
    }

    /// <inheritdoc/>
    public async Task UpdateEntityAsync<T>(
        string tableName,
        T entity,
        CancellationToken cancellationToken,
        TableUpdateMode mode = TableUpdateMode.Replace) where T : ITableEntity
    {
        ArgumentNullException.ThrowIfNull(tableName, nameof(tableName));
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        await this.ExecuteTableStorageOperation(
                this.getRequestType(nameof(UpdateEntityAsync)),
                async () =>
                {
                    TableClient tableClient = this.serviceClient.GetTableClient(tableName);
                    return await tableClient
                        .UpdateEntityAsync(
                            entity,
                            entity.ETag,
                            mode,
                            cancellationToken)
                        .ConfigureAwait(false);
                });
    }

    private async Task<T> ExecuteTableStorageOperation<T>(
        string requestType,
        Func<Task<T>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (RequestFailedException ex)
        {
            throw ex.ToServiceException();
        }
    }
}
