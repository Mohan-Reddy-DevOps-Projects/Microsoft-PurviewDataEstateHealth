// <copyright file="CosmosDBQuery.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels.Queries
{
    using System.Collections.Generic;

    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public class CosmosDBQuery<TFilter>
    {
        public static readonly int DefaultTop = 1000;

        public int Skip { get; set; }

        public int Top { get; set; } = DefaultTop;

        public TFilter? Filter { get; set; }

        public List<Sorter>? Sorters { get; set; }
    }

    public class Sorter
    {
        public required string Field { get; set; }

        public SortOrder Order { get; set; } = SortOrder.Ascending;
    }
}
