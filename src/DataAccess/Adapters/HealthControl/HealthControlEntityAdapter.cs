// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.Errors;

[ModelAdapter(typeof(IHealthControlModel<HealthControlProperties>), typeof(HealthControlEntity))]
internal class HealthControlEntityAdapter : BaseModelAdapter<IHealthControlModel<HealthControlProperties>, HealthControlEntity>
{
    public override HealthControlEntity FromModel(IHealthControlModel<HealthControlProperties> model)
    {
        return this.Builder.AdapterFor<IHealthControlModel<HealthControlProperties>, HealthControlEntity>(
            model.GetType(),
            HealthControlEntityRegistry.Instance.CreateHealthControlEntity(model.Properties.Kind).GetType())
            .FromModel<HealthControlEntity>(model);
    }

    public override IHealthControlModel<HealthControlProperties> ToModel(HealthControlEntity entity)
    {
        if (entity == null)
        {
            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.MissingField.Code,
                    ErrorCode.MissingField.FormatMessage("HealthControl entity"))
                .ToException();
        }

        return this.Builder.AdapterFor<IHealthControlModel<HealthControlProperties>, HealthControlEntity>(
            HealthControlModelRegistry.Instance.CreateHealthControlModelFor(entity.Kind).GetType(),
            entity.GetType())
            .ToModel<IHealthControlModel<HealthControlProperties>>(entity);
    }
}
