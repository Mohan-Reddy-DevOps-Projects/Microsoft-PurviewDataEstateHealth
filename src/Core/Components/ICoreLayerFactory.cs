// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Contract for creating core layer components.
/// </summary>
public interface ICoreLayerFactory
{
    /// <summary>
    /// Scoped the factory to a given service version.
    /// </summary>
    /// <param name="version">The service version.</param>
    /// <returns>The operations.</returns>
    ICoreLayerFactoryOperations Of(ServiceVersion version);
}
