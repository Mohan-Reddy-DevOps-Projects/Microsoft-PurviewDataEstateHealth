// <copyright file="BatchResults.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess
{
    using System.Collections.Generic;

    /// <summary>
    /// Entity for collecing batched results.
    /// </summary>
    /// <typeparam name="T">Entity Type.</typeparam>
    public class BatchResults<T> : IBatchResults<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchResults{T}"/> class.
        /// </summary>
        /// <param name="results">Result entities.</param>
        /// <param name="count">Result count.</param>
        public BatchResults(IEnumerable<T> results, int count)
        {
            this.Results = results;
            this.Count = count;
        }

        /// <summary>
        /// Gets or sets the Results.
        /// </summary>
        public IEnumerable<T> Results { get; set; }

        /// <summary>
        /// Gets the result count.
        /// </summary>
        public int Count { get; set; }
    }
}
