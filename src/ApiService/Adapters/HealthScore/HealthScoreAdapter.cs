// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.Errors;

/// <summary>
/// Handles health score model and resource conversions.
/// </summary>
[ModelAdapter(typeof(IHealthScoreModel<HealthScoreProperties>), typeof(HealthScore))]
public class HealthScoreAdapter : BaseModelAdapter<IHealthScoreModel<HealthScoreProperties>, HealthScore>
{
    /// <inheritdoc />
    public override IHealthScoreModel<HealthScoreProperties> ToModel(HealthScore resource)
    {
        if (resource == null)
        {
            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.MissingField.Code,
                    ErrorCode.MissingField.FormatMessage("HealthScore information"))
                .ToException();
        }

        IHealthScoreModel<HealthScoreProperties> healthScoreModel =
            HealthScoreModelRegistry.Instance.CreateHealthScoreModelFor(resource.ScoreKind.ToModel());

        // use the specific health score adapter
        healthScoreModel = this.Builder.AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScore>(
                healthScoreModel.GetType(),
                resource.GetType())
            .ToModel<IHealthScoreModel<HealthScoreProperties>>(resource);

        return healthScoreModel;
    }

    /// <inheritdoc />
    public override HealthScore FromModel(IHealthScoreModel<HealthScoreProperties> model)
    {
        // use the specific health score adapter to build the resource
        dynamic adapter =
            this.Builder.AdapterWhereOtherIsKindOf<IHealthScoreModel<HealthScoreProperties>, HealthScore>(model.GetType());
        return adapter.FromModel((dynamic)model) as HealthScore;
    }
}
