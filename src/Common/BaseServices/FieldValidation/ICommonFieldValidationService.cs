// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using Microsoft.DGP.ServiceBasics.Services.FieldValidation;

/// <summary>
/// Provides standardized field validation capabilities.
/// </summary>

public interface ICommonFieldValidationService : IFieldValidationService
{
    /// <summary>
    /// Scopes validation for the given field.
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="fieldValue">The value of the field.</param>
    /// <returns>ICommonValidationOperations</returns>
    public new ICommonValidationOperations<TField> For<TField>(string fieldName, TField fieldValue);
}
