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
[Component(typeof(IDataEstateHealthSummaryComponent), ServiceVersion.V1)]
internal class DataEstateHealthSummaryComponent : BaseComponent<IDataEstateHealthSummaryContext>, IDataEstateHealthSummaryComponent
{
    [Inject]
    private IDataEstateHealthSummaryRepository dataEstateHealthSummaryRepository;

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
           new SummaryKey(this.Context.DomainId),
           cancellationToken);

        if (dataEstateHealthSummaryModel == null)
        {
            throw new ServiceError(
                    ErrorCategory.ResourceNotFound,
                    ErrorCode.BusinessDomainSummary_NotAvailable.Code,
                    ErrorCode.BusinessDomainSummary_NotAvailable.FormatMessage(this.Context.DomainId.ToString()))
                .ToException();
        }

        return dataEstateHealthSummaryModel;
    }
}

