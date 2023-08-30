// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

using System;
using Newtonsoft.Json;

/// <summary>
/// An extension method to create deep copies of objects.
/// </summary>
public static class DeepCopyExtension
{
    /// <summary>
    /// Returns a deep copy of the object.
    /// </summary>
    public static T DeepCopy<T>(this T self)
    {
        try
        {
            string serialized = JsonConvert.SerializeObject(self);

            return JsonConvert.DeserializeObject<T>(serialized);
        }
        catch (Exception)
        {
            return default;
        }
    }
}
