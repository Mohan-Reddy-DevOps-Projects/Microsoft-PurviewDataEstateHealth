// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataAccess.DataAccess.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

/// <summary>
/// A list filter for indexed properties in the ArtifactStore.
/// </summary>
public class IndexedPropertyListEqualityFilter
{
    /// <summary>
    /// A constructor for a <see cref="IndexedPropertyListEqualityFilter"/>.
    /// </summary>
    /// <param name="propertyName">The name of property to filter by.</param>
    /// <param name="propertyValues">An <see cref="IEnumerable{T}"/> containing filter values.</param>
    /// <param name="startIndex">An optional start index to begin the ordinal parameter naming scheme.</param>
    /// <param name="ignoreCase">A bool indicating whether the comparison should be case sensitive.</param>
    public IndexedPropertyListEqualityFilter(
        string propertyName,
        IEnumerable<string> propertyValues,
        int startIndex = 0,
        bool ignoreCase = false)
    {
        this.Parameters = IndexedPropertyListEqualityFilter
            .CreateIndexedParameters(
                propertyName,
                new List<string>(ignoreCase ? propertyValues.Select(v => v.ToLowerInvariant()) : propertyValues),
                startIndex);

        this.Predicate = CreatePredicate(propertyName, this.Parameters.Keys, ignoreCase);
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
    public IDictionary<string, string> Parameters { get; init; }

    /// <summary>
    /// Creates a parameterized predicate expression that checks an indexed property with the provided name for
    /// equality.
    /// </summary>
    /// <param name="propertyName">The name of the indexed property.</param>
    /// <param name="parameterNames">The names of parameters in the predicate.</param>
    /// <param name="ignoreCase">A bool indicating whether the comparison should be case sensitive.</param>
    /// <returns>A parameterized predicate expression that checks an indexed property with the provided name for
    /// equality.
    /// </returns>
    private static string CreatePredicate(string propertyName, IEnumerable<string> parameterNames, bool ignoreCase)
    {
        return string.Join(" OR ", parameterNames.Select(parameterName =>
        {
            string propertyNameExpression = $"E.IndexedProperties.{propertyName.UncapitalizeFirstChar()}";

            if (ignoreCase is true)
            {
                propertyNameExpression = $"LOWER({propertyNameExpression})";
            }

            return FormattableString.Invariant($"{propertyNameExpression} = {parameterName}");
        }));
    }

    private static IDictionary<string, string> CreateIndexedParameters(
        string propertyName,
        IEnumerable<string> propertyValues,
        int startIndex = 0)
    {
        return propertyValues.ToDictionary(_ => $"@{propertyName}{startIndex++}", value => $"{value}");
    }
}
