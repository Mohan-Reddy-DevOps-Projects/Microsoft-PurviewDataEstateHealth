// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using global::Microsoft.DGP.ServiceBasics.Errors;

/// <summary>
/// Base class capable of creating concrete implementation classes based on a selector defined and attached to a class attribute.
/// </summary>
/// <typeparam name="TSelectorAttribute">Selector attribute.</typeparam>
/// <typeparam name="TSelector">Implementation class selector value.</typeparam>
/// <typeparam name="TValue">Implementation class type.</typeparam>
public abstract class Registry<TSelectorAttribute, TSelector, TValue> where TSelectorAttribute : Attribute where TValue : class
{
    private readonly Func<TSelectorAttribute, TSelector> selectorFunc;
    private readonly Lazy<Dictionary<TSelector, (Type implementation, TSelectorAttribute attribute)>> selectorMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="Registry{TSelectorAttribute, TSelector, TValue}"/> class.
    /// </summary>
    /// <param name="selectorFunc">A function that returns the selector value from the given attribute.</param>
    protected Registry(Func<TSelectorAttribute, TSelector> selectorFunc)
    {
        this.selectorFunc = selectorFunc;
        this.selectorMap = new Lazy<Dictionary<TSelector, (Type implementation, TSelectorAttribute attribute)>>(this.InitializeSelectorMap);
    }

    /// <summary>
    /// Creates an implementation class for the given selector and passes the arguments to its constructor.
    /// </summary>
    /// <param name="selector">The implementation class selector.</param>
    /// <param name="arguments">Optional arguments for the class constructor.</param>
    /// <returns>An implementation class.</returns>
    /// <exception cref="ServiceException"></exception>
    protected TValue Resolve(TSelector selector, params object[] arguments)
    {
        return arguments.Length == 1 && arguments[0] == null
            ? Activator.CreateInstance(this.ImplementatationFor(selector)) as TValue
            : Activator.CreateInstance(this.ImplementatationFor(selector), arguments) as TValue;
    }

    /// <summary>
    /// Returns the implementation class type for the given selector.
    /// </summary>
    /// <param name="selector">The implementation class selector.</param>
    /// <returns>Implementation class type.</returns>
    /// <exception cref="ServiceException"></exception>
    protected Type ImplementatationFor(TSelector selector)
    {
        this.ValidateSelector(selector);

        return this.selectorMap.Value[selector].implementation;
    }

    /// <summary>
    /// Returns the attribute associated with the given selector.
    /// </summary>
    /// <param name="selector">An implementation type selector.</param>
    /// <returns>The attribute associated with the given selector.</returns>
    /// <exception cref="ServiceException"></exception>
    protected TSelectorAttribute AttributeFor(TSelector selector)
    {
        this.ValidateSelector(selector);

        return this.selectorMap.Value[selector].attribute;
    }

    private Dictionary<TSelector, (Type implementation, TSelectorAttribute attribute)> InitializeSelectorMap()
    {
        IEnumerable<(Type type, TSelectorAttribute attribute)> implementationTypes = this
            .GetType()
            .Assembly
            .GetTypes()
            .Select(
                type => (type,
                    attribute: type.GetCustomAttributes(typeof(TSelectorAttribute), true).FirstOrDefault() as
                        TSelectorAttribute))
            .Where(t => t.attribute != null);

        var selectorMap = new Dictionary<TSelector, (Type implementation, TSelectorAttribute attribute)>();

        foreach ((Type type, TSelectorAttribute attribute) in implementationTypes)
        {
            var selector = this.selectorFunc(attribute);

            if (!typeof(TValue).IsAssignableFrom(type) && !this.IsSubclassOfRawGeneric(typeof(TValue), type))
            {
                throw new ServiceError(
                        ErrorCategory.ServiceError,
                        ErrorCode.Unknown.Code,
                        FormattableString.Invariant($"{selector} {type.FullName} must extend {typeof(TValue).FullName}."))
                    .ToException();
            }

            if (!selectorMap.ContainsKey(selector))
            {
                selectorMap.Add(selector, (implementation: type, attribute: attribute));
            }
            else
            {
                throw new ServiceError(
                        ErrorCategory.ServiceError,
                        ErrorCode.Unknown.Code,
                        FormattableString.Invariant(
                            $"Multiple implementations found for selector {selector} - {type.FullName} and {selectorMap[selector].implementation.FullName}."))
                    .ToException();
            }
        }

        return selectorMap;
    }

    private void ValidateSelector(TSelector selector)
    {
        if (!this.selectorMap.Value.ContainsKey(selector))
        {
            throw new ServiceError(
                    ErrorCategory.ServiceError,
                    ErrorCode.Unknown.Code,
                    $"{selector} does not have a registered implementation class in {this.GetType().FullName}")
                .ToException();
        }
    }

    private bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

            if (generic == cur)
            {
                return true;
            }

            toCheck = toCheck.BaseType;
        }

        return false;
    }
}
