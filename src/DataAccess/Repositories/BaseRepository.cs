// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal class BaseRepository : IBaseRepository
{
    protected readonly IProcessingStorageManager processingStorageManager;

    public BaseRepository(
        IProcessingStorageManager processingStorageManager)
    {
        this.processingStorageManager = processingStorageManager;
    }

    public async Task<string> ConstructContainerPath(string containerName, Guid accountId, CancellationToken cancellationToken)
    {
        Models.ProcessingStorageModel storageModel = await this.processingStorageManager.Get(accountId, cancellationToken);
        ArgumentNullException.ThrowIfNull(storageModel, nameof(storageModel));

        return $"{storageModel.GetDfsEndpoint()}/{containerName}";
    }
}
