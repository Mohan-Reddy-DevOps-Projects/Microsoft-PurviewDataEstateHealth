// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Business Domain Adapter
/// </summary>
[ModelAdapter(typeof(IBusinessDomainModel), typeof(BusinessDomain))]
public class BusinessDomainAdapter : BaseModelAdapter<IBusinessDomainModel, BusinessDomain>
{
    /// <inheritdoc />
    public override BusinessDomain FromModel(IBusinessDomainModel model)
    {
        return new BusinessDomain
        {
            BusinessDomainName = model.BusinessDomainName,
            BusinessDomainId = model.BusinessDomainId,
        };
    }

    /// <inheritdoc />
    public override IBusinessDomainModel ToModel(BusinessDomain businessDomainDto)
    {
        return new BusinessDomainModel
        {
          BusinessDomainId = businessDomainDto.BusinessDomainId,
          BusinessDomainName = businessDomainDto.BusinessDomainName, 
        };
    }
}
