// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Azure.ProjectBabylon.Metadata;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.MetadataStore;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest.TransientFaultHandling;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

internal class MetadataAccessorService : IMetadataAccessorService
{
    private readonly IProjectBabylonMetadataClient metadataClient;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly RetryPolicy<TransientErrorIgnoreStrategy> noRetryPolicy = new(0);

    private readonly MetadataServiceConfiguration metadataServiceConfiguration;

    private readonly EnvironmentConfiguration environmentConfiguration;

    public MetadataAccessorService(
        IProjectBabylonMetadataClient metadataClient,
        IDataEstateHealthRequestLogger dataEstateHealthRequestLogger,
        IRequestHeaderContext requestHeaderContext,
        IOptions<MetadataServiceConfiguration> metadataServiceConfiguration,
        IOptions<EnvironmentConfiguration> environmentConfiguration)
    {
        this.metadataClient = metadataClient;
        this.dataEstateHealthRequestLogger = dataEstateHealthRequestLogger;
        this.requestHeaderContext = requestHeaderContext;
        this.metadataServiceConfiguration = metadataServiceConfiguration.Value;
        metadataClient.ApiVersion = this.metadataServiceConfiguration.ApiVersion;
        this.environmentConfiguration = environmentConfiguration.Value;
        this.metadataClient.BaseUri = new Uri(this.metadataServiceConfiguration.Endpoint);
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
        this.dataEstateHealthRequestLogger.LogError(
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
