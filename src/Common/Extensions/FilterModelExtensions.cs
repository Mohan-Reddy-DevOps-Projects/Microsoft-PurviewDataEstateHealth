// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

using System;
using System.Collections.Generic;
using Microsoft.DGP.ServiceBasics.BaseModels;

/// <summary>
/// Filter model extensions
/// </summary>
public static class FilterModelExtensions
{
    /// <summary>
    /// Convert filter model to the artifact store list parameters.
    /// </summary>
    /// <param name="filterModel"></param>
    /// <returns></returns>
    public static Dictionary<string, string> ToArtifactStoreFilterParameters(this FilterModel filterModel)
    {
        Dictionary<string, string> filterParameters = new();

        if (filterModel?.FilterDictionary == null)
        {
            return filterParameters;
        }

        foreach (var filterKeyValuePair in filterModel.FilterDictionary)
        {
            filterParameters.Add(
                FormattableString.Invariant($"@{filterKeyValuePair.Key}"),
                filterKeyValuePair.Value);
        }

        return filterParameters;
    }

    /// <summary>
    /// Convert the filter model to the artifact store filter text.
    /// </summary>
    /// <param name="filterModel"></param>
    /// <returns></returns>
    public static List<string> ToArtifactStoreFilterText(this FilterModel filterModel)
    {
        var filterText = new List<string>();

        if (filterModel?.FilterDictionary == null)
        {
            return filterText;
        }

        foreach (KeyValuePair<string, string> filterKeyValuePair in filterModel.FilterDictionary)
        {
            filterText.Add(
                FormattableString.Invariant(
                    $"E.IndexedProperties.{filterKeyValuePair.Key.UncapitalizeFirstChar()} = @{filterKeyValuePair.Key}"));
        }

        return filterText;
    }

    /// <summary>
    /// Determines if the <see cref="FilterModel"/> contains the provided filter.
    /// </summary>
    /// <param name="filterModel">A <see cref="FilterModel"/>.</param>
    /// <param name="filterName">The name of the filter.</param>
    /// <returns>true if the <see cref="FilterModel"/> contains the provided filterName; otherwise, false.</returns>
    public static bool ContainsFilter(this FilterModel filterModel, string filterName)
    {
        return filterModel?.FilterDictionary.ContainsKey(filterName) == true;
    }
}
