// <copyright file="FacetAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FacetAttribute : Attribute
    {
        public FacetAttribute()
        {
        }

        public FacetAttribute(string facetName, string? graphAlias = null)
        {
            if (string.IsNullOrEmpty(facetName))
            {
                throw new ArgumentException("facetName cannot be null or empty", nameof(facetName));
            }
            this.FacetName = facetName;
            this.GraphAlias = graphAlias ?? facetName;
        }

        public string? GraphAlias { get; }

        public string? FacetName { get; }
    }
}
