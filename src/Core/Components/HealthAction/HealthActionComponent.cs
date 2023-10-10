// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;

/// <inheritdoc />
[Component(typeof(IHealthActionComponent), ServiceVersion.V1)]
internal class HealthActionComponent : BaseComponent<IHealthActionContext>, IHealthActionComponent
{
    [Inject]
    private IHealthActionRepository healthActionRepository;

    public HealthActionComponent(IHealthActionContext context, int version) : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
        this.healthActionRepository = this.healthActionRepository.ByLocation(this.Context.Location);
    }

    /// <inheritdoc />
    public async Task<IHealthActionModel> Get(CancellationToken cancellationToken)
    {
        IHealthActionModel healthActionModel = await this.healthActionRepository.GetSingle(
           new HealthActionKey(this.Context.BusinessDomainId),
           cancellationToken);

        if (healthActionModel == null)
        {
            throw new ServiceError(
                    ErrorCategory.ResourceNotFound,
                    ErrorCode.HealthActions_NotAvailable.Code,
                    ErrorCode.HealthActions_NotAvailable.FormatMessage(this.Context.BusinessDomainId.ToString()))
                .ToException();
        }

        return healthActionModel;
    }
}
