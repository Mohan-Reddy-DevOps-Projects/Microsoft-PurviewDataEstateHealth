// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;

/// <summary>
/// Artifact store account component.
/// </summary>
public interface IArtifactStoreAccountComponent
{
    /// <summary>
    /// Create artifact store resources
    /// </summary>
    /// <param name="account"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateArtifactStoreResources(AccountServiceModel account, CancellationToken cancellationToken);
}
