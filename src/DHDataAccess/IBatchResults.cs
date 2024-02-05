#nullable enable
// <copyright file="IBatchResults.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess
{
    using System.Collections.Generic;

    public interface IBatchResults<out TEntity>
    {
        /// <summary>
        /// Gets the current results.
        /// </summary>
        IEnumerable<TEntity> Results { get; }

        /// <summary>
        /// Gets the result count.
        /// </summary>
        int Count { get; }
    }
}
