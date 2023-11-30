// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;

/// <inheritdoc />
[Component(typeof(IHealthTrendComponent), ServiceVersion.V1)]
internal class HealthTrendComponent : BaseComponent<IHealthTrendContext>, IHealthTrendComponent
{
#pragma warning disable 649
    [Inject]
    private IHealthTrendRepository healthTrendRepository;

    [Inject]
    private readonly IRequestHeaderContext requestHeaderContext;
#pragma warning disable 649

    public HealthTrendComponent(HealthTrendContext context, int version) : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
        this.healthTrendRepository = this.healthTrendRepository.ByLocation(this.Context.Location);
    }

    public IHealthTrendComponent ById(Guid domainId)
    {
        this.Context.DomainId = domainId;
        return this;
    }

    /// <inheritdoc />
    public async Task<IHealthTrendModel> Get(TrendKind trendKind, CancellationToken cancellationToken)
    {
        IHealthTrendModel healthTrendModel = await this.healthTrendRepository.GetSingle(
           new HealthTrendKey(this.Context.DomainId, this.Context.AccountId, new Guid(this.requestHeaderContext.CatalogId), trendKind),
           cancellationToken);

        if (healthTrendModel == null)
        {
            throw new ServiceError(
                    ErrorCategory.ResourceNotFound,
                    ErrorCode.HealthTrends_NotAvailable.Code,
                    ErrorCode.HealthTrends_NotAvailable.FormatMessage(this.Context.DomainId.ToString()))
                .ToException();
        }

        return healthTrendModel;
    }
}
