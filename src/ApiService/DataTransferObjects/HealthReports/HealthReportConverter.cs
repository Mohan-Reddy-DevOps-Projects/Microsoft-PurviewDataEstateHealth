// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json.Linq;

public abstract partial class HealthReport
{
    private class HealthReportConverter : JsonCreationConverter<HealthReport>
    {
        protected override HealthReport Create(Type objectType, JObject jObject)
        {
            string resourceKind = jObject["kind"]?.Value<string>();

            if (resourceKind != null)
            {
                if (Enum.TryParse(resourceKind, true, out HealthReportKind reportKind))
                {
                    switch (reportKind)
                    {
                        case HealthReportKind.PowerBIHealthReport:
                            return new PowerBIHealthReport();
                        case HealthReportKind.LegacyHealthReport:
                            return new LegacyHealthReport();
                    }
                }

                throw new ServiceError(
                           ErrorCategory.InputError,
                           ErrorCode.HealthReport_InvalidKind.Code,
                           FormattableString.Invariant(
                               $"Invalid health report kind."))
                       .ToException();
            }

            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.MissingField.Code,
                    ErrorCode.MissingField.FormatMessage("Health report Kind"))
                .ToException();
        }
    }
}
