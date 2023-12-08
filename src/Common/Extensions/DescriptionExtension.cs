// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

/// <summary>
/// Extension Methods to Retrieve Values from <see cref="DescriptionAttribute"/>.
/// </summary>
public static class DescriptionExtension
{
    /// <summary>
    /// Returns the string description of the given enumeration.
    /// </summary>
    public static string GetDescription<T>(this T e) where T : IConvertible
    {
        if (e is Enum)
        {
            Type type = e.GetType();
            Array values = Enum.GetValues(type);

            foreach (int val in values)
            {
                if (val == e.ToInt32(CultureInfo.InvariantCulture))
                {
                    MemberInfo[] memInfo = type.GetMember(type.GetEnumName(val));

                    if (memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is
                        DescriptionAttribute descriptionAttribute)
                    {
                        return descriptionAttribute.Description;
                    }
                }
            }
        }

        return string.Empty;
    }
}
