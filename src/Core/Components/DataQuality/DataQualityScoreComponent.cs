// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;

[Component(typeof(IDataQualityScoreComponent), ServiceVersion.V1)]
internal class DataQualityScoreComponent : BaseComponent<IDataQualityScoreContext>, IDataQualityScoreComponent
{
#pragma warning disable 649
    [Inject]
    protected readonly IComponentContextFactory contextFactory;

    [Inject]
    private IHealthScoreRepository healthScoreRepository;

    [Inject]
    private readonly IRequestHeaderContext requestHeaderContext;
#pragma warning restore 649

    public DataQualityScoreComponent(DataQualityScoreContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc />
    public async Task<IHealthScoreModel<HealthScoreProperties>> Get(Guid businessDomain, CancellationToken cancellationToken)
    {
        return await this.healthScoreRepository.GetSingleOrDefault(
            new HealthScoreKey(this.Context.BusinessDomainId, this.Context.AccountId, new Guid(this.requestHeaderContext.CatalogId)));
    }
}
