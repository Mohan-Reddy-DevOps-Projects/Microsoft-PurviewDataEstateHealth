// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using Microsoft.DGP.ServiceBasics.BaseModels;

/// <inheritdoc/>
public class BaseBatchResults<TEntity> : IBatchResults<TEntity>
{
    /// <inheritdoc/>
    public IEnumerable<TEntity> Results { get; set; }

    /// <inheritdoc/>
    public string ContinuationToken { get; set; }
}
