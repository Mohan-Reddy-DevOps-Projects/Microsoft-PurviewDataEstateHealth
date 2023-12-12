// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;


/// <summary>
/// Defines properties used for <see cref="IRefreshHistoryComponent"/> operations.
/// </summary>
public interface IRefreshHistoryContext : IRootComponentContext
{
    /// <summary>
    /// Gets the dataset id.
    /// </summary>
    Guid DatasetId { get; }

    /// <summary>
    /// Gets the number of entities to return.
    /// </summary>
    int? Top { get; }
}
