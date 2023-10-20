// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.Errors;

/// <summary>
/// Handles health control model and resource conversions.
/// </summary>
[ModelAdapter(typeof(IHealthControlModel<Models.HealthControlProperties>), typeof(HealthControl))]
public class HealthControlAdapter : BaseModelAdapter<IHealthControlModel<Models.HealthControlProperties>, HealthControl>
{
    /// <inheritdoc />
    public override IHealthControlModel<Models.HealthControlProperties> ToModel(HealthControl resource)
    {
        if (resource == null)
        {
            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.MissingField.Code,
                    ErrorCode.MissingField.FormatMessage("HealthControl information"))
                .ToException();
        }

        IHealthControlModel<Models.HealthControlProperties> healthControlModel =
            HealthControlModelRegistry.Instance.CreateHealthControlModelFor(resource.Kind.ToModel());

        // use the specific health control adapter
        healthControlModel = this.Builder.AdapterFor<IHealthControlModel<Models.HealthControlProperties>, HealthControl>(
                healthControlModel.GetType(),
                resource.GetType())
            .ToModel<IHealthControlModel<Models.HealthControlProperties>>(resource);

        return healthControlModel;
    }

    /// <inheritdoc />
    public override HealthControl FromModel(IHealthControlModel<Models.HealthControlProperties> model)
    {
        // use the specific health control adapter to build the resource
        dynamic adapter =
            this.Builder.AdapterWhereOtherIsKindOf<IHealthControlModel<Models.HealthControlProperties>, HealthControl>(model.GetType());
        return adapter.FromModel((dynamic)model) as HealthControl;
    }
}
