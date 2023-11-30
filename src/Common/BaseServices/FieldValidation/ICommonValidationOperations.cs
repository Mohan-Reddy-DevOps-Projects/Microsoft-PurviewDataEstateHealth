// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using Microsoft.DGP.ServiceBasics.Services.FieldValidation;

/// <inheritdoc />
public interface ICommonValidationOperations<TField> : IValidationOperations<TField>
{
    /// <summary>
    /// Ensures an Enumerable is not empty.
    /// </summary>
    /// <returns>An instance of <see cref="ICommonValidationOperations{TField}"/>.</returns>
    ICommonValidationOperations<TField> IsNonEmptyEnumerable();

    /// <summary>
    /// Ensures a list is not empty.
    /// </summary>
    /// <returns>An instance of <see cref="ICommonValidationOperations{TField}"/>.</returns>
    public ICommonValidationOperations<TField> IsNonEmptyList();
}
