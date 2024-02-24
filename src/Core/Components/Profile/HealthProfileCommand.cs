// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;

/// <summary>
/// Creates a unique profile. If the profile already exists, it will be returned without modification.
/// </summary>
internal sealed class HealthProfileCommand : IHealthProfileCommand
{
    private readonly ProfileProvider profileCommand;

    public HealthProfileCommand(ProfileProvider profileCommand)
    {
        this.profileCommand = profileCommand;
    }

    /// <inheritdoc/>
    public async Task<IProfileModel> Create(ProfileKey profileKey, CancellationToken cancellationToken)
    {
        IProfileRequest profileRequest = new ProfileRequest()
        {
            AccountId = profileKey.PartitionKey,
            ProfileName = profileKey.Name
        };

        return await this.profileCommand.Create(profileRequest, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Microsoft.Purview.DataGovernance.Reporting.Common.DeletionResult> Delete(ProfileKey profileKey, CancellationToken cancellationToken)
    {
        return await this.profileCommand.Delete(profileKey.Name, profileKey.PartitionKey, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IProfileModel> Get(ProfileKey profileKey, CancellationToken cancellationToken)
    {
        return await this.profileCommand.Get(profileKey.Name, profileKey.PartitionKey, cancellationToken);
    }
}
