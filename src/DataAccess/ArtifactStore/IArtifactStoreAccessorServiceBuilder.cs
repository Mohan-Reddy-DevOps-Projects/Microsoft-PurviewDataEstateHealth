// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;

/// <summary>
/// An interface for classes that can build artifact store accessor services.
/// </summary>
internal interface IArtifactStoreAccessorServiceBuilder
{
    /// <summary>
    /// Builds an artifact store accessor service for the given location.
    /// </summary>
    /// <returns>An artifact store accessor service</returns>
    IArtifactStoreAccessorService Build();
}
