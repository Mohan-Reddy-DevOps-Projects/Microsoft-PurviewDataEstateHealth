// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json.Linq;

/// JSON converter for <see cref="HealthScoreEntityConverter"/> objects.
internal class HealthScoreEntityConverter : JsonCreationConverter<HealthScoreEntity>
{
    protected override HealthScoreEntity Create(Type objectType, JObject jObject)
    {
        string resourceKind = jObject["scoreKind"]?.Value<string>();

        if (resourceKind != null)
        {
            if (Enum.TryParse(resourceKind, true, out HealthScoreKind scoreKind))
            {
                return HealthScoreEntityRegistry.Instance.CreateHealthScoreEntity(scoreKind);
            }

            //throw 
        }

        throw new ServiceError(
                ErrorCategory.InputError,
                ErrorCode.MissingField.Code,
                ErrorCode.MissingField.FormatMessage("Health Score Kind"))
            .ToException();
    }
}
