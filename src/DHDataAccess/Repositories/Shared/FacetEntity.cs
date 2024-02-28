// <copyright file="FacetEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.Shared
{
    using System.Collections.Generic;

    public class FacetEntity
    {
        public bool IsEnabled { get; set; }

        public IEnumerable<FacetEntityItem>? Items { get; set; }

        public FacetEntity()
        {
        }

        public FacetEntity(bool isEnabled)
        {
            this.IsEnabled = isEnabled;
        }
    }

    public class FacetEntityItem
    {
        public string Value { get; set; }

        public int Count { get; set; }

        public FacetEntityItem(string value, int count)
        {
            this.Value = value;
            this.Count = count;
        }
    }
}
