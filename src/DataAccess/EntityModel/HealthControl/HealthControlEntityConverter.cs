// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json.Linq;

/// JSON converter for <see cref="HealthControlEntityConverter"/> objects.
internal class HealthControlEntityConverter : JsonCreationConverter<HealthControlEntity>
{
    protected override HealthControlEntity Create(Type objectType, JObject jObject)
    {
        string resourceKind = jObject["kind"]?.Value<string>();

        if (resourceKind != null)
        {
            if (Enum.TryParse(resourceKind, true, out HealthControlKind controlKind))
            {
                return HealthControlEntityRegistry.Instance.CreateHealthControlEntity(controlKind);
            }
        }

        throw new ServiceError(
                ErrorCategory.InputError,
                ErrorCode.MissingField.Code,
                ErrorCode.MissingField.FormatMessage("Health control Kind"))
            .ToException();
    }
}
