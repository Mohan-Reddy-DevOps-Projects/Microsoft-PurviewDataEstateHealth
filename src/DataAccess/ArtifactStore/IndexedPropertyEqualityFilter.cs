// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;

using System;
using System.Collections.Generic;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

/// <summary>
/// An equality filter for indexed properties in the ArtifactStore.
/// </summary>
internal class IndexedPropertyEqualityFilter
{
    /// <summary>
    /// A constructor for a <see cref="IndexedPropertyEqualityFilter"/>.
    /// </summary>
    /// <param name="propertyName">The name of the property to apply the filter upon.</param>
    /// <param name="propertyValue">The value to filter by.</param>
    /// <param name="ignoreCase">A bool indicating whether the comparison should be case sensitive.</param>
    public IndexedPropertyEqualityFilter(string propertyName, string propertyValue, bool ignoreCase = false)
    {
        this.Predicate = IndexedPropertyEqualityFilter.CreatePredicate(propertyName, ignoreCase);
        this.Parameters = new Dictionary<string, string>
        {
            {
                FormattableString.Invariant($"@{propertyName}"),
                ignoreCase ? propertyValue.ToLowerInvariant() : propertyValue
            }
        };
    }

    /// <summary>
    /// An expression that must be satisfied by entities to be included in the result set. May contain parameters
    /// denoted with a leading '@' symbol.
    /// </summary>
    public string Predicate { get; init; }

    /// <summary>
    /// A <see cref="Dictionary{TKey, TValue}"/> that associates parameter names with parameter values.  Parameter
    /// names are extracted from the provided predicate.
    /// </summary>
    public Dictionary<string, string> Parameters { get; init; }

    /// <summary>
    /// Creates a parameterized predicate expression that checks an indexed property with the provided name for
    /// equality.
    /// </summary>
    /// <param name="propertyName">The name of the indexed property.</param>
    /// <param name="ignoreCase">A bool indicating whether the comparison should be case sensitive.</param>
    /// <returns>A parameterized predicate expression that checks an indexed property with the provided name for
    /// equality.
    /// </returns>
    private static string CreatePredicate(string propertyName, bool ignoreCase)
    {
        string propertyNameExpression = $"E.IndexedProperties.{propertyName.UncapitalizeFirstChar()}";

        if (ignoreCase is true)
        {
            propertyNameExpression = $"LOWER({propertyNameExpression})";
        }

        return FormattableString.Invariant($"{propertyNameExpression} = @{propertyName}");
    }
}
