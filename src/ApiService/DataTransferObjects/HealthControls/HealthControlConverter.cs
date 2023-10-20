// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json.Linq;

public abstract partial class HealthControl
{
    private class HealthControlConverter : JsonCreationConverter<HealthControl>
    {
        protected override HealthControl Create(Type objectType, JObject jObject)
        {
            string resourceKind = jObject["kind"]?.Value<string>();

            if (resourceKind != null)
            {
                if (Enum.TryParse(resourceKind, true, out HealthControlKind scoreKind))
                {
                    switch (scoreKind)
                    {
                        case HealthControlKind.DataGovernance:
                            return new DataGovernanceHealthControl();
                        case HealthControlKind.DataQuality:
                            return new DataQualityHealthControl();
                    }
                }

                throw new ServiceError(
                           ErrorCategory.InputError,
                           ErrorCode.HealthControl_InvalidKind.Code,
                           FormattableString.Invariant(
                               $"Invalid health control kind."))
                       .ToException();
            }

            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.MissingField.Code,
                    ErrorCode.MissingField.FormatMessage("Health control kind"))
                .ToException();
        }
    }
}
