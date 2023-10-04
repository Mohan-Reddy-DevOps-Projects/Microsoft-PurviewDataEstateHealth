// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Contract for components that are location based.
/// </summary>
/// <typeparam name="TComponent">The type of the components.</typeparam>
public interface ILocationBased<out TComponent>
{
    /// <summary>
    /// Returns an instance customized to the given location.
    /// </summary>
    /// <param name="location">The location to work with.</param>
    /// <returns>An instance customized to the given location.</returns>
    TComponent ByLocation(string location);
}
