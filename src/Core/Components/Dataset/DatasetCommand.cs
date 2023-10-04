// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.PowerBI.Api.Models;
using Microsoft.WindowsAzure.Storage.Blob;

internal sealed class DatasetCommand : IDatasetCommand
{
    private readonly IDataEstateHealthLogger logger;
    private readonly IPowerBIService powerBiService;

    public DatasetCommand(IDataEstateHealthLogger logger, IPowerBIService powerBiService)
    {
        this.logger = logger;
        this.powerBiService = powerBiService;
    }

    /// <inheritdoc/>
    public async Task<Dataset> Create(IDatasetRequest requestContext, CancellationToken cancellationToken)
    {
        CreateValidate(requestContext);

        Dictionary<string, string> parameters = new()
        {
            { "SERVER", requestContext.Server },
            { "DATABASE", requestContext.DatabaseName },
            { "DATABASE_SCHEMA", requestContext.DatabaseSchema }
        };

        (BlobProperties properties, MemoryStream ms) = await this.GetDatasetFile(requestContext, cancellationToken);
        using (ms)
        {
            ms.Seek(0, SeekOrigin.Begin);
            Import import = await this.powerBiService.CreateDataset(requestContext.ProfileId, requestContext.WorkspaceId, ms, requestContext.DatasetName, parameters, requestContext.PowerBICredential, cancellationToken, requestContext.OptimizedDataset);
            Dataset dataset = import.Datasets.First();

            return dataset;
        }
    }

    /// <inheritdoc/>
    public async Task<DeletionResult> Delete(IDatasetRequest requestContext, CancellationToken cancellationToken)
    {
        if (requestContext.ProfileId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing ProfileId.").ToException();
        }
        if (requestContext.WorkspaceId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing WorkspaceId.").ToException();
        }
        if (requestContext.DatasetId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing DatasetId.").ToException();
        }

        using HttpResponseMessage response = await this.powerBiService.DeleteDataset(requestContext.ProfileId, requestContext.WorkspaceId, requestContext.DatasetId, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            this.logger.LogTrace($"Failed to delete dataset. Dataset does not exist. {requestContext};");

            return new DeletionResult()
            {
                DeletionStatus = DeletionStatus.ResourceNotFound,
                JobId = Guid.NewGuid().ToString(),
                Location = string.Empty
            };
        }

        response.EnsureSuccessStatusCode();

        return new DeletionResult()
        {
            DeletionStatus = DeletionStatus.Deleted,
            JobId = Guid.NewGuid().ToString(),
            Location = string.Empty
        };
    }

    /// <inheritdoc/>
    public async Task<Dataset> Get(IDatasetRequest requestContext, CancellationToken cancellationToken)
    {
        if (requestContext.ProfileId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing ProfileId.").ToException();
        }
        if (requestContext.WorkspaceId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing WorkspaceId.").ToException();
        }
        if (requestContext.DatasetId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing DatasetId.").ToException();
        }

        return await this.powerBiService.GetDataset(requestContext.ProfileId, requestContext.WorkspaceId, requestContext.DatasetId, cancellationToken);
    }

    /// <summary>
    /// Get a dataset.
    /// </summary>
    /// <param name="requestContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Datasets> List(IDatasetRequest requestContext, CancellationToken cancellationToken)
    {
        if (requestContext.ProfileId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing ProfileId.").ToException();
        }
        if (requestContext.WorkspaceId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing WorkspaceId.").ToException();
        }

        return await this.powerBiService.GetDatasets(requestContext.ProfileId, requestContext.WorkspaceId, cancellationToken);
    }

    /// <summary>
    /// Retrieve the dataset file from storage.
    /// </summary>
    /// <param name="requestContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<(BlobProperties properties, MemoryStream ms)> GetDatasetFile(IDatasetRequest requestContext, CancellationToken cancellationToken)
    {
        BlobProperties properties = new();
        MemoryStream ms = new();

        await Task.CompletedTask;

        return (properties, ms);
    }

    private static void CreateValidate(IDatasetRequest requestContext)
    {
        if (requestContext.ProfileId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing ProfileId.").ToException();
        }
        if (requestContext.WorkspaceId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing WorkspaceId.").ToException();
        }
        if (requestContext.AccountId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing AccountId.").ToException();
        }
        if (string.IsNullOrEmpty(requestContext.DatasetName))
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing DatasetName.").ToException();
        }
        if (string.IsNullOrEmpty(requestContext.DatabaseSchema))
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing DatabaseSchema.").ToException();
        }
        if (string.IsNullOrEmpty(requestContext.Server))
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing Server.").ToException();
        }
        if (string.IsNullOrEmpty(requestContext.DatasetContainer))
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing DatasetContainer.").ToException();
        }
        if (string.IsNullOrEmpty(requestContext.DatasetFileName))
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing DatasetFileName.").ToException();
        }
        if (requestContext.PowerBICredential == null)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing PowerBICredential.").ToException();
        }
    }   
}
