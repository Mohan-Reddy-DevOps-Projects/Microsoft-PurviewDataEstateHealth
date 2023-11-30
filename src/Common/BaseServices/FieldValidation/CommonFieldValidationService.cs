// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using Microsoft.DGP.ServiceBasics.Services.FieldValidation;

/// <inheritdoc/>
public class CommonFieldValidationService : FieldValidationService, ICommonFieldValidationService
{
    /// <inheritdoc/>
    public new ICommonValidationOperations<TField> For<TField>(string fieldName, TField fieldValue)
    {
        return new CommonValidationOperations<TField>(fieldName, fieldValue);
    }
}
