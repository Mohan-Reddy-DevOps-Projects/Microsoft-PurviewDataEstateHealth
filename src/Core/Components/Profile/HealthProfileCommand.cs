// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Common;
using Microsoft.Purview.DataGovernance.Reporting.Models;

/// <summary>
/// Creates a unique profile. If the profile already exists, it will be returned without modification.
/// </summary>
internal sealed class HealthProfileCommand : IEntityCreateOperation<Guid, IProfileModel>,
    IRetrieveEntityByIdOperation<Guid, IProfileModel>,
    IEntityDeleteOperation<Guid>
{
    private readonly ProfileProvider profileCommand;

    public HealthProfileCommand(ProfileProvider profileCommand)
    {
        this.profileCommand = profileCommand;
    }

    /// <inheritdoc/>
    public async Task<IProfileModel> Create(Guid accountId, CancellationToken cancellationToken)
    {
        IProfileRequest profileRequest = new ProfileRequest()
        {
            AccountId = accountId,
            ProfileName = OwnerNames.Health
        };

        return await this.profileCommand.Create(profileRequest, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DeletionResult> Delete(Guid accountId, CancellationToken cancellationToken)
    {
        return await this.profileCommand.Delete(OwnerNames.Health, accountId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IProfileModel> Get(Guid accountId, CancellationToken cancellationToken)
    {
        return await this.profileCommand.Get(OwnerNames.Health, accountId, cancellationToken);
    }
}
