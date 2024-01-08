// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;

/// <inheritdoc />
[Component(typeof(IDataEstateHealthSummaryComponent), ServiceVersion.V1)]
internal class DataEstateHealthSummaryComponent : BaseComponent<IDataEstateHealthSummaryContext>, IDataEstateHealthSummaryComponent
{
#pragma warning disable 649
    [Inject]
    private IDataEstateHealthSummaryRepository dataEstateHealthSummaryRepository;

    [Inject]
    private readonly IRequestHeaderContext requestHeaderContext;
#pragma warning disable 649

    public DataEstateHealthSummaryComponent(DataEstateHealthSummaryContext context, int version) : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
        this.dataEstateHealthSummaryRepository = this.dataEstateHealthSummaryRepository.ByLocation(this.Context.Location);
    }

    public IDataEstateHealthSummaryComponent ById(Guid domainId)
    {
        this.Context.DomainId = domainId;
        return this;
    }

    /// <inheritdoc />
    public async Task<IDataEstateHealthSummaryModel> Get(CancellationToken cancellationToken)
    {
        IDataEstateHealthSummaryModel dataEstateHealthSummaryModel = await this.dataEstateHealthSummaryRepository.GetSingle(
           new SummaryKey(this.Context.DomainId, this.Context.AccountId, new Guid(this.requestHeaderContext.CatalogId)),
           cancellationToken);

        if (dataEstateHealthSummaryModel == null)
        {
            return new DataEstateHealthSummaryModel()
            {
                BusinessDomainsSummaryModel = null,
                DataAssetsSummaryModel = null,
                DataProductsSummaryModel = null,
                HealthActionsSummaryModel = null
            };
        }

        return dataEstateHealthSummaryModel;
    }
}
