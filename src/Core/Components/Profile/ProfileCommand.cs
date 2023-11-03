// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.Options;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;

internal sealed class ProfileCommand : IProfileCommand
{
    private readonly IDataEstateHealthLogger logger;
    private readonly IPowerBIService powerBiService;
    private readonly PowerBIAuthConfiguration powerBIAuthConfig;

    public ProfileCommand(IDataEstateHealthLogger logger, IPowerBIService powerBiService, IOptions<PowerBIAuthConfiguration> powerBIAuthConfig)
    {
        this.logger = logger;
        this.powerBiService = powerBiService;
        this.powerBIAuthConfig = powerBIAuthConfig.Value;
    }

    /// <inheritdoc/>
    public async Task<IProfileModel> Create(IProfileRequest requestContext, CancellationToken cancellationToken)
    {
        Validate(requestContext);
        IProfileModel profileModel = await this.Get(requestContext, cancellationToken);
        if (profileModel != null && profileModel.ClientId == Guid.Parse(this.powerBIAuthConfig.ClientId) && profileModel.TenantId == Guid.Parse(this.powerBIAuthConfig.TenantId))
        {
            return profileModel;
        }

        ServicePrincipalProfile pbiProfile;
        string profileDisplayName = $"{requestContext.ProfileName}-{requestContext.AccountId}";
        try
        {
            pbiProfile = await this.powerBiService.CreateProfile(profileDisplayName, cancellationToken);
        }
        catch (HttpOperationException httpEx) when (httpEx.Response.StatusCode == HttpStatusCode.BadRequest)
        {
            // it is possible that the profile already exists, so try to get it
            this.logger.LogWarning($"Failed to create profile={requestContext};reason={httpEx.Response.Content}", httpEx);
            ServicePrincipalProfiles profiles = await this.powerBiService.GetProfiles(cancellationToken, filter: $"displayName eq '{profileDisplayName}'");
            pbiProfile = profiles.Value.First();
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Failed to create profile={requestContext}", ex);
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.Profile_CreateFailed.Code, $"Failed to create profile.").ToException();
        }

        profileModel = new ProfileModel
        {
            PartitionKey = requestContext.AccountId.ToString(),
            ClientId = Guid.Parse(this.powerBIAuthConfig.ClientId),
            RowKey = pbiProfile.DisplayName,
            Id = pbiProfile.Id,
            TenantId = Guid.Parse(this.powerBIAuthConfig.TenantId)
        };
        // return await this.profileRepository.Create(profileModel, cancellationToken);

        return profileModel;
    }

    /// <inheritdoc/>
    public async Task<DeletionResult> Delete(IProfileRequest requestContext, CancellationToken cancellationToken)
    {
        Validate(requestContext);

        try
        {
            // PofileKey profileKey = new(this.profileRequest.ProfileName, this.profileRequest.AccountId, Guid.Parse(this.powerBIAuthConfig.TenantId));
            if (requestContext.ProfileId == Guid.Empty)
            {
                //IProfileModel profile = await profileRepository.GetSingle(profileKey, cancellationToken);
                requestContext = new ProfileRequest(requestContext)
                {
                    // ProfileId = profile.Id
                };
            }
            // await this.powerBiService.DeleteProfile(requestContext.ProfileId, cancellationToken);
            // await profileRepository.Delete(profileKey, cancellationToken);
        }
        catch (Exception ex)
        {
            // TODO: catch not found exception in case the profile is already deleted
            this.logger.LogError($"Failed to delete profile={requestContext}", ex);
            return new DeletionResult()
            {
                DeletionStatus = DeletionStatus.Unknown
            };
        }
        await Task.CompletedTask;

        return new DeletionResult()
        {
            DeletionStatus = DeletionStatus.Deleted
        };
    }

    /// <inheritdoc/>
    public async Task<IProfileModel> Get(IProfileRequest requestContext, CancellationToken cancellationToken)
    {
        Validate(requestContext);

        string profileDisplayName = $"{requestContext.ProfileName}-{requestContext.AccountId}";
        ServicePrincipalProfile pbiProfile;
        try
        {
            ServicePrincipalProfiles profiles = await this.powerBiService.GetProfiles(cancellationToken, filter: $"displayName eq '{profileDisplayName}'");
            pbiProfile = profiles.Value.First();
        }
        catch (HttpOperationException httpEx) when (httpEx.Response.StatusCode == HttpStatusCode.NotFound)
        {
            this.logger.LogWarning($"Failed to get profile={requestContext}", httpEx);
            return null;
        }

        return new ProfileModel()
        {
            PartitionKey = requestContext.AccountId.ToString(),
            ClientId = Guid.Parse(this.powerBIAuthConfig.ClientId),
            RowKey = requestContext.ProfileName,
            Id = pbiProfile.Id,
            TenantId = Guid.Parse(this.powerBIAuthConfig.TenantId)
        };
        //ProfileKey profileKey = new(this.profileRequest.ProfileName, this.profileRequest.AccountId, Guid.Parse(this.powerBIAuthConfig.TenantId));

        //return await this.profileRepository.GetSingle(profileKey, cancellationToken);
    }

    private static void Validate(IProfileRequest requestContext)
    {
        if (string.IsNullOrEmpty(requestContext.ProfileName))
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing ProfileName.").ToException();
        }
        if (requestContext.AccountId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing AccountId.").ToException();
        }
    }
}
