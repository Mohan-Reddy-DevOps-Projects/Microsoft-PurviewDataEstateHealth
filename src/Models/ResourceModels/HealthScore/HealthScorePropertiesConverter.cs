// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using global::Microsoft.Azure.Purview.DataEstateHealth.Common;
using global::Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json.Linq;

/// <summary>
/// JSON converter for <see cref="HealthScoreProperties" /> objects.
/// </summary>
public class HealthScorePropertiesConverter : JsonCreationConverter<HealthScoreProperties>
{
    /// <inheritdoc />
    protected override HealthScoreProperties Create(Type objectType, JObject jObject)
    {
        string resourceKind = jObject["scoreKind"]?.Value<string>() ?? jObject["scoreKind"]?.Value<string>();

        if (resourceKind != null)
        {
            if (Enum.TryParse(resourceKind, true, out HealthScoreKind scoreKind))
            {
                return HealthScoreModelRegistry.Instance.CreateHealthScoreModelFor(scoreKind).Properties;
            }

            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.HealthScore_InvalidKind.Code,
                    ErrorCode.HealthScore_InvalidKind.FormatMessage(resourceKind))
                .ToException();
        }

        throw new ServiceError(
                ErrorCategory.InputError,
                ErrorCode.MissingField.Code,
                ErrorCode.MissingField.FormatMessage("Invalid health score kind."))
            .ToException();
    }
}
