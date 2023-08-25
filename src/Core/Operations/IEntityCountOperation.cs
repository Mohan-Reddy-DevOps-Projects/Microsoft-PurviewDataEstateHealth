// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;

/// <summary>
/// The contract for components capable of counting entities.
/// </summary>
public interface IEntityCountOperation
{
    /// <summary>
    /// Returns a count of entities.
    /// </summary>
    /// <returns>A count</returns>
    Task<long> Count();
}
