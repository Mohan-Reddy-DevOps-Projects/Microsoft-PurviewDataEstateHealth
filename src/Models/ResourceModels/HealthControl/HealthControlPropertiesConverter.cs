// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json.Linq;

/// <summary>
/// JSON converter for <see cref="HealthControlProperties" /> objects.
/// </summary>
public class HealthControlPropertiesConverter : JsonCreationConverter<HealthControlProperties>
{
    /// <inheritdoc />
    protected override HealthControlProperties Create(Type objectType, JObject jObject)
    {
        string resourceKind = jObject["controlKind"]?.Value<string>() ?? jObject["controlKind"]?.Value<string>();

        if (resourceKind != null)
        {
            if (Enum.TryParse(resourceKind, true, out HealthControlKind controlKind))
            {
                return HealthControlModelRegistry.Instance.CreateHealthControlModelFor(controlKind).Properties;
            }

            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.HealthControl_InvalidKind.Code,
                    ErrorCode.HealthControl_InvalidKind.FormatMessage(resourceKind))
                .ToException();
        }

        throw new ServiceError(
                ErrorCategory.InputError,
                ErrorCode.MissingField.Code,
                ErrorCode.MissingField.FormatMessage("Invalid health control kind."))
            .ToException();
    }
}
