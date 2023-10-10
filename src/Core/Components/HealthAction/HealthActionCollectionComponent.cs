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

[Component(typeof(IHealthActionCollectionComponent), ServiceVersion.V1)]
internal class HealthActionCollectionComponent : BaseComponent<IHealthActionListContext>, IHealthActionCollectionComponent
{
#pragma warning disable 649
    [Inject]
    protected readonly IComponentContextFactory contextFactory;

    [Inject]
    private IHealthActionRepository healthActionRepository;
#pragma warning restore 649

    public HealthActionCollectionComponent(IHealthActionListContext context, int version) : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
        this.healthActionRepository = this.healthActionRepository.ByLocation(this.Context.Location);
    }

    public IHealthActionComponent ById(Guid id)
    {
        return this.ComponentRuntime.ResolveLatest<IHealthActionComponent, IHealthActionContext>(
          this.contextFactory.CreateHealthActionContext(
              this.Context.Version,
              this.Context.Location,
              this.Context.AccountId,
              this.Context.TenantId,
              id));
    }

    /// <inheritdoc />
    public async Task<IBatchResults<IHealthActionModel>> Get(
        CancellationToken cancellationToken,
        string skipToken = null)
    {
        return await this.healthActionRepository.GetMultiple(
            cancellationToken,
            skipToken);
    }
}
