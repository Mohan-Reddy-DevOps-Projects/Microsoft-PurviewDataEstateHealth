// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System.Reflection;
using System.Collections.Concurrent;

/// <summary>
/// Region extensions
/// </summary>
public static class RegionExtensions
{
    /// <summary>
    /// Checks if region is valid
    /// </summary>
    /// <param name="region"></param>
    /// <returns></returns>
    public static bool IsValidRegion(this Region region)
    {
        IEnumerable<object> allowedRegionsEnumerable = typeof(Region).GetFields(BindingFlags.Static | BindingFlags.Public)
        .Select(p => p.GetValue(null));
        var allowedRegions = new ConcurrentBag<object>(allowedRegionsEnumerable);

        var centralUSEuap = Region.Create("Central US EUAP");
        var eastUSEuap = Region.Create("East US 2 EUAP");

        if (!allowedRegions.Contains(centralUSEuap))
        {
            allowedRegions.Add(centralUSEuap);
        }

        if (!allowedRegions.Contains(eastUSEuap))
        {
            allowedRegions.Add(eastUSEuap);
        }

        return allowedRegions.Any(v => Region.Create(region.Name).Equals(v));
    }
}
