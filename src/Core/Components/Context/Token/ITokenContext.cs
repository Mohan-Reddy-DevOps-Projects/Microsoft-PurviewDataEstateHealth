// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Token context
/// </summary>
public interface ITokenContext : IRootComponentContext
{
    /// <summary>
    /// The owner.
    /// </summary>
    string Owner { get; }
}
