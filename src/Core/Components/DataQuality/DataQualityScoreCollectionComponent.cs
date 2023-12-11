// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;

[Component(typeof(IDataQualityScoreCollectionComponent), ServiceVersion.V1)]
internal class DataQualityScoreCollectionComponent :
    BaseComponent<IDataQualityScoreContext>,
    IDataQualityScoreCollectionComponent
{
#pragma warning disable 649
    [Inject]
    protected readonly IComponentContextFactory contextFactory;

    [Inject]
    private IHealthScoreRepository healthScoreRepository;

    [Inject]
    private readonly IRequestHeaderContext requestHeaderContext;
#pragma warning restore 649

    public DataQualityScoreCollectionComponent(IDataQualityScoreContext context, int version) : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
        this.healthScoreRepository = this.healthScoreRepository.ByLocation(this.Context.Location);
    }

    public IDataQualityScoreCollectionComponent ById(Guid id)
    {
        this.Context.BusinessDomainId = id;
        return this;
    }

    /// <inheritdoc />
    public async Task<IBatchResults<IHealthScoreModel<HealthScoreProperties>>> Get(
        CancellationToken cancellationToken,
        string skipToken = null)
    {
        IBatchResults<IHealthScoreModel<HealthScoreProperties>> actionResults = await this.healthScoreRepository.GetMultiple(
            new HealthScoreKey(this.Context.BusinessDomainId, this.Context.AccountId, new Guid(this.requestHeaderContext.CatalogId)),
            cancellationToken, skipToken);

        if (actionResults == null)
        {
            throw new ServiceError(
               ErrorCategory.ResourceNotFound,
               ErrorCode.HealthScores_NotAvailable.Code,
               ErrorCode.HealthScores_NotAvailable.FormatMessage("Data quality not available for all business domains."))
           .ToException();
        }

        return actionResults;
    }
}
