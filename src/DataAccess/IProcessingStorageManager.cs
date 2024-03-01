// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using System.Threading;
using System.Threading.Tasks;
using ProcessingStorageModel = Models.ProcessingStorageModel;
using StorageSasRequest = Models.StorageSasRequest;

/// <summary>
/// Defines the processing storage manager.
/// </summary>
public interface IProcessingStorageManager
{
    /// <summary>
    /// Get processing storage account.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ProcessingStorageModel> Get(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);

    /// <summary>
    /// Get processing storage account.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ProcessingStorageModel> Get(Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    /// Provision default storage account.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Provision(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);

    /// <summary>
    /// Delete default storage account.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<DeletionResult> Delete(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the processing storage SAS URI.
    /// </summary>
    /// <param name="processingStorageModel"></param>
    /// <param name="parameters">SAS Token Parameters</param>
    /// <param name="containerName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The processing storage SAS token.</returns>
    Task<Uri> GetProcessingStorageSasUri(ProcessingStorageModel processingStorageModel, StorageSasRequest parameters, string containerName, CancellationToken cancellationToken);

    /// <summary>
    /// Constructs the container path.
    /// </summary>
    /// <param name="containerName"></param>
    /// <param name="accountId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> ConstructContainerPath(string containerName, Guid accountId, CancellationToken cancellationToken);

    Task<List<string>> GetDataQualityOutputFileNames(ProcessingStorageModel processingStorageModel, string folderPath);

    Task<Stream> GetDataQualityOutput(ProcessingStorageModel processingStorageModel, string folderPath, string fileName);

    Task<string> GetSasTokenForDQ(ProcessingStorageModel processingStorageModel, StorageSasRequest parameters);
}
