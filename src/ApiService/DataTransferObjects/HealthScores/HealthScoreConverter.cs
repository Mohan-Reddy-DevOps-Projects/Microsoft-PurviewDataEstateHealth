// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json.Linq;

public abstract partial class HealthScore
{
    private class HealthScoreConverter : JsonCreationConverter<HealthScore>
    {
        protected override HealthScore Create(Type objectType, JObject jObject)
        {
            string resourceKind = jObject["kind"]?.Value<string>();

            if (resourceKind != null)
            {
                if (Enum.TryParse(resourceKind, true, out HealthScoreKind scoreKind))
                {
                    switch (scoreKind)
                    {
                        case HealthScoreKind.DataGovernance:
                            return new DataGovernanceScore();
                        case HealthScoreKind.DataCuration:
                            return new DataCurationScore();
                        case HealthScoreKind.DataQuality:
                            return new DataQualityScore();
                    }
                }

                throw new ServiceError(
                           ErrorCategory.InputError,
                           ErrorCode.HealthScore_InvalidKind.Code,
                           FormattableString.Invariant(
                               $"Invalid health score kind."))
                       .ToException();
            }

            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.MissingField.Code,
                    ErrorCode.MissingField.FormatMessage("Health score kind"))
                .ToException();
        }
    }
}
