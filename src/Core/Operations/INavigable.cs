// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// The contract for components capable of navigating to other components using an ID.
/// </summary>
public interface INavigable<TId, TTargetComponent>
{
    /// <summary>
    /// Navigates from the current a collection scope to a single entity scope.
    /// </summary>
    /// <param name="id">The single entity's ID to navigate the context to.</param>
    /// <returns>The single entity's component.</returns>
    TTargetComponent ById(TId id);
}
