// <copyright file="SorterHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Helpers;

using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Purview.DataEstateHealth.DHModels.Queries;
using System;
using System.Collections.Generic;

public static class SorterHelper
{
    public static List<Sorter> ParseSorters(List<OrderBy> orderBys, Dictionary<string, string> validFields)
    {
        if (orderBys == null || orderBys.Count == 0)
        {
            return null;
        }
        var sorters = new List<Sorter>();

        foreach (var orderBy in orderBys)
        {
            var field = orderBy.Field.Trim();
            var order = string.Equals(orderBy.Direction?.Trim(), "desc", StringComparison.OrdinalIgnoreCase) ? SortOrder.Descending : SortOrder.Ascending;

            if (validFields == null || !validFields.TryGetValue(field, out string fieldNameInDB))
            {
                throw new InvalidRequestException($"Invalid sorter field: {field}. Available sorters: {string.Join(", ", validFields)}");
            }

            var sorter = new Sorter
            {
                Field = fieldNameInDB,
                Order = order
            };

            sorters.Add(sorter);
        }
        return sorters;
    }
}
