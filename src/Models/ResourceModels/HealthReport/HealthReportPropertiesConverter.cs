// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using global::Microsoft.Azure.Purview.DataEstateHealth.Common;
using global::Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json.Linq;

/// <summary>
/// JSON converter for <see cref="HealthReportProperties" /> objects.
/// </summary>
public class HealthReportPropertiesConverter : JsonCreationConverter<HealthReportProperties>
{
    /// <inheritdoc />
    protected override HealthReportProperties Create(Type objectType, JObject jObject)
    {
        string resourceKind = jObject["reportKind"]?.Value<string>() ?? jObject["reportKind"]?.Value<string>();

        if (resourceKind != null)
        {
            if (Enum.TryParse(resourceKind, true, out HealthReportKind reportKind))
            {
                return HealthReportModelRegistry.Instance.CreateHealthReportModelFor(reportKind).Properties;
            }

            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.HealthReport_InvalidKind.Code,
                    ErrorCode.HealthReport_InvalidKind.FormatMessage(resourceKind))
                .ToException();
        }

        throw new ServiceError(
                ErrorCategory.InputError,
                ErrorCode.MissingField.Code,
                ErrorCode.MissingField.FormatMessage("Invalid health report kind."))
            .ToException();
    }
}
