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

[Component(typeof(IHealthScoreCollectionComponent), ServiceVersion.V1)]
internal class HealthScoreCollectionComponent : BaseComponent<IHealthScoreListContext>, IHealthScoreCollectionComponent
{
#pragma warning disable 649
    [Inject]
    protected readonly IComponentContextFactory contextFactory;

    [Inject]
    private IHealthScoreRepository healthScoreRepository;

    [Inject]
    private readonly IRequestHeaderContext requestHeaderContext;
#pragma warning restore 649

    public HealthScoreCollectionComponent(IHealthScoreListContext context, int version) : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
        this.healthScoreRepository = this.healthScoreRepository.ByLocation(this.Context.Location);
    }

    public IHealthScoreCollectionComponent ById(Guid id)
    {
        this.Context.BusinessDomainId = id;
        return this;
    }

    /// <inheritdoc />
    public async Task<IBatchResults<IHealthScoreModel<HealthScoreProperties>>> Get(
        CancellationToken cancellationToken,
        string skipToken = null)
    {
        IBatchResults<IHealthScoreModel<HealthScoreProperties>> scoreResults = await this.healthScoreRepository.GetMultiple(
             new HealthScoreKey(this.Context.BusinessDomainId, this.Context.AccountId, new Guid(this.requestHeaderContext.CatalogId)),
             cancellationToken, skipToken);

        return scoreResults;
    }
}
