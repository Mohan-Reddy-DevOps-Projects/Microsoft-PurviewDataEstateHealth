// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for BusinessDomainEntity to BusinessDomainModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(IBusinessDomainModel), typeof(BusinessDomainEntity))]
internal class BusinessDomainEntityAdapter : BaseModelAdapter<IBusinessDomainModel, BusinessDomainEntity>
{
    public override BusinessDomainEntity FromModel(IBusinessDomainModel model)
    {
        return new BusinessDomainEntity()
        {
           BusinessDomainName = model.BusinessDomainName,
           BusinessDomainId = model.BusinessDomainId
        };
    }

    public override IBusinessDomainModel ToModel(BusinessDomainEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new BusinessDomainModel()
        {
           BusinessDomainId = entity.BusinessDomainId,
           BusinessDomainName = entity.BusinessDomainName,
        };
    }
}
