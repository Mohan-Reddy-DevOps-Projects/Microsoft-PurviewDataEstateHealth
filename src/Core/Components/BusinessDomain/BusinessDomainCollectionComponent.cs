// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using global::Microsoft.Azure.Purview.DataEstateHealth.Common;
using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.BaseModels;
using global::Microsoft.DGP.ServiceBasics.Components;
using global::Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

[Component(typeof(IBusinessDomainCollectionComponent), ServiceVersion.V1)]
internal class BusinessDomainCollectionComponent : BaseComponent<IBusinessDomainListContext>, IBusinessDomainCollectionComponent
{
    [Inject]
    private IBusinessDomainRepository businessDomainRepository;

    public BusinessDomainCollectionComponent(IBusinessDomainListContext context, int version) : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
        this.businessDomainRepository = this.businessDomainRepository.ByLocation(this.Context.Location);
    }

    public async Task<IBatchResults<IBusinessDomainModel>> Get(CancellationToken cancellationToken,
        string skipToken = null)
    {
        return await this.businessDomainRepository.GetMultiple(
             cancellationToken,
             skipToken);
    }
}
