// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;

internal class MetadataAccessorService : IMetadataAccessorService
{
    private readonly IDataEstateHealthLogger logger;
    private readonly MetadataServiceClientFactory metadataServiceClientFactory;

    public MetadataAccessorService(
        MetadataServiceClientFactory metadataServiceClientFactory,
        IDataEstateHealthLogger logger)
    {
        this.metadataServiceClientFactory = metadataServiceClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public void Initialize()
    {
        this.GetMetadataServiceClient();
    }

    private IProjectBabylonMetadataClient GetMetadataServiceClient()
    {
        return this.metadataServiceClientFactory.GetClient();
    }

    /// <inheritdoc/>
    public Task<string> GetManagedIdentityTokensAsync(
            string accountId,
            CancellationToken cancellationToken)
    {
        try
        {
            //TODO add implementation when onboarded
            return default;
        }
        catch (ErrorResponseModelException exception)
        {
            if (exception.Body.Error.Code.EqualsOrdinalInsensitively("29000"))
            {
                //acccount not found
                return null;
            }

            throw this.LogAndConvert(accountId,  exception);
        }
        catch (Exception exception)
        {
            throw this.LogAndConvert(accountId, exception);
        }
    }

    private ServiceException LogAndConvert(
        string accountId,
        Exception exception)
    {
        // Logging as error to avoid multiple criticals for each try. Higher level caller will log critical on failure.
        this.logger.LogError(
            FormattableString.Invariant(
                $"Failed to perform operation on {accountId}:"),
            exception);

        return new ServiceError(
                ErrorCategory.ServiceError,
                ErrorCode.MetadataServiceException.Code,
                ErrorMessage.DownstreamDependency)
            .ToException();
    }
}
