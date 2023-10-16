// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Errors;
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

    public IHealthActionCollectionComponent ById(Guid id)
    {
        this.Context.BusinessDomainId = id;
        return this;
    }

    /// <inheritdoc />
    public async Task<IBatchResults<IHealthActionModel>> Get(
        CancellationToken cancellationToken,
        string skipToken = null)
    {
        IBatchResults<IHealthActionModel> actionResults = null;
        if (!this.Context.BusinessDomainId.HasValue)
        {
            actionResults = await this.healthActionRepository.GetMultiple(
            cancellationToken,
            skipToken);

            if (actionResults == null)
            {
                throw new ServiceError(
                   ErrorCategory.ResourceNotFound,
                   ErrorCode.HealthActions_NotAvailable.Code,
                   ErrorCode.HealthActions_NotAvailable.FormatMessage("Health actions not available for all business domains."))
               .ToException();
            }
        }
        else
        {
            actionResults = await this.healthActionRepository.GetMultiple(new HealthActionKey(this.Context.BusinessDomainId.Value),
                cancellationToken,
                skipToken);

            if (actionResults == null)
            {
                throw new ServiceError(
                   ErrorCategory.ResourceNotFound,
                   ErrorCode.HealthActions_NotAvailable.Code,
                   ErrorCode.HealthActions_NotAvailable.FormatMessage(this.Context.BusinessDomainId.ToString()))
               .ToException();
            }
        }
        return actionResults;
    }
}
