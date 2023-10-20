// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;

[Component(typeof(IHealthControlCollectionComponent), ServiceVersion.V1)]
internal class HealthControlCollectionComponent : BaseComponent<IHealthControlListContext>, IHealthControlCollectionComponent
{
#pragma warning disable 649
    [Inject]
    protected readonly IComponentContextFactory contextFactory;

    [Inject]
    private IHealthControlRepository healthControlRepository;
#pragma warning restore 649

    public HealthControlCollectionComponent(IHealthControlListContext context, int version) : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
        this.healthControlRepository = this.healthControlRepository.ByLocation(this.Context.Location);
    }

    /// <inheritdoc />
    public async Task<IBatchResults<IHealthControlModel<HealthControlProperties>>> Get(
        CancellationToken cancellationToken,
        string skipToken = null)
    {
         return await this.healthControlRepository.GetMultiple(
             cancellationToken,
             skipToken);
    }
}
