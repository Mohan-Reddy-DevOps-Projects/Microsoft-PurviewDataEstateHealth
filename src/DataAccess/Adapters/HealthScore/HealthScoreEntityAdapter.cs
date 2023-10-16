// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.Errors;

[ModelAdapter(typeof(IHealthScoreModel<HealthScoreProperties>), typeof(HealthScoreEntity))]
internal class HealthScoreEntityAdapter : BaseModelAdapter<IHealthScoreModel<HealthScoreProperties>, HealthScoreEntity>
{
    public override HealthScoreEntity FromModel(IHealthScoreModel<HealthScoreProperties> model)
    {
        return this.Builder.AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScoreEntity>(
            model.GetType(),
            HealthScoreEntityRegistry.Instance.CreateHealthScoreEntity(model.Properties.ScoreKind).GetType())
            .FromModel<HealthScoreEntity>(model);
    }

    public override IHealthScoreModel<HealthScoreProperties> ToModel(HealthScoreEntity entity)
    {
        if (entity == null)
        {
            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.MissingField.Code,
                    ErrorCode.MissingField.FormatMessage("HealthScore entity"))
                .ToException();
        }

        return this.Builder.AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScoreEntity>(
            HealthScoreModelRegistry.Instance.CreateHealthScoreModelFor(entity.ScoreKind).GetType(),
            entity.GetType())
            .ToModel<IHealthScoreModel<HealthScoreProperties>>(entity);
    }
}
