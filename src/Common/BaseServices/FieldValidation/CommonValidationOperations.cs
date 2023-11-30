// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using System.Collections;
using System.Collections.Generic;
using Microsoft.DGP.ServiceBasics.Services.FieldValidation;

/// <inheritdoc />
public class CommonValidationOperations<TField> : ValidationOperations<TField>, ICommonValidationOperations<TField>
{
    internal CommonValidationOperations(string fieldName, TField fieldValue) : base(fieldName, fieldValue)
    {
    }

    /// <inheritdoc/>
    public ICommonValidationOperations<TField> IsNonEmptyList()
    {
        this.Required();

        bool isList = this.FieldValue is IList &&
           this.FieldValue.GetType().IsGenericType &&
           this.FieldValue.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));

        if (!isList)
        {
            this.ReturnInvalidField($"{this.FieldName} is not a valid list.");
        }

        var enumerable = this.FieldValue as IList;

        this.Validate(
            () => enumerable != null && enumerable.Count != 0,
            errorMessage: $"{this.FieldName} does not contain any elements.");

        return this;
    }

    /// <inheritdoc/>
    public ICommonValidationOperations<TField> IsNonEmptyEnumerable()
    {
        this.Required();

        bool isEnumerable = this.FieldValue is IEnumerable &&
           this.FieldValue.GetType().IsGenericType;

        if (!isEnumerable)
        {
            this.ReturnInvalidField($"{this.FieldName} is not a valid enumerable.");
        }

        var enumerable = this.FieldValue as IEnumerable;

        this.Validate(
            () => enumerable != null && enumerable.GetEnumerator().MoveNext() is true,
            errorMessage: $"{this.FieldName} does not contain any elements.");

        return this;
    }
}
