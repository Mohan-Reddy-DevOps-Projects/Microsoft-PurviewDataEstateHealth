// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using System;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Class that holds utility methods for queries.
/// </summary>
public static class QueryUtils
{
    /// <summary>
    /// Get custom attribute.
    /// </summary>
    public static T GetCustomAttribute<T, U>(Expression<Func<U, object>> propertyExpression)
        where T : Attribute
    {
        if (propertyExpression.Body is UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MemberExpression memberExpression)
            {
                if (memberExpression.Member is PropertyInfo propertyInfo)
                {
                    return propertyInfo.GetCustomAttribute<T>();
                }
            }
        }
        else if (propertyExpression.Body is MemberExpression memberExpression)
        {
            if (memberExpression.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetCustomAttribute<T>();
            }
        }
        throw new ArgumentException("Expression must be a property expression.");
    }

#nullable enable
    /// <summary>
    /// TryParse as datetime.
    /// </summary>
    public static DateTime AsDateTime(this string? input)
    {
        if (!DateTime.TryParse(input, out DateTime result))
        {
            throw new ArgumentException($"failed to parse as datetime: '{input}'");
        }

        return result;
    }

    /// <summary>
    /// TryParse as float.
    /// </summary>
    public static double AsFloat(this string? input)
    {
        if (!float.TryParse(input, out float result))
        {
            throw new ArgumentException($"failed to parse as float: '{input}'");
        }

        return result;
    }

    /// <summary>
    /// TryParse as int.
    /// </summary>
    public static int AsInt(this string? input)
    {
        if (!int.TryParse(input, out int result))
        {
            throw new ArgumentException($"failed to parse as int: '{input}'");
        }

        return result;
    }

    /// <summary>
    /// TryParse as Guid.
    /// </summary>
    public static Guid AsGuid(this string? input)
    {
        if (!Guid.TryParse(input, out Guid result))
        {
            throw new ArgumentException($"failed to parse as Guid: '{input}'");
        }

        return result;
    }
#nullable disable

}
