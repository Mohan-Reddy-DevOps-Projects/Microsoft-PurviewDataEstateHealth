// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;

/// <summary>
/// Creates a unique profile. If the profile already exists, it will be returned without modification.
/// </summary>
internal sealed class HealthProfileCommand : IEntityCreateOperation<Guid, IProfileModel>,
    IRetrieveEntityByIdOperation<Guid, IProfileModel>,
    IEntityDeleteOperation<Guid>
{
    private const string HealthProfileName = "health";
    private readonly IProfileCommand profileCommand;

    public HealthProfileCommand(IProfileCommand profileCommand)
    {
        this.profileCommand = profileCommand;
    }

    /// <inheritdoc/>
    public async Task<IProfileModel> Create(Guid accountId, CancellationToken cancellationToken)
    {
        IProfileRequest profileRequest = new ProfileRequest()
        {
            AccountId = accountId,
            ProfileName = HealthProfileName
        };

        return await this.profileCommand.Create(profileRequest, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DeletionResult> Delete(Guid accountId, CancellationToken cancellationToken)
    {
        IProfileRequest profileRequest = new ProfileRequest()
        {
            AccountId = accountId,
            ProfileName = HealthProfileName
        };

        return await this.profileCommand.Delete(profileRequest, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IProfileModel> Get(Guid accountId, CancellationToken cancellationToken)
    {
        IProfileRequest profileRequest = new ProfileRequest()
        {
            AccountId = accountId,
            ProfileName = HealthProfileName
        };

        return await this.profileCommand.Get(profileRequest, cancellationToken) ?? throw new ServiceError(
                ErrorCategory.ServiceError,
                ErrorCode.Profile_NotFound.Code,
                ErrorCode.Profile_NotFound.Message)
                .ToException();
    }
}
