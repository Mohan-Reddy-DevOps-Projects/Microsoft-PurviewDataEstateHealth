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
}
